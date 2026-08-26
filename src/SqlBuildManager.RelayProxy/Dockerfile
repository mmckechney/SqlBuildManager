# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:72dd743782f2ae7e5476fd64f6a460045e3998dc862218b80e6944cba79a01b0 AS restore
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
