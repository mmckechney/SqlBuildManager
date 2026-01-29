using System;
using System.Collections.Generic;
using System.Linq;
namespace SqlSync.SqlBuild
{
    public static class Extensions
    {

        /// <summary>
        /// Splits a <see cref="List{T}"/> into multiple chunks.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be chunked.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <returns>A list of chunks.</returns>
        public static IEnumerable<IEnumerable<T>> SplitIntoChunks<T>(this IEnumerable<T> list, int numberOfChunks)
        {
            if (numberOfChunks <= 0)
            {
                throw new ArgumentException("numberOfChunks must be greater than 0.");
            }

            int listCount = list.Count();
            double dblChunks = (double)numberOfChunks;
            double maxChunkSize = Math.Ceiling(listCount / dblChunks);
            double minChunkSize = Math.Floor(listCount / dblChunks);
            double chunkSize;
            if (Math.Ceiling(listCount / maxChunkSize) < numberOfChunks)
            {
                chunkSize = minChunkSize;
            }
            else
            {
                chunkSize = maxChunkSize;
            }

            List<IEnumerable<T>> retVal = new List<IEnumerable<T>>();
            int index = 0;
            int usedChunkCount = 0;
            while (index < listCount && usedChunkCount < numberOfChunks)
            {
                int count = listCount - index > chunkSize ? (int)chunkSize : listCount - index;
                //If more than half way though and there are more items left than can fit in the remaining chunks -- start spreading them out. 
                // if(usedChunkCount > numberOfChunks/2 && index > listCount/2 && 
                if ((listCount - (double)index) / (dblChunks - usedChunkCount) > chunkSize)
                {
                    count++;
                }

                retVal.Add(list.ToList().GetRange(index, count).AsEnumerable());
                usedChunkCount++;
                index += (int)count;
            }

            //any leftovers should go into last chunk.
            if (index < listCount)
            {

                List<T> leftOvers = list.ToList().GetRange(index, listCount - index);
                List<T> lastChunk = retVal[retVal.Count - 1].ToList();
                lastChunk.AddRange(leftOvers.AsEnumerable());
                retVal[retVal.Count - 1] = lastChunk.AsEnumerable();
            }

            return retVal.AsEnumerable();
        }



        //public static T DeepClone<T>(this object obj) where T : class
        //{
        //    object cloned = new object();
        //    BinaryFormatter formatter = new BinaryFormatter();
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        formatter.Serialize(stream, obj);
        //        stream.Flush();
        //        stream.Position = 0;
        //        cloned = formatter.Deserialize(stream);
        //    }


        //    return cloned as T;

        //}

        //This works, but splits one at a time vs. chunking.. I want chunking.
        //Keeping the code just in case I want it later for something else. 
        //public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        //{
        //    int i = 0;

        //    var splits = from name in list
        //                 group name by i++ % parts into part
        //                 select part.AsEnumerable();
        //    return splits;
        //}
    }
}
