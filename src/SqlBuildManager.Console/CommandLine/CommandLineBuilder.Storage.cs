using System.CommandLine;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        internal static Option<string> storageContainerOption = new("--container")
        {
            Description = "Blob container to access through Azure Relay",
            Required = true
        };

        internal static Option<string> storagePrefixOption = new("--prefix")
        {
            Description = "Optional blob name prefix used to filter the listing"
        };

        internal static Option<string> storageDownloadPrefixOption = new("--prefix")
        {
            Description = "Blob name prefix identifying files to download",
            Required = true
        };

        internal static Option<string[]> storageBlobOption = new("--blob", "-b")
        {
            Description = "Blob name to download. Specify one or more names after the option.",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
            Required = true
        };

        internal static Option<DirectoryInfo> storageOutputPathOption = new("--outputpath", "-o")
        {
            Description = "Local directory where downloaded blobs are saved",
            Required = true
        };

        private static Command StorageListCommand
        {
            get
            {
                var cmd = new Command("list", "List files in a Blob container through Azure Relay")
                {
                    storageContainerOption,
                    storagePrefixOption,
                    relayProxyEndpointOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.SetAction(async (parseResult, cancellationToken) =>
                {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.ListRelayBlobFilesAsync(
                        cmdLine,
                        parseResult.GetValue(storageContainerOption)!,
                        parseResult.GetValue(storagePrefixOption) ?? string.Empty,
                        cancellationToken);
                });
                return cmd;
            }
        }

        private static Command StorageDownloadCommand
        {
            get
            {
                var cmd = new Command("download", "Download one or more Blob files through Azure Relay")
                {
                    storageContainerOption,
                    storageBlobOption,
                    storageOutputPathOption,
                    relayProxyEndpointOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.SetAction(async (parseResult, cancellationToken) =>
                {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.DownloadRelayBlobFilesAsync(
                        cmdLine,
                        parseResult.GetValue(storageContainerOption)!,
                        parseResult.GetValue(storageBlobOption)!,
                        parseResult.GetValue(storageOutputPathOption)!,
                        cancellationToken);
                });
                return cmd;
            }
        }

        private static Command StorageDownloadPrefixCommand
        {
            get
            {
                var cmd = new Command("download-prefix", "Download Blob files matching a prefix through Azure Relay")
                {
                    storageContainerOption,
                    storageDownloadPrefixOption,
                    storageOutputPathOption,
                    relayProxyEndpointOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.SetAction(async (parseResult, cancellationToken) =>
                {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.DownloadRelayBlobFilesByPrefixAsync(
                        cmdLine,
                        parseResult.GetValue(storageContainerOption)!,
                        parseResult.GetValue(storageDownloadPrefixOption)!,
                        parseResult.GetValue(storageOutputPathOption)!,
                        cancellationToken);
                });
                return cmd;
            }
        }

        private static Command StorageDownloadAllCommand
        {
            get
            {
                var cmd = new Command(
                    "download-all",
                    "Download all Blob files in a container through Azure Relay into an output subfolder named for the container")
                {
                    storageContainerOption,
                    storageOutputPathOption,
                    relayProxyEndpointOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.SetAction(async (parseResult, cancellationToken) =>
                {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.DownloadAllRelayBlobFilesAsync(
                        cmdLine,
                        parseResult.GetValue(storageContainerOption)!,
                        parseResult.GetValue(storageOutputPathOption)!,
                        cancellationToken);
                });
                return cmd;
            }
        }

        private static Command StorageCommand =>
            new("storage", "List and download private Blob Storage files through Azure Relay")
            {
                StorageListCommand,
                StorageDownloadCommand,
                StorageDownloadPrefixCommand,
                StorageDownloadAllCommand
            };
    }
}
