using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Interfaces.Console;
using System;
using System.Reflection;
using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class ExecutionOptionsTest
    {
        [TestMethod]
        public void CommandLineArgs_UsesCentralExecutionDefaults()
        {
            var commandLine = new CommandLineArgs();

            Assert.AreEqual(ExecutionOptions.DefaultConcurrency, commandLine.Concurrency);
            Assert.AreEqual(ExecutionOptions.DefaultScriptTimeoutSeconds, commandLine.DefaultScriptTimeout);
            Assert.AreEqual(ExecutionOptions.DefaultTimeoutRetryCount, commandLine.TimeoutRetryCount);
            Assert.AreEqual(ExecutionOptions.DefaultBatchNodeCount, commandLine.BatchArgs.BatchNodeCount);
            Assert.AreEqual(ExecutionOptions.DefaultBatchJobMonitorTimeoutMinutes, commandLine.BatchArgs.JobMonitorTimeout);
            Assert.AreEqual(ExecutionOptions.DefaultKubernetesPodCount, commandLine.KubernetesArgs.PodCount);
            Assert.AreEqual(ExecutionOptions.DefaultContainerAppMaxCount, commandLine.ContainerAppArgs.MaxContainerCount);
        }

        [TestMethod]
        public void Validate_RejectsNonPositiveCommonExecutionOptions()
        {
            var commandLine = new CommandLineArgs
            {
                Concurrency = 0,
                DefaultScriptTimeout = -1
            };

            var errors = ExecutionOptionValidator.Validate(commandLine);

            Assert.HasCount(2, errors);
        }

        [TestMethod]
        public void Validate_IgnoresBackendOptionsForUnrelatedCommands()
        {
            var commandLine = new CommandLineArgs();
            commandLine.BatchArgs.BatchNodeCount = 0;
            commandLine.BatchArgs.JobMonitorTimeout = 0;
            commandLine.KubernetesArgs.PodCount = 0;
            commandLine.ContainerAppArgs.MaxContainerCount = 0;

            var errors = ExecutionOptionValidator.Validate(commandLine);

            Assert.IsEmpty(errors);
        }

        [TestMethod]
        public void BackendValidators_RejectNonPositiveExecutionOptions()
        {
            var commandLine = new CommandLineArgs();
            commandLine.BatchArgs.BatchNodeCount = 0;
            commandLine.BatchArgs.JobMonitorTimeout = 0;
            commandLine.KubernetesArgs.PodCount = 0;
            commandLine.ContainerAppArgs.MaxContainerCount = 0;

            Assert.HasCount(2, ExecutionOptionValidator.ValidateBatch(commandLine));
            Assert.HasCount(1, ExecutionOptionValidator.ValidateKubernetes(commandLine));
            Assert.HasCount(1, ExecutionOptionValidator.ValidateContainerApp(commandLine));
        }

        [TestMethod]
        public void ValidateUserNameAndPassword_UsesAuthenticationExitCode()
        {
            var commandLine = new CommandLineArgs();
            commandLine.AuthenticationArgs.AuthenticationType = SqlBuildManager.Connection.AuthenticationType.Password;

            int result = Validation.ValidateUserNameAndPassword(commandLine, out string[] errors);

            Assert.AreEqual((int)ExecutionReturn.InvalidAuthenticationArguments, result);
            StringAssert.Contains(errors[1], ((int)ExecutionReturn.InvalidAuthenticationArguments).ToString());
        }

        [TestMethod]
        [DataRow("job")]
        [DataRow("job-123")]
        [DataRow("a12")]
        public void TryValidateBatchJobName_AcceptsPortableNames(string jobName)
        {
            bool valid = ExecutionOptionValidator.TryValidateBatchJobName(jobName, required: true, out string error);

            Assert.IsTrue(valid, error);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("ab")]
        [DataRow("UPPER")]
        [DataRow("job--name")]
        [DataRow("job_name")]
        public void TryValidateBatchJobName_RejectsUnsafeNames(string jobName)
        {
            bool valid = ExecutionOptionValidator.TryValidateBatchJobName(jobName, required: true, out string error);

            Assert.IsFalse(valid);
            Assert.IsFalse(string.IsNullOrWhiteSpace(error));
        }

        [TestMethod]
        public void ExecutionReturn_AllValuesHaveOperatorDescriptions()
        {
            foreach (ExecutionReturn value in Enum.GetValues<ExecutionReturn>())
            {
                FieldInfo field = typeof(ExecutionReturn).GetField(value.ToString())!;
                ComponentDescriptionAttribute? description = field.GetCustomAttribute<ComponentDescriptionAttribute>();

                Assert.IsNotNull(description, $"ExecutionReturn.{value} is missing an operator description.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(description.Description));
            }
        }
    }
}
