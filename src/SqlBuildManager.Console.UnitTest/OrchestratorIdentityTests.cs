#nullable enable
using Azure.Core;
using Azure.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Aad;
using System;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class OrchestratorIdentityTests
    {
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void MissingOrchestratorIdentity_PreservesExistingCredentialSelection(string? clientId)
        {
            var credential = AadHelper.CreateOrchestratorCredential(clientId, _ =>
                throw new AssertFailedException("An unspecified orchestration identity must not override existing credentials."));

            Assert.IsNull(credential);
        }

        [TestMethod]
        public void ExplicitOrchestratorIdentity_IsIndependentOfTargetWorkerIdentity()
        {
            const string orchestratorId = "11111111-1111-1111-1111-111111111111";
            TokenCredential expected = new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(orchestratorId));
            string? selectedId = null;

            var actual = AadHelper.CreateOrchestratorCredential(orchestratorId, id =>
            {
                selectedId = id;
                return expected;
            });

            Assert.AreEqual(orchestratorId, selectedId);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        [DataRow("not-a-client-id")]
        [DataRow("00000000-0000-0000-0000-000000000000")]
        public void InvalidOrchestratorIdentity_DoesNotFallBack(string clientId)
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                AadHelper.CreateOrchestratorCredential(clientId, _ =>
                    throw new AssertFailedException("Invalid configuration must not select an identity.")));
        }
    }
}
