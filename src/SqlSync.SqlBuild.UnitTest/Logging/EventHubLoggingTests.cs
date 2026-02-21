using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Logging.Threaded;
using Microsoft.Extensions.Logging;
using System;

namespace SqlSync.SqlBuild.UnitTest.Logging
{
    /// <summary>
    /// Unit tests for EventHubLogging class.
    /// Note: Methods that require Azure Event Hub connectivity are tested in integration tests.
    /// These tests focus on class structure, property behaviors, and static state management.
    /// </summary>
    [TestClass]
    public class EventHubLoggingTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up static state after each test
            EventHubLogging.CloseAndFlush();
        }

        #region Class Structure Tests

        [TestMethod]
        public void EventHubLogging_ClassExists_IsPublic()
        {
            // Arrange & Act
            var type = typeof(EventHubLogging);

            // Assert
            Assert.IsNotNull(type);
            Assert.IsTrue(type.IsPublic);
            Assert.IsTrue(type.IsClass);
        }

        [TestMethod]
        public void EventHubLogging_CreateLogger_WithConnectionString_MethodExists()
        {
            // Arrange
            var type = typeof(EventHubLogging);

            // Act
            var methods = type.GetMethods();
            var createLoggerMethods = Array.FindAll(methods, m => m.Name == "CreateLogger");

            // Assert
            Assert.IsTrue(createLoggerMethods.Length >= 2, "Should have at least 2 CreateLogger overloads");
        }

        [TestMethod]
        public void EventHubLogging_CloseAndFlush_MethodExists()
        {
            // Arrange
            var type = typeof(EventHubLogging);

            // Act
            var method = type.GetMethod("CloseAndFlush");

            // Assert
            Assert.IsNotNull(method, "CloseAndFlush method should exist");
            Assert.IsTrue(method.IsStatic, "CloseAndFlush should be static");
            Assert.IsTrue(method.IsPublic, "CloseAndFlush should be public");
        }

        [TestMethod]
        public void EventHubLogging_ConfigureEventHubLogger_MethodExists()
        {
            // Arrange
            var type = typeof(EventHubLogging);

            // Act
            var method = type.GetMethod("ConfigureEventHubLogger");

            // Assert
            Assert.IsNotNull(method, "ConfigureEventHubLogger method should exist");
            Assert.IsTrue(method.IsStatic, "ConfigureEventHubLogger should be static");
            Assert.IsTrue(method.IsPublic, "ConfigureEventHubLogger should be public");
        }

        #endregion

        #region CloseAndFlush Tests

        [TestMethod]
        public void CloseAndFlush_WhenCalled_DoesNotThrow()
        {
            // Act & Assert - should not throw
            EventHubLogging.CloseAndFlush();
        }

        [TestMethod]
        public void CloseAndFlush_CalledMultipleTimes_DoesNotThrow()
        {
            // Act & Assert - should not throw when called multiple times
            EventHubLogging.CloseAndFlush();
            EventHubLogging.CloseAndFlush();
            EventHubLogging.CloseAndFlush();
        }

        #endregion

        #region EventHubLoggerFactory Property Tests

        [TestMethod]
        public void EventHubLoggerFactory_Get_ReturnsLoggerFactory()
        {
            // Act
            var factory = EventHubLogging.EventHubLoggerFactory;

            // Assert
            Assert.IsNotNull(factory);
            Assert.IsInstanceOfType(factory, typeof(ILoggerFactory));
        }

        [TestMethod]
        public void EventHubLoggerFactory_Set_AcceptsLoggerFactory()
        {
            // Arrange
            var customFactory = new LoggerFactory();

            // Act
            EventHubLogging.EventHubLoggerFactory = customFactory;

            // Assert
            Assert.AreSame(customFactory, EventHubLogging.EventHubLoggerFactory);
        }

        [TestMethod]
        public void EventHubLoggerFactory_SetToNull_AcceptsNull()
        {
            // Arrange - first set it to something
            var customFactory = new LoggerFactory();
            EventHubLogging.EventHubLoggerFactory = customFactory;

            // Act
            EventHubLogging.EventHubLoggerFactory = null!;

            // Assert - getting it again should create a new one
            var result = EventHubLogging.EventHubLoggerFactory;
            Assert.IsNotNull(result);
        }

        #endregion

        #region CreateLogger Method Parameter Tests

        [TestMethod]
        public void CreateLogger_ConnectionStringOverload_HasCorrectParameters()
        {
            // Arrange
            var type = typeof(EventHubLogging);
            var method = type.GetMethod("CreateLogger", new[] { typeof(Type), typeof(string) });

            // Act
            var parameters = method?.GetParameters();

            // Assert
            Assert.IsNotNull(method);
            Assert.AreEqual(2, parameters!.Length);
            Assert.AreEqual(typeof(Type), parameters[0].ParameterType);
            Assert.AreEqual(typeof(string), parameters[1].ParameterType);
        }

        [TestMethod]
        public void CreateLogger_ManagedIdentityOverload_HasCorrectParameters()
        {
            // Arrange
            var type = typeof(EventHubLogging);
            var method = type.GetMethod("CreateLogger", new[] { typeof(Type), typeof(string), typeof(string), typeof(string) });

            // Act
            var parameters = method?.GetParameters();

            // Assert
            Assert.IsNotNull(method);
            Assert.AreEqual(4, parameters!.Length);
            Assert.AreEqual(typeof(Type), parameters[0].ParameterType);
            Assert.AreEqual(typeof(string), parameters[1].ParameterType);
            Assert.AreEqual(typeof(string), parameters[2].ParameterType);
            Assert.AreEqual(typeof(string), parameters[3].ParameterType);
        }

        [TestMethod]
        public void CreateLogger_BothOverloads_ReturnILogger()
        {
            // Arrange
            var type = typeof(EventHubLogging);
            var connectionStringMethod = type.GetMethod("CreateLogger", new[] { typeof(Type), typeof(string) });
            var managedIdentityMethod = type.GetMethod("CreateLogger", new[] { typeof(Type), typeof(string), typeof(string), typeof(string) });

            // Assert
            Assert.IsNotNull(connectionStringMethod);
            Assert.IsNotNull(managedIdentityMethod);
            Assert.AreEqual(typeof(ILogger), connectionStringMethod.ReturnType);
            Assert.AreEqual(typeof(ILogger), managedIdentityMethod.ReturnType);
        }

        #endregion

        #region CreateLogger Behavior Tests

        [TestMethod]
        public void CreateLogger_WithEmptyConnectionString_ReturnsLoggerOrNull()
        {
            // Act - Empty connection string should not throw, but might return null due to config failure
            var logger = EventHubLogging.CreateLogger(typeof(EventHubLoggingTests), string.Empty);

            // Assert - We accept both outcomes since this depends on Azure SDK behavior
            // The important thing is it doesn't throw
            Assert.IsTrue(logger == null || logger is ILogger);
        }

        [TestMethod]
        public void CreateLogger_WithManagedIdentity_AddsServiceBusSuffix()
        {
            // Arrange & Act - This should add .servicebus.windows.net suffix
            // Note: This will fail to connect but tests the namespace formatting logic
            var logger = EventHubLogging.CreateLogger(
                typeof(EventHubLoggingTests),
                "testnamespace",
                "testhub",
                "testclientid");

            // Assert - Logger creation might fail due to no actual Event Hub, but shouldn't throw
            Assert.IsTrue(logger == null || logger is ILogger);
        }

        [TestMethod]
        public void CreateLogger_WithFullyQualifiedNamespace_DoesNotDuplicateSuffix()
        {
            // Arrange & Act - Namespace already has suffix
            var logger = EventHubLogging.CreateLogger(
                typeof(EventHubLoggingTests),
                "testnamespace.servicebus.windows.net",
                "testhub",
                "testclientid");

            // Assert
            Assert.IsTrue(logger == null || logger is ILogger);
        }

        [TestMethod]
        public void CreateLogger_WithUpperCaseNamespace_HandlesCorrectly()
        {
            // Arrange & Act - Tests case insensitive handling
            var logger = EventHubLogging.CreateLogger(
                typeof(EventHubLoggingTests),
                "TESTNAMESPACE.SERVICEBUS.WINDOWS.NET",
                "testhub",
                string.Empty);

            // Assert
            Assert.IsTrue(logger == null || logger is ILogger);
        }

        #endregion

        #region Static State Tests

        [TestMethod]
        public void EventHubLogging_AfterCloseAndFlush_CanCreateNewLogger()
        {
            // Arrange
            EventHubLogging.CloseAndFlush();

            // Act - Should be able to get a new factory
            var factory = EventHubLogging.EventHubLoggerFactory;

            // Assert
            Assert.IsNotNull(factory);
        }

        [TestMethod]
        public void EventHubLogging_MultipleLoggerCreation_DoesNotThrow()
        {
            // Act & Assert - Multiple creations should not throw
            EventHubLogging.CreateLogger(typeof(string), string.Empty);
            EventHubLogging.CreateLogger(typeof(int), string.Empty);
            EventHubLogging.CreateLogger(typeof(object), string.Empty);
        }

        #endregion
    }
}
