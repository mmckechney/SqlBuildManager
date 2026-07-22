FROM mcr.microsoft.com/powershell:7.5-ubuntu-22.04

ENV ACCEPT_EULA=Y
ENV AZD_PROJECT_PATH=/bootstrap

RUN sed -i 's|http://archive.ubuntu.com|https://archive.ubuntu.com|g; s|http://security.ubuntu.com|https://security.ubuntu.com|g' /etc/apt/sources.list && \
    apt-get update && \
    apt-get install -y ca-certificates curl apt-transport-https gnupg && \
    mkdir -p /etc/apt/keyrings && \
    curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | \
      gpg --dearmor -o /etc/apt/keyrings/microsoft.gpg && \
    chmod go+r /etc/apt/keyrings/microsoft.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/repos/azure-cli/ jammy main" \
      > /etc/apt/sources.list.d/azure-cli.list && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" \
      > /etc/apt/sources.list.d/microsoft-prod.list && \
    apt-get update && \
    apt-get install -y azure-cli msodbcsql18 postgresql-client unixodbc && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

RUN pwsh -NoLogo -NoProfile -Command \
    "Set-PSRepository PSGallery -InstallationPolicy Trusted; Install-Module SqlServer -RequiredVersion 22.3.0 -Scope AllUsers -Force -AllowClobber"

WORKDIR /bootstrap
COPY scripts ./scripts
COPY infra ./infra
COPY run-private-postprovision.ps1 .

ENTRYPOINT ["pwsh", "-NoLogo", "-NoProfile", "-File", "/bootstrap/run-private-postprovision.ps1"]
