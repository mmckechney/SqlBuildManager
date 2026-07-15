FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SqlBuildManager.StorageProxy/SqlBuildManager.StorageProxy.csproj SqlBuildManager.StorageProxy/
RUN dotnet restore SqlBuildManager.StorageProxy/SqlBuildManager.StorageProxy.csproj
COPY SqlBuildManager.StorageProxy/ SqlBuildManager.StorageProxy/
RUN dotnet publish SqlBuildManager.StorageProxy/SqlBuildManager.StorageProxy.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SqlBuildManager.StorageProxy.dll"]
