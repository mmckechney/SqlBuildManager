# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:ed034a8bf0b24ded0cbbac07e17825d8e9ebfe21e308191d0f7421eaf5ad4664 AS restore
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY SqlBuildManager.RelayProxy/SqlBuildManager.RelayProxy.csproj SqlBuildManager.RelayProxy/packages.lock.json SqlBuildManager.RelayProxy/
RUN dotnet restore SqlBuildManager.RelayProxy/SqlBuildManager.RelayProxy.csproj --locked-mode

FROM restore AS publish
COPY SqlBuildManager.RelayProxy/ SqlBuildManager.RelayProxy/
RUN dotnet publish SqlBuildManager.RelayProxy/SqlBuildManager.RelayProxy.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0@sha256:68d35011fe04a39cca38208d392ed48f2df15653633dca16dbc4582d07342b9f AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "SqlBuildManager.RelayProxy.dll"]
