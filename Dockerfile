FROM microsoft/aspnetcore

WORKDIR /app
COPY ./src/app/publish .
ENTRYPOINT ["dotnet", "NetCoreExample.dll"]