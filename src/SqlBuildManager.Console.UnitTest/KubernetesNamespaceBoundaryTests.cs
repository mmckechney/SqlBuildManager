using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Kubernetes;
using System.Collections.Generic;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class KubernetesNamespaceBoundaryTests
    {
        private static KubernetesFiles Files() => new KubernetesFiles
        {
            RuntimeConfigMapFile = "runtime.yaml",
            SecretsFile = string.Empty,
            JobFileName = "job.yaml"
        };

        [TestMethod]
        public void InaccessibleNamespace_DoesNotApplyWorkloadOrCreateNamespace()
        {
            var applied = new List<string>();
            var result = KubernetesManager.ApplyDeployment(Files(), name =>
            {
                Assert.AreEqual("sqlbuildmanager", name);
                return false;
            }, file => { applied.Add(file); return 0; });

            Assert.IsFalse(result);
            Assert.AreEqual(0, applied.Count);
        }

        [TestMethod]
        public void PreprovisionedNamespace_AppliesNamespacedWorkload()
        {
            var applied = new List<string>();
            var result = KubernetesManager.ApplyDeployment(Files(), _ => true,
                file => { applied.Add(file); return 0; });

            Assert.IsTrue(result);
            CollectionAssert.AreEqual(new[] { "runtime.yaml", "job.yaml" }, applied);
        }

        [TestMethod]
        public void ConfigurationFailure_DoesNotStartJobWithStaleConfiguration()
        {
            var applied = new List<string>();
            var result = KubernetesManager.ApplyDeployment(Files(), _ => true,
                file => { applied.Add(file); return 1; });

            Assert.IsFalse(result);
            CollectionAssert.AreEqual(new[] { "runtime.yaml" }, applied);
        }
    }
}
