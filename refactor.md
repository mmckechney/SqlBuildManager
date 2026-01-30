Great use case for Copilot, Michael—this is exactly the kind of structured refactor it can help with if you give it a clear brief. 💪

You want **one good prompt** that tells GitHub Copilot to:
1. Analyze a folder with Bicep + PowerShell.
2. Move everything to **managed identity** for service-to-service auth (no keys/conn strings).
3. Configure **Azure SQL** for **Entra ID (AAD) only** (no SQL auth / username+password).
4. Reshape things so the deployment runs via **Azure Developer CLI (`azd`)**.

Below is a ready‑to‑paste prompt you can use with GitHub Copilot Chat (in VS Code or GitHub.com). You’ll just need to tweak a couple of folder names to match your repo.

---

## ✅ Final Prompt for GitHub Copilot

> You are an expert Azure cloud and DevOps engineer with deep experience in:
> - Bicep templates for Azure infrastructure  
> - PowerShell deployment automation  
> - Secure-by-default service communication (Managed Identities, Azure RBAC)  
> - Azure SQL with Entra ID (Azure AD) authentication  
> - Azure Developer CLI (azd) project structure and workflows  
>
> I have a repository open in this workspace that contains many Bicep files and PowerShell deployment scripts. Your job is to **analyze and refactor this codebase** to meet the following goals and constraints.
>
> ---
> ## Goal
> Refactor the existing infrastructure and deployment scripts so that:
>
> 1. **All services communicate using Managed Identity**
>    - Remove the use of connection strings, access keys, or shared secrets for service-to-service communication.  
>    - Where applicable (e.g., calling Azure SQL, Storage, Key Vault, Service Bus, etc.), replace secret-based auth with Managed Identity–based auth and Azure RBAC.
>
> 2. **Azure SQL uses Entra ID / Azure AD only**
>    - Update the Bicep files for any SQL servers and databases so they:
>      - Configure **Entra ID (Azure AD) administrator(s)**.  
>      - Enable **Azure AD–only** authentication and **disable SQL authentication** (no usernames/passwords).
>    - Remove any SQL authentication references from Bicep and PowerShell (e.g., `sqlAdminUser`, `sqlAdminPassword`, connection strings with usernames/passwords).
>
> 3. **Deployment is compatible with Azure Developer CLI (`azd`)**
>    - Restructure or extend the repo so that it can be deployed using **Azure Developer CLI**.  
>    - Add or update any required files for `azd`, such as:
>      - `azure.yaml` / `azure.*.yaml` for environment configuration.
>      - A clear infra folder structure (for example, `infra/` with Bicep entry points).
>      - Any `azd`-friendly script entry points (for example, if needed, `./scripts/` or `./infra/main.bicep` as the main template).
>
> ---
> ## Context about the repo
> - Bicep files are primarily located under:  
>   - `.\scripts\templates\Modules`  
> - PowerShell deployment scripts are located under:  
>   - `.\scripts\templates`  
> - Existing deployments likely:
>   - Pass connection strings or keys between resources (e.g., via outputs/parameters or PowerShell variables).  
>   - Use SQL authentication (`sqlAdminUserName`, `sqlAdminPassword`, or connection strings with `User ID`/`Password`).
>
> Please inspect the repository structure and files in this workspace to confirm the actual layout and resource types before you start making suggestions.
>
> ---
> ## Detailed Requirements
>
> ### 1. Managed identity–based communication
> For every place where one Azure service talks to another:
> - **Identify**:
>   - Where connection strings, keys, or client secrets are used in Bicep outputs, parameters, or PowerShell scripts.  
>   - Any references to secrets that might come from Key Vault that *should* be replaced by Managed Identity + RBAC.
> - **Refactor**:
>   - In Bicep:
>     - Ensure resources that need to call other Azure services have a `identity` block, preferably **system-assigned** unless user-assigned identities are already a pattern in this repo.
>     - Add required role assignments via Bicep (`Microsoft.Authorization/roleAssignments`) to enable Managed Identity access to downstream resources (e.g., SQL, Storage, Service Bus, Key Vault).
>   - In PowerShell:
>     - Remove reliance on connection strings where possible.  
>     - Prefer Managed Identity–based authentication flows (e.g., `Connect-AzAccount -Identity` or equivalent MSI-based authentication patterns if scripts run in Azure).
> - **Constraint**:  
>   - Do **not** introduce any new secrets, keys, or hardcoded credentials anywhere in the repo.
>
> ### 2. Azure SQL with Entra ID only
> For every Azure SQL Server and Database defined in Bicep:
> - **Update Bicep** so that:
>   - An **Entra ID (Azure AD) administrator** is configured (using the appropriate `administrators` / `azureADOnlyAuthentication` / equivalent properties depending on resource type and API version).
>   - **SQL authentication is disabled** so users cannot connect with SQL usernames/passwords.
>   - Any parameters/variables for SQL admin username or password are removed, deprecated, or clearly marked as unused.
> - **Clean up scripts**:
>   - Update PowerShell deployment scripts to remove SQL auth–based connection logic and credentials.  
