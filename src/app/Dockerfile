FROM microsoft/dotnet:2.1-sdk as build-env

COPY ./app/NetCoreExample.csproj /app/
COPY ./shared/Shared.csproj /shared/

WORKDIR /app

RUN dotnet restore

COPY ./app /app
COPY ./shared /shared

RUN dotnet publish -o publish

FROM microsoft/dotnet:2.1-aspnetcore-runtime

WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "NetCoreExample.dll"]