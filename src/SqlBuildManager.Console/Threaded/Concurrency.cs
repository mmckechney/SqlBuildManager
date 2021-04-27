using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using MoreLinq;
using System.Linq;
using SqlSync.Connection;

namespace SqlBuildManager.Console.Threaded
{
    public class Concurrency
    {
        public static List<IEnumerable<(string, List<DatabaseOverride>)>> ConcurrencyByType(MultiDbData multiData, int concurrency, ConcurrencyType concurrencyType)
        {
            switch(concurrencyType)
            {
                case ConcurrencyType.Server:
                    return ConcurrencyByServer(multiData);
                case ConcurrencyType.MaxPerServer:
                    return MaxConcurrencyByServer(multiData, concurrency);
                case ConcurrencyType.Count:
                default:
                    return ConcurrencyByInt(multiData, concurrency);
            }
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
            foreach (ServerData srv in multiData)
            {
                var lstSrv = new List<(string, List<DatabaseOverride>)>();
                foreach (List<DatabaseOverride> ovr in srv.OverrideSequence.Values)
                {
                    lstSrv.Add((srv.ServerName, ovr));
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
            foreach(var sC in serverChunks)
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
                foreach (List<DatabaseOverride> ovr in srv.OverrideSequence.Values)
                {
                    flattened.Add((srv.ServerName, ovr));
                }
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
            if(over.Count() > 0)
            {
                consolidated.AddRange(over);
                over.ToList().ForEach(o => buckets.Remove(o));
            }

            //Special case... is the number of buckets close to the number of servers? If so, do minumum consolidation
            var gap = Math.Abs((consolidated.Count() + buckets.Count()) - fixedBucketCount);
            if (gap <= 6 && fixedBucketCount / gap > 2)
            {
                //Combine the smallest buckets until we hit the fixed bucket count
                while(buckets.Count() > 0 &&  consolidated.Count() + buckets.Count() > fixedBucketCount)
                {
                    var nextTwo = buckets.OrderBy(b => b.Count()).Take(2).ToList();
                    var tmp = new List<(string, List<DatabaseOverride>)>();
                    nextTwo.ForEach(n => tmp.AddRange(n));
                    consolidated.Add(tmp);
                    nextTwo.ForEach(o => buckets.Remove(o));
                }

                //now just add the remaining buckets to the consolidated collection
                buckets.ForEach(b => consolidated.Add(b));
                buckets.Clear();
            }
            else
            {
                //Start creating -- fill as best to start, but not exceeding the ideal size and equalling the bucket cound
                while (buckets.Count() > 0)
                {
                    int bucketSum = 0;
                    var nextSet = buckets.OrderByDescending(b => b.Count()).TakeWhile(p =>
                    {
                        bool exceeded = bucketSum > idealBucketSize;
                        bucketSum += p.Count();
                        return !exceeded;
                    });
                    var nextTmp = nextSet.ToList();
                    if (nextTmp.Count() > 0)
                    {
                        while (nextTmp.Sum(n => n.Count()) > idealBucketSize)
                        {
                            nextTmp.RemoveAt(nextTmp.Count() - 1);
                        }
                        var tmp = new List<(string, List<DatabaseOverride>)>();
                        foreach (var n in nextTmp)
                        {
                            tmp.AddRange(n);
                        }
                        nextTmp.ForEach(o => buckets.Remove(o));
                        consolidated.Add(tmp);
                    }
                    if (consolidated.Count() == fixedBucketCount)
                    {
                        break;
                    }
                }

                //Capture any left over buckets
                if (buckets.Count() > 0)
                {
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
            }

          
            if(itemCheckSum != consolidated.Sum(c => c.Count()))
            {
                throw new Exception($"While filling concurrency buckets, the end count of {consolidated.Sum(c => c.Count())} does not equal the start count of {itemCheckSum}");
            }
            return consolidated;

        }

        public static List<string[]> ConvertBucketsToConfigLines(List<IEnumerable<(string, List<DatabaseOverride>)>> buckets)
        {
            List<string[]> bucketStrings = new List<string[]>();
            foreach(var bucket in buckets)
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
