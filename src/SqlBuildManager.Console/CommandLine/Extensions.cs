using Azure.ResourceManager.Resources.Models;
using Microsoft.SqlServer.Management.Dmf;
using SqlBuildManager.Console.ContainerApp.Internal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
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
                    property.PropertyType == typeof(CommandLineArgs.Aci) ||
                    property.PropertyType == typeof(CommandLineArgs.Kubernetes) ||
                    property.PropertyType == typeof(CommandLineArgs.EventHub) ||
                    property.PropertyType == typeof(CommandLineArgs.Network))
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
                        }else
                        {
                            if(cmd.AuthenticationArgs.AuthenticationType == SqlSync.Connection.AuthenticationType.ManagedIdentity)
                            {
                                args.AddRange(new string[] { "--authtype", "ManagedIdentity".Quoted() });
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
                            else if(toStringType != StringType.Batch)
                            {
                                args.AddRange(new string[] { "--resourcegroup", property.GetValue(obj).ToString().Quoted() });
                            }
                            break;
                        case "TenantId":
                            if (toStringType != StringType.Batch)
                            {
                                args.AddRange(new string[] { "--tenantid", property.GetValue(obj).ToString().Quoted() });
                            }
                            break;
                        case "ServiceAccountName":
                            if (toStringType != StringType.Batch)
                            {
                                args.AddRange(new string[] { "--serviceaccountname", property.GetValue(obj).ToString().Quoted() });
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
                        case "KeyVaultSecretsRetrieved":
                        case "JobMonitorTimeout":
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
                            else if(property.PropertyType == typeof(EventHubLogging[]))
                            {
                                var values = (EventHubLogging[])property.GetValue(obj);
                                foreach (var value in values)
                                {
                                    args.AddRange(new string[] { $"--{property.Name.ToLower()}", value.ToString().Quoted() });
                                }
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
            foreach (var opt in options)
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


        public static void SetValues(this CommandLineArgs current, CommandLineArgs incoming)
        {
            foreach (System.Reflection.PropertyInfo incomingProp in incoming.GetType().GetProperties())
            {
                if(incomingProp.PropertyType.BaseType == typeof(ArgsBase))
                {
                    var currentProp = current.GetType().GetProperty(incomingProp.Name);
                    currentProp.GetValue(current).SetValues(incomingProp.GetValue(incoming), current.DirectPropertyChangeTracker);
                }
            }

            if(incoming.EventHubLogging.Length > 0)
            {
                current.EventHubArgs.Logging = incoming.EventHubLogging;
            }
            current.EventHubLogging = incoming.EventHubLogging;
            current.EventHubArgs.Logging = incoming.EventHubArgs.Logging;

        }
        /// <summary>
        /// Used to set property values from a twin object, but not overwrite existing values if they have already been directly set
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="current">Parent object that is to be updated (but may have already had properties set directly)</param>
        /// <param name="incoming">Incoming twin object that will contain values read from a config file. These should not overwrite any existing values that have already been updated.</param>
        private static void SetValues<T>(this T current, T incoming, List<string> changeTracked)
        {
            foreach (System.Reflection.PropertyInfo incomingProp in incoming.GetType().GetProperties())
            {
                var typeName = incoming.GetType().Name;
                if (changeTracked.Contains($"{typeName}.{incomingProp.Name}"))
                {
                    continue;
                }

                if (incomingProp.CanWrite && incomingProp.CanRead && incomingProp.GetValue(incoming) != null)
                {
                    var incomingPropType = incomingProp.PropertyType;
                    var defaultVal = incomingProp.GetCustomAttribute<DefaultValueAttribute>();
                    var incomingValue = incomingProp.GetValue(incoming);

                    //There is a value coming from the deserialized config file.. so keep checking
                    if (incomingValue != null)
                    {
                        //See if we can skip because it's value is insignificant or a default
                        if (incomingPropType == typeof(string) && string.IsNullOrWhiteSpace(incomingValue.ToString()))
                        {
                            continue;
                        }
                        else if (incomingPropType == typeof(int) && (int)incomingValue == 0)
                        {
                            continue;
                        }
                        else if (defaultVal != null && incomingValue == defaultVal.Value)
                        {
                            continue;
                        }
                        //If we get here.. we have a meaningful value, we need to see if we can overwrite any existing value that has already been set..
                        var currentProp = current.GetType().GetProperty(incomingProp.Name);
                        currentProp.SetValue(current, incomingValue);
                    }
                }
            }
        }
        
        public static CommandLineArgs NullEmptyStrings(this CommandLineArgs obj)
        {
            foreach (System.Reflection.PropertyInfo property in obj.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string) && property.CanWrite && property.CanRead && property.GetValue(obj) != null && string.IsNullOrWhiteSpace(property.GetValue(obj).ToString()))
                {
                    property.SetValue(obj, null);
                }
                else if (
                    property.PropertyType == typeof(CommandLineArgs.Authentication) ||
                    property.PropertyType == typeof(CommandLineArgs.Batch) ||
                    property.PropertyType == typeof(CommandLineArgs.Connections) ||
                    property.PropertyType == typeof(CommandLineArgs.ContainerApp) ||
                    property.PropertyType == typeof(CommandLineArgs.ContainerRegistry) ||
                    property.PropertyType == typeof(CommandLineArgs.Identity) ||
                    property.PropertyType == typeof(CommandLineArgs.Kubernetes) ||
                    property.PropertyType == typeof(CommandLineArgs.Network))
                {
                    property.SetValue(obj, NullEmptyStrings(property.GetValue(obj)));
                }
            }

            return (CommandLineArgs)obj;
        }
        private static object NullEmptyStrings(this object obj)
        {
            if(obj == null)
            {
                return obj;
            }
            foreach (System.Reflection.PropertyInfo property in obj.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string) && property.CanWrite && property.CanRead && property.GetValue(obj) != null && string.IsNullOrWhiteSpace(property.GetValue(obj).ToString()))
                {
                    property.SetValue(obj, null);
                }
            }
            return obj;
        }
    }
}
