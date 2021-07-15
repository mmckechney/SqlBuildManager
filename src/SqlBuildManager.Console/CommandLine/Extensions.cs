using System;
using System.Collections.Generic;
using System.Text;
using System.CommandLine;
using System.Linq;
using System.Globalization;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{
    public static class Extensions
    {
        public static string ToStringExtension(this object obj, StringType toStringType)
        {
            StringBuilder sb = new StringBuilder();
            foreach (System.Reflection.PropertyInfo property in obj.GetType().GetProperties())
            {
                if (!property.CanRead || (property.GetValue(obj) == null || string.IsNullOrWhiteSpace(property.GetValue(obj).ToString())))
                {
                    continue;
                }

                if (property.PropertyType == typeof(CommandLineArgs.AutoScripting) ||
                    property.PropertyType == typeof(CommandLineArgs.StoredProcTesting) ||
                    property.PropertyType == typeof(CommandLineArgs.Synchronize) ||
                    property.PropertyType == typeof(CommandLineArgs.Connections))
                {
                    if (property.GetValue(obj) != null && toStringType == StringType.Basic)
                    {
                        sb.Append(property.GetValue(obj).ToStringExtension(toStringType));
                    }
                }
                else if (property.PropertyType == typeof(CommandLineArgs.Authentication) ||
                         property.PropertyType == typeof(CommandLineArgs.DacPac) ||
                         property.PropertyType == typeof(CommandLineArgs.Batch))
                {
                    if (property.GetValue(obj) != null)
                    {
                        sb.Append(property.GetValue(obj).ToStringExtension(toStringType));
                    }
                }
                else
                {
                    switch (property.Name)
                    {
                        case "AuthenticationType":
                            sb.Append("--authtype \"" + property.GetValue(obj).ToString() + "\" ");
                            break;

                        case "SettingsFile":
                            if (toStringType == StringType.Basic)
                            {
                                sb.Append("--settingsfile \"" + property.GetValue(obj).ToString() + "\" ");
                            }
                            break;

                        case "MultiDbRunConfigFileName":
                        case "ManualOverRideSets":
                            if ((toStringType == StringType.Batch) &&
                                    (!string.IsNullOrWhiteSpace(((CommandLineArgs)obj).BatchArgs.ServiceBusTopicConnectionString)))
                            {
                                break;
                            }
                            else
                            {
                                sb.Append("--override \"" + property.GetValue(obj).ToString() + "\" ");
                            }
                            break;

                        case "BuildFileName":
                            sb.Append("--packagename \"" + property.GetValue(obj).ToString() + "\" ");
                            break;

                        case "EventHubConnectionString":
                            sb.Append("--eventhubconnection \"" + property.GetValue(obj).ToString() + "\" ");
                            break;

                        case "ServiceBusTopicConnectionString":
                            sb.Append("--servicebustopicconnection \"" + property.GetValue(obj).ToString() + "\" ");
                            break;
                        case "JobName": //Ignore this because it will be counted as a duplicate for BatchJobName
                        case "OverrideDesignated":
                        case "CliVersion":
                        case "WhatIf":
                        case "LogLevel":
                        case "SettingsFileKey":
                            //ignore these
                            break;

                        default:
                            if (property.PropertyType == typeof(bool))
                            {
                                if (property.Name == "PollBatchPoolStatus" && (toStringType == StringType.Batch))
                                {
                                    continue;
                                }
                                if (bool.Parse(property.GetValue(obj).ToString()) == true) //ignore anything not set
                                {
                                    sb.Append("--" + property.Name.ToLower() + " true ");
                                }
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                sb.Append("--" + property.Name.ToLower() + " \"" + property.GetValue(obj).ToString() + "\" ");
                            }
                            else
                            {
                                double num;
                                if (double.TryParse(property.GetValue(obj).ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out num))
                                {
                                    sb.Append("--" + property.Name.ToLower() + " " + property.GetValue(obj).ToString() + " ");
                                }
                                else
                                {
                                    sb.Append("--" + property.Name.ToLower() + " \"" + property.GetValue(obj).ToString() + "\" ");
                                }
                            }
                            break;
                    }

                }

            }
            return sb.ToString();
        }

        public static Option<T> Copy<T>(this Option<T> opt, bool required)
        {
            var aliases = opt.Aliases.ToArray();
            Option<T> newOpt = new Option<T>(aliases, opt.Description);
            newOpt.Name = opt.Name;
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
            catch(Exception)
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
