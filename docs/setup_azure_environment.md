# Development and Testing

- [Setting up a test Azure Environment](#setting-up-a-test-azure-environment)
- [Notes on Unit Testing](#notes-on-unit-testing)
- [SQL Express for local testing](#sql-express)
- [Visual Studio Installer Project](#Visual-studio-installer-project)

----

## Setting up a test Azure Environment

### Create Test Target databases

To set up your Azure environment for running tests, please see the instructions and documentation on [Massively Parallel Database Builds](massively_parallel.md). Since you will need test database targets, be sure to set the `-testDatabaseCount` parameter to something greater than zero.

## Notes on Unit Testing

**NOTE: There are currently some concurrency issues with the unit tests. You may get some failures in a full run that will then succeed after running aain, selecting only the failed tests** 

There are two types of Unit Tests included in the solution. Those that are dependent on a local SQLEXPRESS database (~.Dependent.UnitTest.csproj) and those that aren't (~UnitTest.csproj). If you want to be able to run the database dependent tests, you will need to install SQL Express as per the next section

## SQL Express

In order to get some of the unit tests to succeed, you need to have a local install of SQLExpress. You can find the installer from here [https://www.microsoft.com/en-us/sql-server/sql-server-editions-express] (https://www.microsoft.com/en-us/sql-server/sql-server-editions-express). You should be able to leverage the basic install.


# Visual Studio Installer Project
For Visual Studio 2015 and beyond, you will need to install an extension to load the installer project (.vdproj)

### Visual Studio 2015
https://visualstudiogallery.msdn.microsoft.com/f1cc3f3e-c300-40a7-8797-c509fb8933b9

### Visual Studio 2017 and 2019
https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects

_If you are having trouble with the installer project loading try disable extension "Microsoft Visual Studio Installer Projects", reenable, then reload the projects.
