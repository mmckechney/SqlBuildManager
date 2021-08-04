#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SqlBuildManager.Console/sbm.csproj", "SqlBuildManager.Console/"]
COPY ["SqlBuildManager.Enterprise/SqlBuildManager.Enterprise.csproj", "SqlBuildManager.Enterprise/" ]
COPY ["SqlBuildManager.Interfaces/SqlBuildManager.Interfaces.csproj", "SqlBuildManager.Interfaces/" ]
COPY ["SqlBuildManager.Logging/SqlBuildManager.Logging.csproj", "SqlBuildManager.Logging/" ]
COPY ["SqlSync.Connection/SqlSync.Connection.csproj", "SqlSync.Connection/" ]
COPY ["SqlSync.Constants/SqlSync.Constants.csproj", "SqlSync.Constants/" ]
COPY ["SqlSync.DbInformation/SqlSync.DbInformation.csproj", "SqlSync.DbInformation/" ]
COPY ["SqlSync.ObjectScript/SqlSync.ObjectScript.csproj", "SqlSync.ObjectScript/" ]
COPY ["SqlSync.SqlBuild/SqlSync.SqlBuild.csproj", "SqlSync.SqlBuild/" ]
COPY ["SqlBuildManager.ScriptHandling/SqlBuildManager.ScriptHandling.csproj", "SqlBuildManager.ScriptHandling/"]
RUN dotnet restore "SqlBuildManager.Console/sbm.csproj"


COPY . .
WORKDIR "/src/SqlBuildManager.Console"
RUN dotnet build "sbm.csproj" --configuration Release -f net5.0 -o /app/build -r linux-x64 

FROM build AS publish
RUN dotnet publish "sbm.csproj" --configuration Release -f net5.0 -o /app/publish -r linux-x64  

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
