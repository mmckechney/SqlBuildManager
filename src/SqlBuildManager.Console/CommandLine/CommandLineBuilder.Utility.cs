using System;
using System.CommandLine;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
	{
		private static Option<FileInfo[]> packagesOption = new Option<FileInfo[]>("--packages", "-p") { Description = "One or more SBM packages to get contents for" , Required = true };
		private static Option<bool> withHashOption = new Option<bool>("--withhash", "-w") { Description = "Also include the SHA1 hash of the script files in the package" };
		private static Option<FileInfo> unpackPackageOption = new Option<FileInfo>("--package", "-p") { Description = "Name of the SBM package to unpack" , Required = true };
		private static Option<DateTime?> startDateOption = new Option<DateTime?>("--startdate", "--date") { Description = "Date to start counting messages from (will result in faster retrieval if there are a lot of messages)" };
		private static Option<int> timeoutOption = new Option<int>("--timeout") { Description = "Number of seconds to wait for next event before terminating. Zero (0) will wait indefinitely." };
		private static Option<bool> markdownOption = new Option<bool>("--markdown", "--md") { Description = "Output command list as markdown" };
		
		/// <summary>
		/// List contents and hash of a list of SBM packages
		/// </summary>
		private static Command ListCommand
		{
			get
			{
				var cmd = new Command("list", "List the script contents (order, script name, date added/modified, user info, script ids, script hashes) for SBM packages. (For SBX, just open the XML file!)")
				{
					packagesOption,
					withHashOption
				};
				cmd.SetAction(async (parseResult, ct) => {
					var packages = parseResult.GetValue(packagesOption);
					var withHash = parseResult.GetValue(withHashOption);
					await Worker.ListPackageScripts(packages: packages!, withHash: withHash);
					return 0;
				});
				return cmd;
			}
		}


		internal static Option<string> golddatabaseOption = new Option<string>("--golddatabase", "-gd") { Description = "The \"gold copy\" database that will serve as the model for what the target database should look like", Required = true };
		internal static Option<string> goldserverOption = new Option<string>("--goldserver", "-gs") { Description = "The server that the \"gold copy\" database can be found", Required = true };

		/// <summary>
		/// Sync two databases 
		/// </summary>
		private static Command SynchronizeCommand
		{
			get
			{
				var cmd = new Command("synchronize", "Performs a database synchronization between between --database and --golddatabase. Can only be used for Windows Auth database targets")
				{
					golddatabaseOption,
					goldserverOption,
					databaseRequiredOption,
					serverRequiredOption,
					continueonfailureOption
				};
				DatabaseAuthArgs.ForEach(o => cmd.Add(o));
				cmd.SetAction(async (parseResult, ct) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    await Worker.SyncronizeDatabaseAsync(cmdLine);
				    return 0;
				});
				return cmd;
			}
		}

		/// <summary>
		/// Get SBM run differences between two databases
		/// </summary>
		private static Command GetDifferenceCommand
		{
			get
			{
				
				var cmd = new Command("getdifference", "Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between --database and --golddatabase. Only supports Windows Auth")
				{
					golddatabaseOption,
					goldserverOption,
					databaseRequiredOption,
					serverRequiredOption
				};
				cmd.SetAction((parseResult) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    Worker.GetDifferences(cmdLine);
				    return 0;
				});
				return cmd;
			}
		}

		private static Option<string> directoryOption = new Option<string>("--directory", "--dir") { Description = "Directory containing 1 or more SBX files to package into SBM zip files" , Required = true };
		/// <summary>
		/// Create an SBM package from and SBX and script files
		/// </summary>
		private static Command PackageCommand
		{
			get
			{
				//
				var cmd = new Command("package", "Creates an SBM package from an SBX configuration file and scripts")
				{
					directoryOption
				};
				cmd.SetAction(async (parseResult, ct) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    await Worker.PackageSbxFilesIntoSbmFilesAsync(cmdLine);
				    return 0;
				});
				cmd.Aliases.Add("pack");
				return cmd;
			}
		}

		/// <summary>
		/// Add scripts to an SBM package or SBX project file
		/// </summary>
		private static Command AddScriptsCommand
		{
			get
			{
				//Add
				var cmd = new Command("add", "Adds one or more scripts to an SBM package or SBX project file from a list of scripts")
				{
					outputsbmRequiredOption,
					scriptListOption
				};
				cmd.SetAction(async (parseResult, ct) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    return await Worker.AddScriptsToPackage(cmdLine);
				});
				return cmd;
			}
		}

		/// <summary>
		/// Unpacks an SBM file into its script files and SBX project file
		/// </summary>
		private static Command UnpackCommand
		{
			get
			{
				var cmd = new Command("unpack", "Unpacks an SBM file into its script files and SBX project file.")
				{
					unpackDirectoryOption,
					unpackPackageOption
				};
				cmd.SetAction(async (parseResult, ct) => {
					var directory = parseResult.GetValue(unpackDirectoryOption);
					var package = parseResult.GetValue(unpackPackageOption);
					await Worker.UnpackSbmFile(directory: directory!, package: package!);
					return 0;
				});
				return cmd;
			}
		}

		/// <summary>
		/// Performs a script policy check on the specified SBM package
		/// </summary>
		private static Command PolicyCheckCommand
		{
			get
			{
				var cmd = new Command("policycheck", "Performs a script policy check on the specified SBM package")
				{
					packagenameOption
				};
				cmd.SetAction(async (parseResult, ct) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    await Worker.ExecutePolicyCheck(cmdLine);
				    return 0;
				});
				return cmd;
			}
		}
		
		/// <summary>
		/// Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)
		/// </summary>
		private static Command GetHashCommand
		{
			get
			{
				var cmd = new Command("gethash", "Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)")
				{
					packagenameOption
				};
				cmd.SetAction(async (parseResult, ct) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    await Worker.GetPackageHash(cmdLine);
				    return 0;
				});
				return cmd;
			}
		}
		
		/// <summary>
		/// Generates a backout package (reversing stored procedure and scripted object changes)
		/// </summary>
		private static Command CreateBackoutCommand
		{
			get
			{
				var cmd = new Command("createbackout", "Generates a backout package (reversing stored procedure and scripted object changes)")
				{
					packagenameOption,
					serverRequiredOption,
					databaseRequiredOption
				};
				cmd.AddRange(DatabaseAuthArgs);
				cmd.SetAction(async (parseResult, ct) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    await Worker.CreateBackout(cmdLine);
				    return 0;
				});
				return cmd;
			}
		}

		private static Option utilJobName = new Option<string>("--jobname", "-j") { Description = "Name of job run to query" , Required = true };
		/// <summary>
		/// Retrieve the number of messages currently in a Service Bus Topic Subscription
		/// </summary>
		private static Command QueueUtilityCommand
		{
			get
			{
				var cmd = new Command("queue", "Retrieve the number of messages currently in a Service Bus Topic Subscription")
				{
					utilJobName
				};
				cmd.AddRange(SettingsFileExistingRequiredOptions);
				cmd.SetAction((parseResult) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    Worker.GetQueueMessageCount(cmdLine);
				    return 0;
				});
				return cmd;
			}
		}

		/// <summary>
		/// Retrieve the number of messages in the EventHub for a specific job run.
		/// </summary>
		private static Command EventHubUtilityCommand
		{
			get
			{
				var cmd = new Command("eventhub", "Retrieve the number of messages in the EventHub for a specific job run.")
				{
					utilJobName,
					startDateOption,
					timeoutOption
				};
				cmd.Add(eventhubconnectionOption);
                cmd.Add(storageaccountnameOption);
				cmd.Add(storageaccountkeyOption);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(SettingsFileExistingOptions);
				cmd.SetAction((parseResult) => {
					var cmdLine = CommandLineArgsBinder.Bind(parseResult);
					var stream = parseResult.GetValue(streamEventsOption);
					var timeout = parseResult.GetValue(timeoutOption);
					var startDate = parseResult.GetValue(startDateOption);
					return Worker.GetEventHubEvents(cmdLine: cmdLine, stream: stream, timeout: timeout, startDate: startDate);
				});
				return cmd;
			}
		}

		/// <summary>
		/// Generate an override file from a SQL script. Specify either --scriptfile or --scripttext
		/// </summary>
		private static Command OverrideFromSqlUtilityCommand
		{
			get
			{
				var cmd = new Command("override", "Generate an override file from a SQL script. Specify either --scriptfile or --scripttext.")
				{
					serverOption,
					databaseOption,
					new Option<FileInfo>("--scriptfile") { Description = "Name of the SQL script (with \".sql\" extension) to generate an override file from. Should be in format: \"SELECT <target server>, <target db> ...\"" },
					new Option<string>("--scripttext") { Description = "SQL query to generate an override file from. Should be in format: \"SELECT <target server>, <target db> ...\"" },
					new Option<FileInfo>("--outputfile", "-o") { Description = "Name of the output file to write the override file to. Will always generate file with \".cfg\" extension", Required = true },
					new Option<bool>("--force", "-f") { Description = "Force overwrite of existing output file" },

				};
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(DatabaseAuthArgs);
				cmd.AddRange(IdentityArgumentsForBatch);
				cmd.SetAction((parseResult) => {
				    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
				    var unittest = parseResult.GetValue(unitTestOption);
				    return Worker.GenerateOverrideFileFromSqlScript(cmdLine: cmdLine, force: unittest);
				});
				return cmd;
			}
		}

		private static Command DecryptSettingsFile
		{
			get
			{
                var cmd = new Command("decrypt", "Decrypt a settings file")
                {
                    settingsfileExistingRequiredOption,
                    settingsfileKeyRequiredOption
                };
                cmd.SetAction((parseResult) => {
					var cmdLine = CommandLineArgsBinder.Bind(parseResult);
					var settingsfilekey = parseResult.GetValue(settingsfileKeyRequiredOption);
					Worker.DecryptSettingsFile(cmdLine: cmdLine, settingsfilekey: settingsfilekey!);
					return 0;
				});
				cmd.Hidden = true;
                return cmd;

            }
		}

		/// <summary>
		/// Utility commands for generating override file from SQL statement and interrogating Service Bus and EventHubs
		/// </summary>
		private static Command UtilityCommand
		{
			get
			{
				var cmd = new Command("utility", "Utility commands for generating override file from SQL statement and interrogating Service Bus and EventHubs")
				{
					OverrideFromSqlUtilityCommand,
					QueueUtilityCommand,
					EventHubUtilityCommand,
                    DecryptSettingsFile
                };
				return cmd;
			}
		}

        /// <summary>
        /// Create DACPAC from target database
        /// </summary>
        private static Command DacpacCommand
        {
            get
            {
                var cmd = new Command("dacpac", "Creates a DACPAC file from the target database")
                        {
                            databaseRequiredOption,
                            serverRequiredOption,
                            dacpacOutputOption
                        };

                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.SetAction((parseResult) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return Worker.CreateDacpac(cmdLine);
                });
                return cmd;
            }
        }

		private static Command ShowCommandsCommand
		{
			get
			{
				var cmd = new Command("showcommands", "Creates export of all command and sub-command descriptions")
				{
					markdownOption,
				};
                cmd.SetAction((parseResult) => {
                    var markdown = parseResult.GetValue(markdownOption);
                    return Worker.ShowCommands(markdown);
                });
                return cmd;

            }
		}
    }
}
