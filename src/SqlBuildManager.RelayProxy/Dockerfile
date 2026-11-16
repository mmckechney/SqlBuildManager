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

FROM mcr.microsoft.com/dotnet/runtime:11.0@sha256:3fe5f5495141c97986a49d3a4e76061f6225c3f93f94e8a7e522781376193863 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "SqlBuildManager.RelayProxy.dll"]
