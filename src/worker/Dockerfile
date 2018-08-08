FROM microsoft/dotnet:2.1-runtime

WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "Worker.dll"]