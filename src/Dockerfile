# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:ed034a8bf0b24ded0cbbac07e17825d8e9ebfe21e308191d0f7421eaf5ad4664 AS restore
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY SqlBuildManager.Console/sbm.csproj SqlBuildManager.Console/packages.lock.json SqlBuildManager.Console/
COPY SqlBuildManager.Enterprise/SqlBuildManager.Enterprise.csproj SqlBuildManager.Enterprise/packages.lock.json SqlBuildManager.Enterprise/
COPY SqlBuildManager.Interfaces/SqlBuildManager.Interfaces.csproj SqlBuildManager.Interfaces/packages.lock.json SqlBuildManager.Interfaces/
COPY SqlBuildManager.Logging/SqlBuildManager.Logging.csproj SqlBuildManager.Logging/packages.lock.json SqlBuildManager.Logging/
COPY SqlBuildManager.ScriptHandling/SqlBuildManager.ScriptHandling.csproj SqlBuildManager.ScriptHandling/packages.lock.json SqlBuildManager.ScriptHandling/
COPY SqlSync.Connection/SqlSync.Connection.csproj SqlSync.Connection/packages.lock.json SqlSync.Connection/
COPY SqlSync.Constants/SqlSync.Constants.csproj SqlSync.Constants/packages.lock.json SqlSync.Constants/
COPY SqlSync.DbInformation/SqlSync.DbInformation.csproj SqlSync.DbInformation/packages.lock.json SqlSync.DbInformation/
COPY SqlSync.ObjectScript/SqlSync.ObjectScript.csproj SqlSync.ObjectScript/packages.lock.json SqlSync.ObjectScript/
COPY SqlSync.SqlBuild/SqlSync.SqlBuild.csproj SqlSync.SqlBuild/packages.lock.json SqlSync.SqlBuild/
RUN dotnet restore SqlBuildManager.Console/sbm.csproj --locked-mode

FROM restore AS publish
COPY . .
RUN dotnet publish SqlBuildManager.Console/sbm.csproj \
    --configuration Release \
    --framework net10.0 \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0@sha256:ed5d539b27842d656a06a5984dbcb5114d3e885fbada612a49a5a7c3c3a44e1c AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_ENVIRONMENT=Production
