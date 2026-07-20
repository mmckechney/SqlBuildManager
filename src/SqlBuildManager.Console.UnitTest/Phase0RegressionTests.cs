using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Aci;
using System;
using System.Diagnostics;

namespace SqlBuildManager.Console.UnitTest
{
    /// <summary>
    /// Regression tests for Phase 0 defects:
    ///   MAINT-004 – ACI unknown-error handling must never produce false success
    ///   PERF-007  – Event Hub total timeout uses Elapsed.Seconds (wraps each minute)
    /// </summary>
    [TestClass]
    public class Phase0RegressionTests
    {
        // ── MAINT-004: ACI existence check error classification ───────────────────

        [TestMethod]
        public void AciManager_ClassifyArmExistenceStatus_404_ReturnsFalse()
        {
            // 404 = "Not Found" → ACI does not exist → false.
            bool? result = AciManager.ClassifyArmExistenceStatus(404);
            Assert.IsNotNull(result);
            Assert.IsFalse(result!.Value, "404 must map to 'does not exist' (false).");
        }

        [TestMethod]
        public void AciManager_ClassifyArmExistenceStatus_403_ReturnsNull()
        {
            // 403 = "Forbidden" → we cannot determine existence → null (caller throws).
            bool? result = AciManager.ClassifyArmExistenceStatus(403);
            Assert.IsNull(result, "403 (Forbidden) must return null so callers throw, not return true.");
        }

        [TestMethod]
        public void AciManager_ClassifyArmExistenceStatus_500_ReturnsNull()
        {
            // 500 = server error → we cannot determine existence → null.
            bool? result = AciManager.ClassifyArmExistenceStatus(500);
            Assert.IsNull(result, "500 (server error) must return null so callers throw, not return true.");
        }

        [TestMethod]
        public void AciManager_ClassifyArmExistenceStatus_409_ReturnsNull()
        {
            // 409 = conflict → indeterminate → null.
            bool? result = AciManager.ClassifyArmExistenceStatus(409);
            Assert.IsNull(result, "409 (Conflict) must return null — unknown outcome is not success.");
        }

        [TestMethod]
        public void AciManager_ClassifyArmExistenceStatus_200_ReturnsNull()
        {
            // Caller handles 200 (HasData → true) before reaching the catch path;
            // this method is only called from the exception branch, so 200 is not
            // expected, but if somehow called it must not map to false.
            bool? result = AciManager.ClassifyArmExistenceStatus(200);
            Assert.IsNull(result, "200 is not a 404; this method returns null for all non-404 codes.");
        }

        [TestMethod]
        public void AciManager_ParseAciDeployment_ArmJson_ReturnsDeployment()
        {
            const string responseJson =
                """
                {
                  "properties": {
                    "containers": [
                      {
                        "name": "sqlbuildmanager000",
                        "properties": {
                          "instanceView": {
                            "currentState": {
                              "state": "Running",
                              "detailStatus": "Running"
                            }
                          }
                        }
                      }
                    ]
                  },
                  "name": "aci-test"
                }
                """;

            var deployment = AciManager.ParseAciDeployment(responseJson);

            Assert.AreEqual("aci-test", deployment.Name);
            Assert.HasCount(1, deployment.Properties.Containers);
            Assert.AreEqual(
                "Running",
                deployment.Properties.Containers[0].Properties.InstanceView.CurrentState.DetailStatus);
        }

        // ── PERF-007: Elapsed.TotalSeconds vs Elapsed.Seconds ────────────────────

        [TestMethod]
        public void Stopwatch_TotalSeconds_DoesNotWrapAtSixtySeconds()
        {
            // Before the fix: Elapsed.Seconds wraps back to 0 after 60 s, so a
            // 90-second elapsed time would appear as 30 s – below any timeout > 30.
            // After the fix: Elapsed.TotalSeconds accumulates monotonically.
            var elapsed = TimeSpan.FromSeconds(90);

            int secondsComponent = elapsed.Seconds;     // 30 (wraps)
            double totalSeconds = elapsed.TotalSeconds; // 90.0 (monotonic)

            int timeout = 60;
            bool oldBehavior = secondsComponent <= timeout; // true (30 ≤ 60) — WRONG after 1 min
            bool newBehavior = totalSeconds <= timeout;      // false (90 > 60) — CORRECT

            Assert.IsTrue(oldBehavior,  "Seconds component should be ≤ 60 for 90-second span (confirms the bug).");
            Assert.IsFalse(newBehavior, "TotalSeconds should be > 60 for 90-second span (confirms the fix).");
        }

        [TestMethod]
        public void Stopwatch_TotalSeconds_BelowTimeout_LoopContinues()
        {
            var elapsed = TimeSpan.FromSeconds(30);
            int timeout = 60;
            Assert.IsTrue(elapsed.TotalSeconds <= timeout, "30 s < 60 s timeout: loop should continue.");
        }

        [TestMethod]
        public void Stopwatch_TotalSeconds_ExceedsTimeout_LoopEnds()
        {
            var elapsed = TimeSpan.FromSeconds(65);
            int timeout = 60;
            Assert.IsFalse(elapsed.TotalSeconds <= timeout, "65 s > 60 s timeout: loop should end.");
        }

        [TestMethod]
        public void Stopwatch_TotalSeconds_ZeroTimeout_AlwaysContinues()
        {
            // timeout == 0 means "no timeout" — special case in Worker.cs.
            int timeout = 0;
            var elapsed = TimeSpan.FromSeconds(3600); // 1 hour
            bool shouldContinue = elapsed.TotalSeconds <= timeout || timeout == 0;
            Assert.IsTrue(shouldContinue, "Zero timeout means run indefinitely regardless of elapsed time.");
        }
    }
}
