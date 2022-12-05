#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0.0-jammy-amd64 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0.100-jammy-amd64 AS build
WORKDIR /src


#The goofy "RUN true" is added because of bug: https://github.com/moby/moby/issues/37965 . Can be removed once this is fixed
COPY ["SqlBuildManager.Console/sbm.csproj", "SqlBuildManager.Console/"]
RUN true 
COPY ["SqlBuildManager.Enterprise/SqlBuildManager.Enterprise.csproj", "SqlBuildManager.Enterprise/" ]
RUN true 
COPY ["SqlBuildManager.Interfaces/SqlBuildManager.Interfaces.csproj", "SqlBuildManager.Interfaces/" ]
RUN true 
COPY ["SqlBuildManager.Logging/SqlBuildManager.Logging.csproj", "SqlBuildManager.Logging/" ]
RUN true 
COPY ["SqlSync.Connection/SqlSync.Connection.csproj", "SqlSync.Connection/" ]
RUN true 
COPY ["SqlSync.Constants/SqlSync.Constants.csproj", "SqlSync.Constants/" ]
RUN true 
COPY ["SqlSync.DbInformation/SqlSync.DbInformation.csproj", "SqlSync.DbInformation/" ]
RUN true 
COPY ["SqlSync.ObjectScript/SqlSync.ObjectScript.csproj", "SqlSync.ObjectScript/" ]
RUN true 
COPY ["SqlSync.SqlBuild/SqlSync.SqlBuild.csproj", "SqlSync.SqlBuild/" ]
RUN true 
COPY ["SqlSync.SqlBuild/SqlSync.SqlBuild.csproj", "SqlSync.SqlBuild/SqlSync.SqlBuild.csproj" ]
RUN true 
COPY ["SqlBuildManager.ScriptHandling/SqlBuildManager.ScriptHandling.csproj", "SqlBuildManager.ScriptHandling/"]

RUN true 
RUN dotnet restore "SqlBuildManager.Console/sbm.csproj"


COPY . .
WORKDIR "/src/SqlBuildManager.Console"
RUN dotnet build "sbm.csproj" --configuration Release -f net7.0 -o /app/build -r linux-x64 --self-contained

FROM build AS publish
RUN dotnet publish "sbm.csproj" --configuration Release -f net7.0 -o /app/publish -r linux-x64  --self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
