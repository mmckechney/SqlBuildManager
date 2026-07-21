FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SqlBuildManager.RelayProxy/SqlBuildManager.RelayProxy.csproj SqlBuildManager.RelayProxy/
RUN dotnet restore SqlBuildManager.RelayProxy/SqlBuildManager.RelayProxy.csproj
COPY SqlBuildManager.RelayProxy/ SqlBuildManager.RelayProxy/
RUN dotnet publish SqlBuildManager.RelayProxy/SqlBuildManager.RelayProxy.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SqlBuildManager.RelayProxy.dll"]
