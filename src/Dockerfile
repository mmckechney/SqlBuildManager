#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . .
WORKDIR "/SqlBuildManager.Console"


FROM build AS publish   
RUN dotnet clean --configuration Release -f net8.0 
RUN dotnet nuget locals all --clear
RUN dotnet publish sbm.csproj --configuration Release -f net8.0 -o /app/publish   --self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
