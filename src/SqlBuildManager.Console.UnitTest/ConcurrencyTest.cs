using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        internal static (string, MultiDbData) CreateDefinedMultiDbData(int serverCount, int[] dbCount)
        {
            if (serverCount != dbCount.Length)
            {
                return ("", null);
            }
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cfg";
            StringBuilder sb = new StringBuilder();
            for (int s = 0; s < serverCount; s++)
            {
                int tmpDbCount = dbCount[s];
                for (int d = 0; d < tmpDbCount; d++)
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
        internal static (string, MultiDbData) CreateRandomizedMultiDbData(int serverCount, int minDbCount, int maxDbCount, out int[] matrix)
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cfg";
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            matrix = new int[serverCount];
            for (int s = 0; s < serverCount; s++)
            {
                var dbCount = rnd.Next(minDbCount, maxDbCount + 1);
                matrix[s] = dbCount;
                for (int d = 0; d < dbCount; d++)
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
            try
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
                        targetBuckets = rnd.Next(2, 90);
                        serverCount = rnd.Next(targetBuckets + 1, 300);
                        minDbCount = rnd.Next(10, 150);
                        maxDbCount = rnd.Next(minDbCount + 1, 500);
                        int[] matrix;
                        (tmpFile, multiData) = CreateRandomizedMultiDbData(serverCount, minDbCount, maxDbCount, out matrix);

                        //The real work 
                        var buckets = Concurrency.RecombineServersToFixedBucketCount(multiData, targetBuckets);

                        //Numbers for comparisons
                        var flattened = Concurrency.ConcurrencyByServer(multiData);
                        var idealBucket = Math.Ceiling((double)flattened.Sum(c => c.Count()) / (double)targetBuckets);
                        int maxBucket = flattened.Where(c => c.Count() <= idealBucket).Max(c => c.Count()); //exclude the buckets that were already above the ideal
                        int medianBucket = flattened.OrderBy(c => c.Count()).ToList()[(flattened.Count() / 2) + 1].Count();

                        string message = $"Buckets: {targetBuckets}; Servers: {serverCount}; Matrix: {string.Join(",", matrix)}";
                        Assert.AreEqual(targetBuckets, buckets.Count(), message);
                        Assert.IsTrue(buckets.Max(c => c.Count()) < maxBucket + (idealBucket * 1.2), message);


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
            catch (OutOfMemoryException)
            {
                //GitHub actions sometimes will run out of memory running this test!
            }

        }
        [DataRow(88,91,  new int[] { 170, 26, 159, 252, 249, 268, 225, 253, 255, 151, 37, 235, 179, 259, 189, 88, 130, 58, 115, 85, 266, 65, 204, 27, 103, 72, 90, 258, 170, 163, 217, 196, 267, 137, 282, 171, 244, 280, 199, 190, 114, 238, 175, 178, 268, 258, 131, 220, 101, 187, 263, 58, 27, 78, 108, 131, 110, 158, 174, 203, 203, 256, 108, 118, 223, 130, 257, 67, 265, 43, 132, 138, 147, 213, 259, 54, 178, 52, 47, 133, 229, 159, 182, 272, 188, 184, 194, 215, 55, 177, 87 } )] //Actual 86
        [DataRow(80, 81, new int[] { 185, 159, 72, 186, 41, 39, 101, 74, 75, 177, 27, 151, 243, 22, 48, 64, 141, 123, 102, 24, 38, 166, 19, 57, 26, 148, 23, 56, 97, 171, 138, 49, 29, 240, 84, 246, 58, 243, 151, 45, 170, 125, 174, 251, 225, 219, 199, 116, 135, 60, 248, 245, 79, 186, 93, 29, 150, 25, 169, 156, 26, 55, 240, 167, 49, 242, 132, 13, 201, 65, 16, 162, 235, 66, 117, 176, 256, 31, 95, 71, 213 })]
        [DataRow(39, 47, new int[] { 175, 179, 203, 92, 153, 230, 154, 166, 247, 213, 39, 207, 192, 208, 42, 211, 212, 35, 282, 66, 82, 45, 94, 72, 124, 212, 118, 235, 263, 138, 30, 239, 99, 271, 114, 189, 25, 80, 31, 217, 255, 192, 81, 40, 84, 244, 178 })] //Actual:<38>
        [DataRow( 3,  8, new int[] { 92, 225, 126, 135, 266, 186, 280, 115 })]
        [DataRow(26, 27, new int[] { 554, 436, 194, 441, 382, 440, 337, 242, 85, 449, 513, 426, 475, 151, 507, 460, 138, 425, 529, 120, 262, 117, 123, 391, 344, 260, 119 })] //Actual:<23>
        [DataRow(32, 38, new int[] { 218, 532, 396, 63, 227, 207, 185, 106, 556, 453, 528, 476, 512, 395, 73, 487, 121, 75, 450, 560, 456, 199, 488, 413, 311, 439, 132, 405, 448, 238, 266, 101, 368, 84, 133, 171, 31, 276 })] //Actual:<30>
        [DataRow(48, 52, new int[] { 155, 365, 406, 341, 92, 116, 294, 268, 495, 239, 260, 250, 214, 101, 190, 212, 319, 277, 137, 316, 199, 428, 198, 353, 166, 408, 239, 45, 71, 458, 231, 140, 129, 117, 451, 211, 168, 320, 378, 448, 337, 161, 149, 99, 178, 198, 43, 151, 131, 211, 407, 361 })] // Actual:<46>.
        [DataRow(39, 40, new int[] { 475, 159, 167, 155, 263, 279, 342, 258, 255, 303, 433, 473, 356, 352, 188, 405, 395, 467, 431, 474, 162, 411, 427, 208, 458, 370, 295, 419, 135, 130, 455, 273, 440, 247, 233, 252, 406, 346, 445, 417 })] //Actual:<37>
        [DataRow(34, 37, new int[] { 512, 68, 299, 503, 442, 170, 200, 336, 435, 507, 124, 264, 509, 449, 18, 406, 238, 491, 42, 485, 240, 152, 388, 468, 510, 536, 380, 336, 371, 404, 334, 365, 161, 274, 135, 19, 153 })] //Actual:<31>
        [DataRow(32, 36, new int[] { 429, 295, 251, 206, 436, 155, 285, 203, 214, 89, 53, 70, 232, 194, 298, 87, 315, 298, 377, 412, 231, 270, 392, 286, 354, 299, 320, 235, 98, 87, 130, 75, 247, 56, 141, 441 })] //Actual:<30>
        [DataRow(21, 22, new int[] { 259, 68, 318, 114, 406, 462, 159, 322, 233, 288, 382, 151, 397, 294, 76, 347, 337, 282, 398, 444, 207, 128 })] //Actual:<19>
        [DataTestMethod]
        public void MatchDefinedServersToFixedBucket(int targetBuckets, int serverCount, int[] dbsPerServer)
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;
            try
            {
                (tmpFile, multiData) = CreateDefinedMultiDbData(serverCount, dbsPerServer);

                //The real work
                var buckets = Concurrency.RecombineServersToFixedBucketCount(multiData, targetBuckets);

                //Numbers for comparisons
                var flattened = Concurrency.ConcurrencyByServer(multiData);
                var idealBucket = Math.Ceiling((double)flattened.Sum(c => c.Count()) / (double)targetBuckets);
                int maxBucket = flattened.Where(c => c.Count() <= idealBucket).Max(c => c.Count()); //exclude the buckets that were already above the ideal
                int medianBucket = flattened.OrderBy(c => c.Count()).ToList()[(flattened.Count() / 2) + 1].Count();
                
                string message = $"Buckets: {targetBuckets}; Servers: {serverCount}; Matrix: {string.Join(",", dbsPerServer)}";
                Assert.AreEqual(targetBuckets, buckets.Count(), message);
                Assert.IsTrue(buckets.Max(c => c.Count()) < maxBucket + idealBucket + (idealBucket * 1.2), message);
  

                var str = Concurrency.ConvertBucketsToConfigLines(buckets);

                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
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
                    (tmpFile, multiData) = CreateRandomizedMultiDbData(serverCount, minDbCount, maxDbCount, out matrix);

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
