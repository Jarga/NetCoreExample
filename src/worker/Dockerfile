FROM microsoft/aspnetcore

WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "Worker.dll"]