using SqlBuildManager.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild;
using SqlBuildManager.Interfaces.Console;
using System.IO;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using SqlBuildManager.Console.Threaded;
using System.Text;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass()]
    public partial class ConcurrencyTest
    {
        internal static (string, MultiDbData) GetMultiDbData()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cfg";

            File.WriteAllBytes(tmpCfg, Properties.Resources.concurrency);
            MultiDbData multiData;
            string[] errorMessages;
            CommandLineArgs cmdLine = new CommandLineArgs()
            {
                MultiDbRunConfigFileName = tmpCfg
            };
            string message = string.Empty;

            int tmpValReturn = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out multiData, out errorMessages);
            if (tmpValReturn != 0)
            {
                var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                throw new Exception(String.Join(";", errorMessages));
            }

            return (tmpCfg, multiData);
        }

        internal static (string, MultiDbData) CreateMultiDbData(int serverCount, int minDbCount, int maxDbCount, out int[] matrix)
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cfg";
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            matrix = new int[serverCount];
            for (int s=0;s< serverCount;s++)
            {
                var dbCount = rnd.Next(minDbCount, maxDbCount + 1);
                matrix[s] = dbCount;
                for(int d = 0;d<dbCount;d++)
                {
                    sb.AppendLine($"server{s}:default,database{d}");
                }
            }
            File.WriteAllText(tmpCfg, sb.ToString());
            MultiDbData multiData;
            string[] errorMessages;
            CommandLineArgs cmdLine = new CommandLineArgs()
            {
                MultiDbRunConfigFileName = tmpCfg
            };
            string message = string.Empty;

            int tmpValReturn = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out multiData, out errorMessages);
            if (tmpValReturn != 0)
            {
                var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                throw new Exception(String.Join(";", errorMessages));
            }

            return (tmpCfg, multiData);
        }
        [TestMethod]
        public void MatchServersToFixedBucket()
        {
            int targetBuckets = 3;
            int serverCount = 40;
            int minDbCount = 10;
            int maxDbCount = 500;
            Random rnd = new Random();
        
            string tmpFile = string.Empty;
            MultiDbData multiData;
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    targetBuckets = rnd.Next(2, 51);
                    serverCount = rnd.Next(targetBuckets + 1, 400);
                    minDbCount = rnd.Next(10, 201);
                    maxDbCount = rnd.Next(minDbCount+1 , 600);
                    int[] matrix;
                    (tmpFile, multiData) = CreateMultiDbData(serverCount, minDbCount, maxDbCount,out matrix);
                    
                    var buckets = Concurrency.RecombineServersToFixedBucketCount(multiData, targetBuckets);
                    var flattened = Concurrency.ConcurrencyByServer(multiData);
                    int maxBucket = flattened.Max(c => c.Count());
                    int medianBucket = flattened.OrderBy(c => c.Count()).ToList()[(flattened.Count() / 2)+1].Count();
                    var idealBucket = Math.Ceiling((double)flattened.Sum(c => c.Count()) / (double)targetBuckets);
                    string message = $"Buckets: {targetBuckets}; Servers: {serverCount}; Matrix: {string.Join(",", matrix)}";
                    Assert.AreEqual(targetBuckets, buckets.Count(), message);
                    Assert.IsTrue(buckets.Max(c => c.Count()) <= idealBucket + maxBucket, message);

                    var str = Concurrency.ConvertBucketsToConfigLines(buckets);

                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                }

            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
            
        }

        [TestMethod]
        public void MatchServersToFixedBucket_ToConfigLines()
        {
            int targetBuckets = 3;
            int serverCount = 40;
            int minDbCount = 10;
            int maxDbCount = 500;
            Random rnd = new Random();

            string tmpFile = string.Empty;
            MultiDbData multiData;
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    targetBuckets = rnd.Next(2, 51);
                    serverCount = rnd.Next(targetBuckets + 1, 400);
                    minDbCount = rnd.Next(10, 201);
                    maxDbCount = rnd.Next(minDbCount + 1, 600);
                    int[] matrix;
                    (tmpFile, multiData) = CreateMultiDbData(serverCount, minDbCount, maxDbCount, out matrix);

                    var buckets = Concurrency.RecombineServersToFixedBucketCount(multiData, targetBuckets);
                    var str = Concurrency.ConvertBucketsToConfigLines(buckets);
                    string message = $"Buckets: {targetBuckets}; Servers: {serverCount}; Matrix: {string.Join(",", matrix)}";
                    Assert.AreEqual(buckets.Count(), str.Count(), message);
                    for (int j = 0; j < buckets.Count(); j++)
                    {
                        Assert.AreEqual(buckets[j].Count(), str[j].Count(), message);
                        Assert.AreEqual(buckets[j].First().Item1, str[j].First().Substring(0, str[j].First().IndexOf(":")), message);
                        Assert.AreEqual(buckets[j].Last().Item1, str[j].Last().Substring(0, str[j].Last().IndexOf(":")), message);
                    }

                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                }

            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }

        }

        [TestMethod]
        public void DbOverrideFlattenTest()
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;
           
            try
            {
                (tmpFile, multiData) = GetMultiDbData();
                var flattened = Concurrency.FlattenOverride(multiData);
                Assert.AreEqual(2366, flattened.Count);

            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(234)]
        [DataTestMethod]
        public void DbOverrideSimpleBatchTest(int concurrencyCount)
        {
            
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = GetMultiDbData();
                var flattened = Concurrency.FlattenOverride(multiData);
                int totalRowCount = flattened.Count();
                double expectedChunks = Math.Ceiling((double)totalRowCount /(double) concurrencyCount);
                int lastBatchSize = totalRowCount;
              
                var output = Concurrency.ConcurrencyByInt(multiData, concurrencyCount);
                Assert.IsTrue(expectedChunks <= output[0].Count() && output[0].Count() >= expectedChunks-1);
                Assert.AreEqual(concurrencyCount, output.Count());
               // Assert.AreEqual(lastBatchSize, output.Last().Count());
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }

        [TestMethod]
        public void DbOverrideConcurrencyByServerTest()
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = GetMultiDbData();
                var output = Concurrency.ConcurrencyByServer(multiData);
                Assert.AreEqual(449, output[0].Count());
                Assert.AreEqual(5, output.Count());
                Assert.AreEqual(96, output.Last().Count());
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }

        [DataRow(2,10,225,48)]
        [DataRow(5, 25, 90, 16)]
        [DataRow(10, 50, 45, 6)]
        [DataRow(20, 100, 23, 1)]
        [DataTestMethod]
        public void DbOverrideMaxConcurrencyByServerTest(int concurrency, int totalChunks, int firstCount, int lastCount)
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = GetMultiDbData();
                var output = Concurrency.MaxConcurrencyByServer(multiData, concurrency);
                Assert.AreEqual(totalChunks, output.Count());
                Assert.AreEqual(firstCount, output.First().Count());
                Assert.AreEqual(lastCount, output.Last().Count());
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }

        [DataRow(224, 7558)]
        [DataRow(195, 5901)]
        [DataRow(2, 624)]
        [DataRow(2, 449)]
        [DataRow(2, 511)]
        [DataRow(2, 1102)]
        [DataRow(2, 208)]
        [DataRow(2, 96)]
        [DataRow(5, 449)]
        [DataRow(5, 511)]
        [DataRow(5, 1102)]
        [DataRow(5, 208)]
        [DataRow(5, 96)]
        [DataRow(10, 449)]
        [DataRow(10, 511)]
        [DataRow(10, 1102)]
        [DataRow(10, 208)]
        [DataRow(10, 96)]
        [DataRow(20, 449)]
        [DataRow(20, 511)]
        [DataRow(20, 1102)]
        [DataRow(20, 208)]
        [DataRow(20, 96)]
        [DataRow(5, 412)]
        [DataRow(7, 411)]
        [DataRow(10, 364)]
        [DataRow(10, 51)]
        [DataTestMethod]
        public void ChunkAlgoTest(int numberOfChunks, int listSize)
        {
            double max = Math.Ceiling((double)listSize / (double)numberOfChunks);
            var lst = new List<int>();
            for (int i = 0; i < listSize; i++)
            {
                lst.Add(i);
            }

            var chunked = lst.SplitIntoChunks(numberOfChunks);
            Assert.AreEqual(numberOfChunks, chunked.Count());

            if (numberOfChunks % listSize == 0)
            {
                Assert.AreEqual(chunked.First().Count(), chunked.Last().Count()); //evenly divisable should always be equal
            }
            else
            {
                Assert.IsTrue(chunked.First().Count() <= max); //chunks should not be larger that the mathematical ceiling
                Assert.IsTrue(chunked.Skip(numberOfChunks / 2).First().Count() <= max); //grab one near the middle
                Assert.IsTrue(chunked.Last().Count() <= max * 1.2); //last is not be larger than max
            }
        }
        [TestMethod]
        public void ChunkAlgo_RandomTest()
        {
            int numberOfChunks, listSize;
            Random rnd = new Random();
            int counter = 0;

            while (counter < 5000)
            {
                numberOfChunks = rnd.Next(2, 250);
                listSize = rnd.Next(numberOfChunks, 50000);
                double max = Math.Ceiling((double)listSize / (double)numberOfChunks);
                var lst = new List<int>();
                for (int i = 0; i < listSize; i++)
                {
                    lst.Add(i);
                }

                var chunked = lst.SplitIntoChunks(numberOfChunks);
                Assert.AreEqual(numberOfChunks, chunked.Count());

                if (numberOfChunks % listSize == 0)
                {
                    Assert.AreEqual(chunked.First().Count(), chunked.Last().Count()); //evenly divisable should always be equal
                }
                else
                {
                    Assert.IsTrue(chunked.First().Count() <= max); //chunks should not be larger that the mathematical ceiling
                    Assert.IsTrue(chunked.Skip(numberOfChunks/2).First().Count() <= max); //grab one near the middle
                    Assert.IsTrue(chunked.Last().Count() <= max * 1.2); //last is not be larger than max
                }

                counter++;
            }


        }
    }
}
