variable "resource_group" {
  description = "The name of the resource group in which to create the virtual network."
  default     = "Test-Web-EastUS"
}

variable "rg_prefix" {
  description = "The shortened abbreviation to represent your resource group that will go on the front of some resources."
  default     = "rg"
}

variable "hostname" {
  description = "VM name referenced also in storage-related names."
}

variable "dns_name" {
  description = " Label for the Domain Name. Will be used to make up the FQDN. If a domain name label is specified, an A DNS record is created for the public IP in the Microsoft Azure DNS system."
}

variable "lb_ip_dns_name" {
  description = "DNS for Load Balancer IP"
}

variable "location" {
  description = "The location/region where the virtual network is created. Changing this forces a new resource to be created."
  default     = "eastus"
}

variable "virtual_network_name" {
  description = "The name for the virtual network."
  default     = "vnet"
}

variable "address_space" {
  description = "The address space that is used by the virtual network. You can supply more than one address space. Changing this forces a new resource to be created."
  default     = "10.0.0.0/16"
}

variable "subnet_prefix" {
  description = "The address prefix to use for the subnet."
  default     = "10.0.10.0/24"
}

variable "storage_machine_account_tier" {
  description = "The Tier of the storage account in which your existing VHD and image reside (Standard or Premium)"
  default     = "Standard"
}

variable "storage_machine_replication_type" {
  description = "The Replication Type of the storage account in which your existing VHD and image reside (Options include LRS and GRS)"
  default     = "LRS"
}

variable "vm_size" {
  description = "Specifies the size of the virtual machine."
  default     = "Standard_A1"
}

variable "vm_count" {
  description = "number of VMs to create"
  default = 3
}

variable "image_publisher" {
  description = "name of the publisher of the image (az vm image list)"
  default     = "Canonical"
}

variable "image_offer" {
  description = "the name of the offer (az vm image list)"
  default     = "UbuntuServer"
}

variable "image_sku" {
  description = "image sku to apply (az vm image list)"
  default     = "18.04-LTS"
}

variable "image_version" {
  description = "version of the image to apply (az vm image list)"
  default     = "latest"
}

variable "admin_username" {
  description = "administrator user name"
}

variable "admin_password" {
  description = "administrator password (recommended to disable password auth)"
}

variable "lb_ports" {
  description = "Ports to expose in Load Balancer"
  type = "list"
  default = [80, 3000, 8080, 9000, 15672, 27017]
}
