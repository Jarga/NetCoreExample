FROM microsoft/dotnet:2.1-sdk as build-env

COPY ./worker/Worker.csproj /worker/
COPY ./shared/Shared.csproj /shared/

WORKDIR /worker

RUN dotnet restore

COPY ./worker /worker
COPY ./shared /shared

RUN dotnet publish -o publish

FROM microsoft/dotnet:2.1-runtime

WORKDIR /app
COPY --from=build-env /worker/publish .
ENTRYPOINT ["dotnet", "Worker.dll"]