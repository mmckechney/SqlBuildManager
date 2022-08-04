using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.Linq;
using System.Text;


namespace SqlBuildManager.Console.CommandLine
{
    public static class Extensions
    {
        public static string ToStringExtension(this object obj, StringType toStringType)
        {
            var args = obj.ToArgs(toStringType);
            return string.Join(" ", args);
        }

        public static string[] ToArgs(this object obj, StringType toStringType = StringType.Basic)
        {
            List<string> args = new List<string>();
            foreach (System.Reflection.PropertyInfo property in obj.GetType().GetProperties())
            {
                if (!property.CanRead || (property.GetValue(obj) == null || string.IsNullOrWhiteSpace(property.GetValue(obj).ToString())))
                {
                    continue;
                }

                if (property.PropertyType == typeof(CommandLineArgs.AutoScripting) ||
                    property.PropertyType == typeof(CommandLineArgs.StoredProcTesting) ||
                    property.PropertyType == typeof(CommandLineArgs.Synchronize) ||
                    property.PropertyType == typeof(CommandLineArgs.Aci))
                {
                    if (property.GetValue(obj) != null && toStringType == StringType.Basic)
                    {
                        args.AddRange(property.GetValue(obj).ToArgs(toStringType));
                    }
                }
                else if (property.PropertyType == typeof(CommandLineArgs.Batch))
                {

                    if (property.GetValue(obj) != null)
                    {
                        args.AddRange(property.GetValue(obj).ToArgs(toStringType));
                    }
                    break;

                }
                else if (property.PropertyType == typeof(CommandLineArgs.DacPac) ||
                         property.PropertyType == typeof(CommandLineArgs.Identity))
                {
                    if (property.GetValue(obj) != null)
                    {
                        args.AddRange(property.GetValue(obj).ToArgs(toStringType));
                    }
                }
                else if (property.PropertyType == typeof(CommandLineArgs.Authentication)) //Special case if Key Vault is specified
                {
                    if (obj is CommandLineArgs)
                    {
                        var cmd = (CommandLineArgs)obj;
                        if (string.IsNullOrWhiteSpace(cmd.ConnectionArgs.KeyVaultName))
                        {
                            if (property.GetValue(obj) != null)
                            {
                                args.AddRange(property.GetValue(obj).ToArgs(toStringType));
                            }
                        }

                    }
                }
                else if (property.PropertyType == typeof(CommandLineArgs.Connections)) //Special case if Key Vault is specified
                {
                    if (property.GetValue(obj) != null)
                    {
                        var conArgs = (CommandLineArgs.Connections)property.GetValue(obj);
                        if (!string.IsNullOrWhiteSpace(conArgs.KeyVaultName))
                        {
                            if (!args.Contains("--keyvaultname"))
                            {
                                args.AddRange(new string[] { "--keyvaultname", conArgs.KeyVaultName.Quoted() });
                            }
                        }
                        else
                        {
                            args.AddRange(property.GetValue(obj).ToArgs(toStringType));
                        }
                    }
                }
                else
                {
                    switch (property.Name)
                    {
                        case "AuthenticationType":
                            args.AddRange(new string[] { "--authtype", property.GetValue(obj).ToString().Quoted() });
                            break;

                        case "SettingsFile":
                            if (toStringType == StringType.Basic)
                            {
                                //TODO: do we need this?
                                //args.AddRange(new string[] { "--settingsfile", property.GetValue(obj).ToString().Quoted() });
                            }
                            break;

                        case "MultiDbRunConfigFileName":
                        case "ManualOverRideSets":
                            if ((toStringType == StringType.Batch) &&
                                    (!string.IsNullOrWhiteSpace(((CommandLineArgs)obj).ConnectionArgs.ServiceBusTopicConnectionString)))
                            {
                                break;
                            }
                            else
                            {
                                args.AddRange(new string[] { "--override ", property.GetValue(obj).ToString().Quoted() });
                            }
                            break;

                        case "BuildFileName":
                            args.AddRange(new string[] { "--packagename", property.GetValue(obj).ToString().Quoted() });
                            break;

                        case "EventHubConnectionString":
                            args.AddRange(new string[] { "--eventhubconnection", property.GetValue(obj).ToString().Quoted() });
                            break;

                        case "ServiceBusTopicConnectionString":
                            args.AddRange(new string[] { "--servicebustopicconnection", property.GetValue(obj).ToString().Quoted() });
                            break;
                        case "ResourceGroup":
                            if (obj.GetType() == typeof(CommandLineArgs.Identity))
                            {
                                args.AddRange(new string[] { "--identityresourcegroup", property.GetValue(obj).ToString().Quoted() });

                            }
                            else
                            {
                                args.AddRange(new string[] { "--resourcegroup", property.GetValue(obj).ToString().Quoted() });
                            }
                            break;
                        case "BatchJobName": //Ignore this because it will be counted as a duplicate for JobName
                        case "OverrideDesignated":
                        case "CliVersion":
                        case "WhatIf":
                        case "LogLevel":
                        case "SettingsFileKey":
                        case "ContainerAppArgs":
                        case "ContainerRegistryArgs":
                            //ignore these
                            break;

                        default:

                            if (property.PropertyType == typeof(bool))
                            {
                                if (property.Name == "PollBatchPoolStatus" && (toStringType == StringType.Batch))
                                {
                                    continue;
                                }
                                if (property.Name == "Decrypted" && (toStringType == StringType.Batch))
                                {
                                    continue;
                                }
                                if (bool.Parse(property.GetValue(obj).ToString()) == true) //ignore anything not set
                                {
                                    args.AddRange(new string[] { $"--{property.Name.ToLower()}", "true" });
                                }
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                args.AddRange(new string[] { $"--{property.Name.ToLower()}", property.GetValue(obj).ToString().Quoted() });
                            }
                            else
                            {
                                double num;
                                if (double.TryParse(property.GetValue(obj).ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out num))
                                {
                                    args.AddRange(new string[] { $"--{property.Name.ToLower()}", property.GetValue(obj).ToString() });
                                }
                                else
                                {
                                    args.AddRange(new string[] { $"--{property.Name.ToLower()}", property.GetValue(obj).ToString().Quoted() });
                                }
                            }
                            break;
                    }

                }

            }
            return args.ToArray();
        }

        public static void AddRange(this Command cmd, List<Option> options)
        {
            foreach(var opt in options)
            {
                cmd.Add(opt);
            }
        }
        public static string Quoted(this string str)
        {
            return "\"" + str + "\"";
        }
        public static Option<T> Copy<T>(this Option<T> opt, bool required)
        {

            var aliases = opt.Aliases.ToArray();
            Option<T> newOpt = new Option<T>(aliases, opt.Description);
            //newOpt.Name = opt.Name;
            //newOpt.ArgumentHelpName = "";
            newOpt.IsRequired = required;

            return newOpt;
        }
        public static string DecodeBase64(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            try
            {
                var valueBytes = System.Convert.FromBase64String(value);
                return Encoding.UTF8.GetString(valueBytes);
            }
            catch (Exception)
            {
                return value;
            }
        }

        public static string EncodeBase64(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            try
            {
                var valueBytes = Encoding.UTF8.GetBytes(value);
                return Convert.ToBase64String(valueBytes);
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}
