using Microsoft.SqlServer.Management.Dmf;
using Microsoft.SqlServer.Management.HadrModel;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoreLinq;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerApp.Internal;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass()]
    public partial class CommandLineArgsTest
    {

        [TestMethod]
        public void Duplicate_ArgumentCheck_Test()
        {
            CommandLineArgs cmdLine;
            string message;
            string[] args = new string[0];
            var commandList = CommandLineBuilder.ListCommands();
            var sortedCommands = commandList.OrderBy(x => x[0]).ToList();
            try
            {
                foreach(var command in sortedCommands)
                {
                    command.Add("-h");
                    args = command.ToArray();
                    (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                    Assert.IsTrue(message.Length == 0, $"Expected empty message, instead returned: '{message}'");

                    System.Console.WriteLine(string.Join(' ', command));
                }
            }
            catch(Exception exe)
            {
                if(exe.Message.ToLower().Contains("an item with the same key has already been added"))
                {
                    Assert.Fail($"There is a duplicate key in the command line arguments: '{string.Join(' ', args)}' : {exe.Message}");
                }
                else
                {
                    Assert.Fail($"Error parsing arguments: '{string.Join(' ', args)}' : {exe.Message}");
                }
            }
        }
        
    }
}
