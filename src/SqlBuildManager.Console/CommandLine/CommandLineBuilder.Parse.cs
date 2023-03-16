using Spectre.Console;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Linq;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        public static Parser GetCommandParser()
        {
            RootCommand rootCommand = SetUp();

            var builder = new System.CommandLine.Builder.CommandLineBuilder(rootCommand)
                       .UseTypoCorrections()
                       .UseDefaults()
                       .UseHelp(ctx =>
                       {
                           ctx.HelpBuilder.CustomizeLayout(_ => HelpBuilder.Default
                                             .GetLayout()
                                             .Prepend(
                                                 _ => AnsiConsole.Write(new FigletText("SQL Build Manager"))
                                             ));
                           ctx.HelpBuilder.CustomizeSymbol(FirstBuildRunCommand,
                               firstColumnText: $"** Build Execution Commands:\u0000{Environment.NewLine}{FirstBuildRunCommand.Name}",
                               secondColumnText: $"\u0000{Environment.NewLine}{FirstBuildRunCommand.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(FirstUtilityCommand,
                               firstColumnText: $"\u0000{Environment.NewLine}** Build Utility Commands:\u0000{Environment.NewLine}{FirstUtilityCommand.Name}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstUtilityCommand.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(FirstPackageManagementCommand,
                               firstColumnText: $"\u0000{Environment.NewLine}** Package Management Commands:\u0000{Environment.NewLine}{FirstPackageManagementCommand.Name}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstPackageManagementCommand.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(FirstPackageInformationCommand,
                               firstColumnText: $"\u0000{Environment.NewLine}** Package Information Commands:\u0000{Environment.NewLine}{FirstPackageInformationCommand.Name}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstPackageInformationCommand.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(FirstAdditionalCommand,
                               firstColumnText: $"\u0000{Environment.NewLine}** Additional Commands:\u0000{Environment.NewLine}{FirstAdditionalCommand.Name}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstAdditionalCommand.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(imageTagOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Container Registry Options:\u0000{Environment.NewLine}{OptionString(imageTagOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{imageTagOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(threadedConcurrencyTypeOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Concurrency Options:\u0000{Environment.NewLine}{OptionString(threadedConcurrencyTypeOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{threadedConcurrencyTypeOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(threadedConcurrencyRequiredTypeOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Concurrency Options:\u0000{Environment.NewLine}{OptionString(threadedConcurrencyRequiredTypeOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{threadedConcurrencyRequiredTypeOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(settingsfileExistingOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Settings File Options:\u0000{Environment.NewLine}{OptionString(settingsfileExistingOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{settingsfileExistingOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(settingsfileNewOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Settings File Options:\u0000{Environment.NewLine}{OptionString(settingsfileNewOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{settingsfileNewOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(settingsfileExistingRequiredOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Settings File Options:\u0000{Environment.NewLine}{OptionString(settingsfileExistingRequiredOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{settingsfileExistingRequiredOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(usernameOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Database Auth Options:\u0000{Environment.NewLine}{OptionString(usernameOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{usernameOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(clientIdOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Identity Options:\u0000{Environment.NewLine}{OptionString(clientIdOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{clientIdOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(serviceAccountNameOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Identity Options:\u0000{Environment.NewLine}{OptionString(serviceAccountNameOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{serviceAccountNameOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(keyVaultNameOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Connection and Secrets Options:\u0000{Environment.NewLine}{OptionString(keyVaultNameOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{keyVaultNameOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(batchResourceGroupOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Batch Pool Compute Options :\u0000{Environment.NewLine}{OptionString(batchResourceGroupOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{batchResourceGroupOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(batchjobnameOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Batch Job Settings Options :\u0000{Environment.NewLine}{OptionString(batchjobnameOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{batchjobnameOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(containerAppEnvironmentOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Container App Deployment Options :\u0000{Environment.NewLine}{OptionString(containerAppEnvironmentOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{containerAppEnvironmentOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(runtimeFileOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Kubernetes YAML file Options :\u0000{Environment.NewLine}{OptionString(runtimeFileOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{runtimeFileOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(aciIResourceGroupNameOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Container Instance Options :\u0000{Environment.NewLine}{OptionString(aciIResourceGroupNameOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{aciIResourceGroupNameOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(aciIResourceGroupNameNotReqOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Container Instance Options :\u0000{Environment.NewLine}{OptionString(aciIResourceGroupNameNotReqOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{aciIResourceGroupNameNotReqOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(vnetNameOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** VNET Options :\u0000{Environment.NewLine}{OptionString(vnetNameOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{vnetNameOption.Description}");



                           ctx.HelpBuilder.CustomizeSymbol(sectionPlaceholderOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** General Options:\u0000",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000");

                       });

            builder.RegisterWithDotnetSuggest();
            builder.UseTypoCorrections();
            //builder.AddMiddleware(async (context, next) =>
            //{
            //    if (context.ParseResult.HasOption(settingsfileExistingOption) || context.ParseResult.HasOption(settingsfileExistingOption) || context.ParseResult.HasOption(settingsfileExistingRequiredOption))
            //    {
            //        await next(context);
            //    }
            //    else
            //    {
            //        await next(context);
            //    }

            //});

            var parser = builder.Build();

            return parser;
        }
        private static string OptionString(Option option)
        {
            var str = string.Join(", ", option.Aliases) + $" <{option.Name}>";

            if (option.IsRequired)
            {
                return str + " (REQUIRED)";
            }
            else
            {
                return str;
            }
        }
        public static (CommandLineArgs, string) ParseArgumentsWithMessage(string[] args)
        {
            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var res = rootCommand.Parse(args);
            if (res.Errors.Count > 0)
            {
                return (null, string.Join<string>(System.Environment.NewLine, res.Errors.Select(e => e.Message).ToArray()));
            }

            var bindingContext = new InvocationContext(rootCommand.Parse(args)).BindingContext;

            var binder = new ModelBinder(typeof(CommandLineArgs));
            var instance = (CommandLineArgs)binder.CreateInstance(bindingContext);

            return (instance, string.Empty);
        }
        public static CommandLineArgs ParseArguments(string[] args)
        {
            (CommandLineArgs cmd, string msg) = ParseArgumentsWithMessage(args);
            if (cmd == null)
            {
                throw new System.Exception($"Unable to parse arguments: {msg}");
            }
            else
            {
                return cmd;
            }
        }

        public static Command FirstBuildRunCommand { get; private set; }
        public static Command FirstUtilityCommand { get; private set; }
        public static Command FirstPackageManagementCommand { get; private set; }
        public static Command FirstPackageInformationCommand { get; private set; }
        public static Command FirstAdditionalCommand { get; private set; }
    }
}
