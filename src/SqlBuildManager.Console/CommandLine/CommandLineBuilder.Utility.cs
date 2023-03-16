using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
	{
		private static Option<FileInfo[]> packagesOption = new Option<FileInfo[]>(new string[] { "-p", "--packages" }, "One or more SBM packages to get contents for") { IsRequired = true }.ExistingOnly();
		private static Option<bool> withHashOption = new Option<bool>(new string[] { "-w", "--withhash" }, () => true, "Also include the SHA1 hash of the script files in the package");
		
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
				cmd.Handler = CommandHandler.Create<FileInfo[], bool>(Worker.ListPackageScripts);
				return cmd;
			}
		}


		private static Option<string> golddatabaseOption = new Option<string>(new string[] { "-gd", "--gd", "--golddatabase" }, "The \"gold copy\" database that will serve as the model for what the target database should look like") { IsRequired = true };
		private static Option<string> goldserverOption = new Option<string>(new string[] { "-gs", "--goldserver" }, "The server that the \"gold copy\" database can be found") { IsRequired = true };

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
					databaseOption.Copy(true),
					serverOption.Copy(true),
					continueonfailureOption
				};
				DatabaseAuthArgs.ForEach(o => cmd.Add(o));
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.SyncronizeDatabase);
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
					databaseOption.Copy(true),
					serverOption.Copy(true)
				};
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.GetDifferences);
				return cmd;
			}
		}

		private static Option<string> directoryOption = new Option<string>(new string[] { "--dir", "--directory" }, "Directory containing 1 or more SBX files to package into SBM zip files") { IsRequired = true };
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
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.PackageSbxFilesIntoSbmFiles);
				cmd.AddAlias("pack");
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
					outputsbmOption.Copy(true),
					scriptListOption
				};
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.AddScriptsToPackage);
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
					new Option<FileInfo>(new string[] { "-p", "--package" }, "Name of the SBM package to unpack") { IsRequired = true }.ExistingOnly()
				};
				cmd.Handler = CommandHandler.Create<DirectoryInfo, FileInfo>(Worker.UnpackSbmFile);
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
					packagenameOption.Copy(true)
				};
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.ExecutePolicyCheck);
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
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.GetPackageHash);
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
					serverOption.Copy(true),
					databaseOption.Copy(true)
				};
				cmd.AddRange(DatabaseAuthArgs);
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreateBackout);
				return cmd;
			}
		}

		private static Option utilJobName = new Option<string>(new string[] { "-j", "--jobname" }, "Name of job run to query") { IsRequired = true };
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
				cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.GetQueueMessageCount);
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
					new Option<DateTime?>(new string[] { "--date" }, "Date to start counting messages from (will result in faster retrieval if there are a lot of messages)")
				};
				cmd.AddRange(SettingsFileExistingRequiredOptions);
				cmd.Handler = CommandHandler.Create<CommandLineArgs, DateTime?>(Worker.GetEventHubEvents);
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
					new Option<FileInfo>(new string[] { "--scriptfile" }, "Name of the SQL script (with \".sql\" extension) to generate an override file from. Should be in format: \"SELECT <target server>, <target db> ...\"").ExistingOnly(),
					new Option<string>(new string[] { "--scripttext" }, "SQL query to generate an override file from. Should be in format: \"SELECT <target server>, <target db> ...\""),
					new Option<FileInfo>(new string[] { "-o", "--outputfile" }, "Name of the output file to write the override file to. Will always generate file with \".cfg\" extension") { IsRequired = true },
					new Option<bool>(new string[] { "-f", "--force" }, "Force overwrite of existing output file"),

				};
				cmd.AddRange(DatabaseAuthArgs);
				cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.GenerateOverrideFileFromSqlScript);
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
					EventHubUtilityCommand
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
                            databaseOption.Copy(true),
                            serverOption.Copy(true),
                            dacpacOutputOption
                        };
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreateDacpac);
                return cmd;
            }
        }
    }
}
