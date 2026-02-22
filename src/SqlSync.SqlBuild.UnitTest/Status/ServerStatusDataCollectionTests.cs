using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Status;
using System.IO;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.UnitTest.Status
{
    [TestClass]
    public class ServerStatusDataCollectionTests
    {
        [TestMethod]
        public void BuildFileNameFull_Set_AlsoSetsBuildFileNameShort()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();
            string fullPath = @"C:\Projects\Builds\MyBuildFile.sbm";

            // Act
            collection.BuildFileNameFull = fullPath;

            // Assert
            Assert.AreEqual(fullPath, collection.BuildFileNameFull);
            Assert.AreEqual("MyBuildFile.sbm", collection.BuildFileNameShort);
        }

        [TestMethod]
        public void BuildFileNameShort_SetDirectly_ReturnsValue()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();

            // Act
            collection.BuildFileNameShort = "DirectShortName.sbm";

            // Assert
            Assert.AreEqual("DirectShortName.sbm", collection.BuildFileNameShort);
        }

        [TestMethod]
        public void BuildFileNameFull_SetWithJustFileName_ShortNameMatchesFull()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();

            // Act
            collection.BuildFileNameFull = "JustFileName.sbm";

            // Assert
            Assert.AreEqual("JustFileName.sbm", collection.BuildFileNameFull);
            Assert.AreEqual("JustFileName.sbm", collection.BuildFileNameShort);
        }

        [TestMethod]
        public void Indexer_GetNewServer_CreatesEmptyDatabases()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();

            // Act
            var databases = collection["NewServer"];

            // Assert
            Assert.IsNotNull(databases);
            Assert.AreEqual(0, databases.Count);
        }

        [TestMethod]
        public void Indexer_GetSameServerTwice_ReturnsSameInstance()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();

            // Act
            var databases1 = collection["Server1"];
            var databases2 = collection["Server1"];

            // Assert
            Assert.AreSame(databases1, databases2);
        }

        [TestMethod]
        public void Indexer_SetNewServer_AddsToCollection()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();
            var databases = new Databases();

            // Act
            collection["Server1"] = databases;
            var retrieved = collection["Server1"];

            // Assert
            Assert.AreSame(databases, retrieved);
        }

        [TestMethod]
        public void Indexer_SetExistingServer_UpdatesValue()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();
            var databases1 = new Databases();
            var databases2 = new Databases();

            // Act
            collection["Server1"] = databases1;
            collection["Server1"] = databases2;
            var retrieved = collection["Server1"];

            // Assert
            Assert.AreSame(databases2, retrieved);
        }

        [TestMethod]
        public void ServerDict_Get_ReturnsServerDictionary()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();

            // Act
            var dict = collection.ServerDict;

            // Assert
            Assert.IsNotNull(dict);
            Assert.IsInstanceOfType(dict, typeof(ServerDictionary));
        }

        [TestMethod]
        public void ServerDict_Set_UpdatesDictionary()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();
            var newDict = new ServerDictionary();
            newDict.Add("TestServer", new Databases());

            // Act
            collection.ServerDict = newDict;

            // Assert
            Assert.AreSame(newDict, collection.ServerDict);
            Assert.IsTrue(collection.ServerDict.ContainsKey("TestServer"));
        }

        [TestMethod]
        public void MultipleServers_AddedViaIndexer_AllAccessible()
        {
            // Arrange
            var collection = CreateServerStatusDataCollection();

            // Act
            var _ = collection["Server1"];
            var __ = collection["Server2"];
            var ___ = collection["Server3"];

            // Assert
            Assert.AreEqual(3, collection.ServerDict.Count);
        }

        // Helper to create ServerStatusDataCollection (internal constructor)
        private ServerStatusDataCollection CreateServerStatusDataCollection()
        {
            // Use XML serialization to create instance since constructor is internal
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            using (var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>"))
            {
                return (ServerStatusDataCollection)serializer.Deserialize(reader)!;
            }
        }
    }

    [TestClass]
    public class StatusDataCollectionTests
    {
        [TestMethod]
        public void Constructor_Default_CreatesEmptyList()
        {
            // Act
            var collection = new StatusDataCollection();

            // Assert
            Assert.IsNotNull(collection);
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void Add_ScriptStatusData_IncreasesCount()
        {
            // Arrange
            var collection = new StatusDataCollection();
            var data = new ScriptStatusData { ScriptName = "Test.sql" };

            // Act
            collection.Add(data);

            // Assert
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void AddMultiple_ScriptStatusData_AllAccessible()
        {
            // Arrange
            var collection = new StatusDataCollection();
            var data1 = new ScriptStatusData { ScriptName = "Script1.sql" };
            var data2 = new ScriptStatusData { ScriptName = "Script2.sql" };
            var data3 = new ScriptStatusData { ScriptName = "Script3.sql" };

            // Act
            collection.Add(data1);
            collection.Add(data2);
            collection.Add(data3);

            // Assert
            Assert.AreEqual(3, collection.Count);
            Assert.AreEqual("Script1.sql", collection[0].ScriptName);
            Assert.AreEqual("Script2.sql", collection[1].ScriptName);
            Assert.AreEqual("Script3.sql", collection[2].ScriptName);
        }

        [TestMethod]
        public void Remove_ScriptStatusData_DecreasesCount()
        {
            // Arrange
            var collection = new StatusDataCollection();
            var data = new ScriptStatusData { ScriptName = "Test.sql" };
            collection.Add(data);

            // Act
            collection.Remove(data);

            // Assert
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            var collection = new StatusDataCollection();
            collection.Add(new ScriptStatusData { ScriptName = "Script1.sql" });
            collection.Add(new ScriptStatusData { ScriptName = "Script2.sql" });

            // Act
            collection.Clear();

            // Assert
            Assert.AreEqual(0, collection.Count);
        }
    }

    [TestClass]
    public class ServerDictionaryTests
    {
        [TestMethod]
        public void Constructor_Default_InitializesWithCorrectNames()
        {
            // Act
            var dict = new ServerDictionary();

            // Assert
            Assert.AreEqual("Server", dict.ItemName);
            Assert.AreEqual("ServerName", dict.KeyName);
            Assert.AreEqual("Databases", dict.ValueName);
        }

        [TestMethod]
        public void Servers_Property_ReturnsKeys()
        {
            // Arrange
            var dict = new ServerDictionary();
            dict.Add("Server1", new Databases());
            dict.Add("Server2", new Databases());

            // Act
            var servers = dict.Servers;

            // Assert
            Assert.AreEqual(2, servers.Count);
        }

        [TestMethod]
        public void Databases_Property_ReturnsValues()
        {
            // Arrange
            var dict = new ServerDictionary();
            dict.Add("Server1", new Databases());
            dict.Add("Server2", new Databases());

            // Act
            var databases = dict.Databases;

            // Assert
            Assert.AreEqual(2, databases.Count);
        }
    }

    [TestClass]
    public class DatabasesTests
    {
        [TestMethod]
        public void Constructor_Default_InitializesWithCorrectNames()
        {
            // Act
            var databases = new Databases();

            // Assert
            Assert.AreEqual("Database", databases.ItemName);
            Assert.AreEqual("DatabaseName", databases.KeyName);
            Assert.AreEqual("Scripts", databases.ValueName);
        }

        [TestMethod]
        public void Add_StatusDataCollection_CanBeRetrieved()
        {
            // Arrange
            var databases = new Databases();
            var scripts = new StatusDataCollection();
            scripts.Add(new ScriptStatusData { ScriptName = "Test.sql" });

            // Act
            databases.Add("TestDB", scripts);

            // Assert
            Assert.IsTrue(databases.ContainsKey("TestDB"));
            Assert.AreEqual(1, databases["TestDB"].Count);
        }
    }
}
