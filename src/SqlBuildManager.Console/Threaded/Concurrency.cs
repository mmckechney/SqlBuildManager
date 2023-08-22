using Microsoft.Extensions.Logging;
using MoreLinq;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SqlBuildManager.Console.Threaded
{
    public class Concurrency
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static List<IEnumerable<(string, List<DatabaseOverride>)>> ConcurrencyByType(MultiDbData multiData, int concurrency, ConcurrencyType concurrencyType)
        {
            switch (concurrencyType)
            {
                case ConcurrencyType.Server:
                    return ConcurrencyByServer(multiData);
                case ConcurrencyType.MaxPerServer:
                    return MaxConcurrencyByServer(multiData, concurrency);
                
                case ConcurrencyType.Tag:
                    return ConcurrencyByTag(multiData);
                case ConcurrencyType.MaxPerTag:
                    return MaxConcurrencyByTag(multiData, concurrency);
                
                case ConcurrencyType.Count:
                default:
                    return ConcurrencyByInt(multiData, concurrency);
            }
        }

        private static List<IEnumerable<(string, List<DatabaseOverride>)>> MaxConcurrencyByTag(MultiDbData multiData, int concurrency)
        {
            List<IEnumerable<(string, List<DatabaseOverride>)>> tmp = new List<IEnumerable<(string, List<DatabaseOverride>)>>();
            var serverChunks = ConcurrencyByTag(multiData);
            foreach (var sC in serverChunks)
            {

                var subChunks = sC.SplitIntoChunks(concurrency);
                tmp.AddRange(subChunks);
            }
            return tmp;
        }

        private static List<IEnumerable<(string, List<DatabaseOverride>)>> ConcurrencyByTag(MultiDbData multiData)
        {
            List<IEnumerable<(string, List<DatabaseOverride>)>> tmp = new();

            //get a single list of db overrides
            var lstOverrides = new List<DatabaseOverride>();
            FlattenOverride(multiData).Select(f => f.Item2).ForEach(o => lstOverrides.AddRange(o));

            var tagGroup = lstOverrides.GroupBy(o => o.ConcurrencyTag);
            foreach (var s in tagGroup)
            {
                var lstSrv = new List<(string, List<DatabaseOverride>)>();
                foreach (var o in s)

                    if (lstSrv.Where(x => $"#{x.Item1}" == $"#{s.Key}").Any())
                    {
                        lstSrv.Where(x => $"#{x.Item1}" == $"#{s.Key}").First().Item2.Add(o);
                    }
                    else
                    {
                        lstSrv.Add(($"#{s.Key}", new List<DatabaseOverride>() { o }));
                    }
                tmp.Add(lstSrv);
            }

            var emptyTag = tmp.Where(t => t.Where(a => a.Item1 == "#").Any()).Any();
            if(emptyTag)
            {
                string msg = "Empty database target tags found. Please ensure all database targets have a tag when using ConcurrencyType of `Tag` or `MaxPerTag`";
                log.LogError(msg);
                throw new Exception(msg);
            }
            return tmp;
        }

        /// <summary>
        /// Divides the targets into the "concurrency" count of Lists. The lists would be run in parallel, with the items in each list run in serial
        /// Fully parallel to the extent of the concurrency number
        /// </summary>
        /// <param name="multiData">The database orveride object</param>
        /// <param name="concurrency">The number of lists to return</param>
        /// <returns>"Concurrency" number of target lists </returns>
        public static List<IEnumerable<(string, List<DatabaseOverride>)>> ConcurrencyByInt(MultiDbData multiData, int concurrency)
        {
            var flattened = FlattenOverride(multiData);
            var batches = flattened.SplitIntoChunks(concurrency).ToList(); ;
            return batches;
        }
        /// <summary>
        ///  Divides the targets into a list per database server. The lists would be run in parallel, with the items in each list run in serial
        ///  Would be used to "single thread" per server while parallel across servers
        /// </summary>
        /// <param name="multiData">The database orveride object</param>
        /// <returns>One list per Server</returns>
        public static List<IEnumerable<(string, List<DatabaseOverride>)>> ConcurrencyByServer(MultiDbData multiData)
        {
            List<IEnumerable<(string, List<DatabaseOverride>)>> tmp = new List<IEnumerable<(string, List<DatabaseOverride>)>>();
            var serverGroup = multiData.GroupBy(m => m.ServerName);
            foreach (var s in serverGroup)
            {
                var lstSrv = new List<(string, List<DatabaseOverride>)>();
                foreach (var o in s)
                {
                    lstSrv.Add((o.ServerName, o.Overrides.Select(d => d).ToList()));
                }
                tmp.Add(lstSrv);
            }

            return tmp;
        }

        /// <summary>
        ///  Divides the targets into "concurrency" number of lists per database server. The lists would be run in parallel, with the items in each list run in serial
        ///  Would be used to control the number of threads targeting a single server
        /// </summary>
        /// <param name="multiData"></param>
        /// <param name="concurrency"></param>
        /// <returns>"Concurrency" number of lists per Server</returns>
        public static List<IEnumerable<(string, List<DatabaseOverride>)>> MaxConcurrencyByServer(MultiDbData multiData, int concurrency)
        {
            List<IEnumerable<(string, List<DatabaseOverride>)>> tmp = new List<IEnumerable<(string, List<DatabaseOverride>)>>();
            var serverChunks = ConcurrencyByServer(multiData);
            foreach (var sC in serverChunks)
            {

                var subChunks = sC.SplitIntoChunks(concurrency);
                tmp.AddRange(subChunks);
            }
            return tmp;
        }

        public static List<(string, List<DatabaseOverride>)> FlattenOverride(MultiDbData multiData)
        {
            var flattened = new List<(string, List<DatabaseOverride>)>();
            foreach (ServerData srv in multiData)
            {
                flattened.Add((srv.ServerName, srv.Overrides));
            }
            return flattened;
        }

        public static List<IEnumerable<(string, List<DatabaseOverride>)>> RecombineServersToFixedBucketCount(MultiDbData multiData, int fixedBucketCount)
        {
            List<IEnumerable<(string, List<DatabaseOverride>)>> consolidated = new List<IEnumerable<(string, List<DatabaseOverride>)>>();
            //Get a bucket per server
            var buckets = ConcurrencyByServer(multiData);
            int itemCheckSum = buckets.Sum(b => b.Count());

            //get the ideal number of items per bucket
            double idealBucketSize = Math.Ceiling((double)buckets.Sum(b => b.Count()) / (double)fixedBucketCount);

            //Get any that are already over the ideal size
            var over = buckets.Where(b => b.Count() >= idealBucketSize);
            if (over.Count() > 0)
            {
                consolidated.AddRange(over);
                over.ToList().ForEach(o => buckets.Remove(o));
            }

            //If we have taken some oversize items out, we need to re-calc the ideal bucket size for the remaining buckets
            idealBucketSize = Math.Ceiling((double)buckets.Sum(b => b.Count()) / ((double)fixedBucketCount - consolidated.Count()));


            //Start creating -- fill as best to start, but not exceeding the ideal size and equalling the bucket count
            while (buckets.Count() > 0)
            {

                int bucketSum = 0;
                //get the next group -- note this will actually return a collection that is just over the ideal size
                var nextSet = buckets.OrderByDescending(b => b.Count()).TakeWhile(p =>
                {
                    bool exceeded = bucketSum > idealBucketSize;
                    bucketSum += p.Count();
                    return !exceeded;
                });

                var nextTmp = nextSet.ToList();
                if (nextTmp.Count() > 0)
                {
                    //trim the next group so that it's not too large
                    while (nextTmp.Sum(n => n.Count()) > idealBucketSize && nextTmp.Count() > 1)
                    {
                        nextTmp.RemoveAt(nextTmp.Count() - 1);
                    }

                    //If doing this combining would put us under the number of buckets needed, then add individual buckets to the desired count and let the clean up take care of the rest.
                    if (consolidated.Count + buckets.Count - nextTmp.Count < fixedBucketCount)
                    {
                        while (consolidated.Count() < fixedBucketCount && buckets.Count != 0)
                        {
                            consolidated.Add(buckets.First());
                            buckets.RemoveAt(0);
                        }
                        break;
                    }
                    //Combine the set into a single entry
                    var tmp = new List<(string, List<DatabaseOverride>)>();
                    foreach (var n in nextTmp)
                    {
                        tmp.AddRange(n);
                    }
                    //Remove those selected items from the original set of buckets
                    nextTmp.ForEach(o => buckets.Remove(o));

                    //Add the combined set into the consolidated list
                    consolidated.Add(tmp);

                    //if any further grouping will result in too few buckets, then add the remaining buckets individually
                    if (consolidated.Count() + buckets.Count() == fixedBucketCount && buckets.Count() > 0)
                    {
                        buckets.ForEach(b => consolidated.Add(b));
                        buckets.Clear();
                        break;
                    }


                }
                //did we hit the max number of buckets? if so, break out. We'll deal with any left overs next
                if (consolidated.Count() == fixedBucketCount)
                {
                    break;
                }
            }

            //Capture any left over buckets, loop to add the largest remaining buckets to the smallest buckets in the consolidated list
            if (buckets.Count() > 0)
            {
                buckets = buckets.OrderByDescending(c => c.Count()).ToList();
                consolidated = consolidated.OrderBy(c => c.Count()).ToList();
                while (buckets.Count() > 0)
                {
                    for (int i = 0; i < consolidated.Count(); i++)

                    {
                        if (buckets.Count() == 0) break;
                        var t = consolidated[i].ToList();
                        t.AddRange(buckets.First());
                        consolidated[i] = t;
                        buckets.RemoveAt(0);
                    }
                }
            }

            //Make sure we didn't lose anything!!
            if (itemCheckSum != consolidated.Sum(c => c.Count()))
            {
                throw new Exception($"While filling concurrency buckets, the end count of {consolidated.Sum(c => c.Count())} does not equal the start count of {itemCheckSum}");
            }
            return consolidated;

        }

        public static List<string[]> ConvertBucketsToConfigLines(List<IEnumerable<(string, List<DatabaseOverride>)>> buckets)
        {
            List<string[]> bucketStrings = new List<string[]>();
            foreach (var bucket in buckets)
            {
                var tmp = new List<string>();
                foreach (var sub in bucket)
                {
                    tmp.Add($"{sub.Item1}:{sub.Item2.ToList().Select(d => $"{d.DefaultDbTarget},{d.OverrideDbTarget}").Aggregate((a, b) => $"{a};{b}")}");
                }
                bucketStrings.Add(tmp.ToArray());
            }
            return bucketStrings;
        }
    }
}
