FROM microsoft/aspnetcore

RUN apt-get update

# Copy our code to the "/app" folder in our container
WORKDIR /app

# copy csproj and restore as distinct layers
COPY NetCoreExample.csproj ./
RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/NetCoreExample.dll"]