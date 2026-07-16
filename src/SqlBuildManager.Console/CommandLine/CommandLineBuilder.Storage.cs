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
                    blobProxyEndpointOption
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
                    blobProxyEndpointOption
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

        private static Command StorageCommand =>
            new("storage", "List and download private Blob Storage files through Azure Relay")
            {
                StorageListCommand,
                StorageDownloadCommand
            };
    }
}
