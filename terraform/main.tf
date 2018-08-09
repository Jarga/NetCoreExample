# Configure the Azure Providers

terraform {
  backend "azurerm" {
    storage_account_name = "smcadamsterraformstate"
    container_name       = "smcadamsterraformstate"
    key                  = "prod.terraform.tfstate"
	  resource_group_name  = "Default-Storage-EastUS"
	  arm_subscription_id  = ""
	  arm_client_id        = ""
	  arm_client_secret    = ""
	  arm_tenant_id        = ""
  }
}

provider "azurerm" {
  subscription_id = ""
  client_id       = ""
  client_secret   = ""
  tenant_id       = ""
}

resource "azurerm_resource_group" "rg" {
  name     = "${var.resource_group}"
  location = "${var.location}"
}

resource "azurerm_network_security_group" "nsg" {
  name                = "SecurityGroup-1"
  location            = "${azurerm_resource_group.rg.location}"
  resource_group_name = "${azurerm_resource_group.rg.name}"
}

resource "azurerm_network_security_rule" "secrule" {
  name                        = "any"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = "*"
  destination_address_prefix  = "*"
  resource_group_name         = "${azurerm_resource_group.rg.name}"
  network_security_group_name = "${azurerm_network_security_group.nsg.name}"
}

resource "azurerm_availability_set" "avset" {
  name                         = "${var.dns_name}avset"
  location                     = "${var.location}"
  resource_group_name          = "${azurerm_resource_group.rg.name}"
  platform_fault_domain_count  = 2
  platform_update_domain_count = 2
  managed                      = true
}

resource "azurerm_public_ip" "lbpip" {
  name                         = "${var.rg_prefix}-ip"
  location                     = "${var.location}"
  resource_group_name          = "${azurerm_resource_group.rg.name}"
  public_ip_address_allocation = "Static"
  domain_name_label            = "${var.lb_ip_dns_name}"
  sku                          = "Standard"
}

resource "azurerm_virtual_network" "vnet" {
  name                = "${var.virtual_network_name}"
  location            = "${var.location}"
  address_space       = ["${var.address_space}"]
  resource_group_name = "${azurerm_resource_group.rg.name}"
}

resource "azurerm_subnet" "subnet" {
  name                 = "${var.rg_prefix}subnet"
  virtual_network_name = "${azurerm_virtual_network.vnet.name}"
  resource_group_name  = "${azurerm_resource_group.rg.name}"
  address_prefix       = "${var.subnet_prefix}"
  network_security_group_id = "${azurerm_network_security_group.nsg.id}"
}

resource "azurerm_lb" "lb" {
  resource_group_name = "${azurerm_resource_group.rg.name}"
  name                = "${var.rg_prefix}lb"
  location            = "${var.location}"
  sku                 = "Standard"

  frontend_ip_configuration {
    name                 = "LoadBalancerFrontEnd"
    public_ip_address_id = "${azurerm_public_ip.lbpip.id}"
  }
}

resource "azurerm_lb_backend_address_pool" "backend_pool" {
  resource_group_name = "${azurerm_resource_group.rg.name}"
  loadbalancer_id     = "${azurerm_lb.lb.id}"
  name                = "BackendPool1"
}

resource "azurerm_lb_nat_rule" "tcp" {
  resource_group_name            = "${azurerm_resource_group.rg.name}"
  loadbalancer_id                = "${azurerm_lb.lb.id}"
  name                           = "SSH-VM-${count.index}"
  protocol                       = "tcp"
  frontend_port                  = "2${count.index + 21}"
  backend_port                   = 22
  frontend_ip_configuration_name = "LoadBalancerFrontEnd"
  count                          = "${var.vm_count}"
}

resource "azurerm_lb_rule" "lb_rule" {
  count                          = "${length(var.lb_ports)}"
  resource_group_name            = "${azurerm_resource_group.rg.name}"
  loadbalancer_id                = "${azurerm_lb.lb.id}"
  name                           = "LBRule-${count.index}"
  protocol                       = "tcp"
  frontend_port                  = "${element(var.lb_ports, count.index)}"
  backend_port                   = "${element(var.lb_ports, count.index)}"
  frontend_ip_configuration_name = "LoadBalancerFrontEnd"
  enable_floating_ip             = false
  backend_address_pool_id        = "${azurerm_lb_backend_address_pool.backend_pool.id}"
  idle_timeout_in_minutes        = 5
  probe_id                       = "${element(azurerm_lb_probe.lb_probe.*.id, count.index)}"
  depends_on                     = ["azurerm_lb_probe.lb_probe"]
}

resource "azurerm_lb_probe" "lb_probe" {
  count               = "${length(var.lb_ports)}"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  loadbalancer_id     = "${azurerm_lb.lb.id}"
  name                = "tcpProbe-${count.index}"
  protocol            = "tcp"
  port                = "${element(var.lb_ports, count.index)}"
  interval_in_seconds = 15
  number_of_probes    = 2
}

resource "azurerm_network_interface" "nic" {
  name                = "nic${count.index}"
  location            = "${var.location}"
  network_security_group_id = "${azurerm_network_security_group.nsg.id}"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  count               = "${var.vm_count}"

  ip_configuration {
    name                                    = "ipconfig${count.index}"
    subnet_id                               = "${azurerm_subnet.subnet.id}"
    private_ip_address_allocation           = "Dynamic"
    load_balancer_backend_address_pools_ids = ["${azurerm_lb_backend_address_pool.backend_pool.id}"]
    load_balancer_inbound_nat_rules_ids     = ["${element(azurerm_lb_nat_rule.tcp.*.id, count.index)}"]
  }
}

resource "tls_private_key" "sshkey" {
  algorithm   = "RSA"
}

resource "azurerm_storage_account" "stor" {
  name                     = "${var.dns_name}stor"
  location                 = "${var.location}"
  resource_group_name      = "${azurerm_resource_group.rg.name}"
  account_tier             = "${var.storage_machine_account_tier}"
  account_replication_type = "${var.storage_machine_replication_type}"
}

resource "azurerm_storage_share" "share" {
  name = "swarmshare"

  resource_group_name  = "${azurerm_resource_group.rg.name}"
  storage_account_name = "${azurerm_storage_account.stor.name}"

  quota = 1
}

resource "azurerm_virtual_machine" "vm" {
  name                  = "vm${count.index}"
  location              = "${var.location}"
  resource_group_name   = "${azurerm_resource_group.rg.name}"
  availability_set_id   = "${azurerm_availability_set.avset.id}"
  vm_size               = "${var.vm_size}"
  network_interface_ids = ["${element(azurerm_network_interface.nic.*.id, count.index)}"]
  delete_os_disk_on_termination = true
  count                 = "${var.vm_count}"

  storage_image_reference {
    publisher = "${var.image_publisher}"
    offer     = "${var.image_offer}"
    sku       = "${var.image_sku}"
    version   = "${var.image_version}"
  }

  storage_os_disk {
    name          = "osdisk${count.index}"
    caching       = "ReadWrite"
    create_option = "FromImage"
  }

  os_profile {
    computer_name  = "${var.hostname}${count.index}"
    admin_username = "${var.admin_username}"
    admin_password = "${var.admin_password}"
  }
  
  os_profile_linux_config {
    disable_password_authentication = false
	ssh_keys = { 
      path = "/home/${var.admin_username}/.ssh/authorized_keys"
	  key_data = "${tls_private_key.sshkey.public_key_openssh}"
    }
  }
}

resource "null_resource" "mount-shares" {
  count = "${var.vm_count}"
  depends_on = ["azurerm_virtual_machine.vm", "azurerm_lb_nat_rule.tcp"]
  
  triggers {
    key = "${uuid()}"
  }

  connection {
      host = "${azurerm_public_ip.lbpip.fqdn}"
      user = "${var.admin_username}"
	  private_key = "${tls_private_key.sshkey.private_key_pem}"
	  port = "2${count.index + 21}"
      type = "ssh"
      timeout = "4m"
	  agent = false
  }

  provisioner "remote-exec" {
    inline = [
      "sudo apt --fix-broken install",
      "sudo apt-get update",
      "sudo apt-get -f -y install",
      "sudo apt-get -y upgrade",
      "sudo apt-get install cifs-utils",
	    "sudo mkdir -p /mnt/swarmdata",
	    "sudo mount -t cifs ${replace(azurerm_storage_share.share.url, "https:", "")} /mnt/swarmdata -o vers=3.0,user=${var.dns_name}stor,password=${azurerm_storage_account.stor.primary_access_key},dir_mode=0777,file_mode=0777,serverino || true",
	    "sudo mkdir -p /mnt/swarmdata/grafana",
      "sudo mkdir -p /mnt/swarmdata/prometheus",
      "sudo mkdir -p /mnt/swarmdata/grafana/data",
      "sudo mkdir -p /mnt/swarmdata/grafana/prometheus"

    ]
  }
}

resource "null_resource" "configure-swarm-manager" {
  count = 1
  depends_on = ["null_resource.mount-shares"]
  
  triggers {
    key = "${uuid()}"
  }

  connection {
      host = "${azurerm_public_ip.lbpip.fqdn}"
      user = "${var.admin_username}"
	  private_key = "${tls_private_key.sshkey.private_key_pem}"
	  port = "2${count.index + 21}"
      type = "ssh"
      timeout = "4m"
	  agent = false
  }

  provisioner "remote-exec" {
    inline = [
      "command -v docker >/dev/null 2>&1 || sudo curl -sSL https://get.docker.com/ | sh",
	  "sudo usermod -aG docker ${var.admin_username}",
	  "echo sudo docker swarm init --advertise-addr ${element(azurerm_network_interface.nic.*.private_ip_address, count.index)}",
	  "sudo docker swarm init --advertise-addr ${element(azurerm_network_interface.nic.*.private_ip_address, count.index)} || true",
	  "sudo docker swarm join-token -q worker > /mnt/swarmdata/workertoken",
	  "sudo docker swarm join-token -q manager > /mnt/swarmdata/managertoken"
    ]
  }
}

resource "null_resource" "configure-add-swarm-managers" {
  count = 2
  depends_on = ["null_resource.configure-swarm-manager"]
  
  triggers {
    key = "${uuid()}"
  }
  
  connection {
    host = "${azurerm_public_ip.lbpip.fqdn}"
    user = "${var.admin_username}"
    private_key = "${tls_private_key.sshkey.private_key_pem}"
    port = "2${count.index + 22}"
    type = "ssh"
    timeout = "4m"
	  agent = false
  }

  provisioner "remote-exec" {
    inline = [
      "command -v docker >/dev/null 2>&1 || sudo curl -sSL https://get.docker.com/ | sh",
	  "sudo usermod -aG docker ${var.admin_username}",
	  "echo sudo docker swarm join --token $(cat /mnt/swarmdata/managertoken) ${azurerm_network_interface.nic.0.private_ip_address}",
	  "sudo docker swarm join --token $(cat /mnt/swarmdata/managertoken) ${azurerm_network_interface.nic.0.private_ip_address} || true"
    ]
  }
}

resource "null_resource" "configure-swarm-agents" {
  count = "${var.vm_count - 3}"
  depends_on = ["null_resource.configure-swarm-manager"]
  
  triggers {
    key = "${uuid()}"
  }
  
  connection {
      host = "${azurerm_public_ip.lbpip.fqdn}"
      user = "${var.admin_username}"
	  private_key = "${tls_private_key.sshkey.private_key_pem}"
	  port = "2${count.index + 24}"
      type = "ssh"
      timeout = "4m"
	  agent = false
  }

  provisioner "remote-exec" {
    inline = [
      "command -v docker >/dev/null 2>&1 || sudo curl -sSL https://get.docker.com/ | sh",
	  "sudo usermod -aG docker ${var.admin_username}",
	  "echo sudo docker swarm join --token $(cat /mnt/swarmdata/workertoken) ${azurerm_network_interface.nic.0.private_ip_address}",
	  "sudo docker swarm join --token $(cat /mnt/swarmdata/workertoken) ${azurerm_network_interface.nic.0.private_ip_address} || true"
    ]
  }
}

resource "null_resource" "run-system-containers" {
  count = 1
  depends_on = ["null_resource.configure-swarm-manager"]
  
  triggers {
    key = "${uuid()}"
  }
  
  connection {
      host = "${azurerm_public_ip.lbpip.fqdn}"
      user = "${var.admin_username}"
	    private_key = "${tls_private_key.sshkey.private_key_pem}"
	    port = "2${count.index + 21}"
      type = "ssh"
      timeout = "4m"
	    agent = false
  }

  # Copies all files over
  provisioner "file" {
    source      = "grafana/"
    destination = "/mnt/swarmdata/grafana"
  }

  provisioner "remote-exec" {
    inline = [
      "sudo docker service create --name=viz --publish=8080:8080/tcp --constraint=node.role==manager --mount=type=bind,src=/var/run/docker.sock,dst=/var/run/docker.sock dockersamples/visualizer || true",
	    "sudo docker service create --name portainer --publish 9000:9000 --constraint 'node.role == manager' --mount type=bind,src=//var/run/docker.sock,dst=/var/run/docker.sock portainer/portainer -H unix:///var/run/docker.sock || true",
      "sudo docker stack deploy -c /mnt/swarmdata/grafana/grafana-compose.yml monitor || true"
    ]
  }
}