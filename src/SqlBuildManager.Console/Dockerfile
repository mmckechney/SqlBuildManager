FROM microsoft/dotnet-framework:4.7.1-windowsservercore-ltsc2016
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["C:\\app\sbm.exe"]
