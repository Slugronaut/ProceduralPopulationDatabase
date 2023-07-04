using System.Collections.Generic;
using Unity.Collections;

namespace ProceduralPopulationDatabase
{

    /// <summary>
    /// A sample of a total population as the result of a query.
    /// This is a monadic type that can be used to chain queries together.
    /// </summary>
    public class PopulationSample
    {
        readonly public List<IndexRange> Ranges;
        readonly PopulationTree SourceCensus;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceCensus"></param>
        public PopulationSample(PopulationTree sourceCensus, List<IndexRange> ranges)
        {
            SourceCensus = sourceCensus;
            Ranges = ranges;
        }

        /// <summary>
        /// Performs a query, retrieving all IndexRanges for the given percentage split of the given level of the <see cref="SourceCensus"/>
        /// and then find the intersection of thise IndexRanges with this current samples IndexRanges.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PopulationSample Query(int level, int value)
        {
            var ranges = SourceCensus.GetPopulationIndexRangesList(level);
            int groupSize = SourceCensus.GetSliceGroupCount(level);
            var queriedRanges = SelectNthElementForEveryGroupOfM(ranges, value, groupSize);

            var intersectingRanges = IndexRange.IntersectingRanges(Ranges, queriedRanges);
            return new PopulationSample(SourceCensus, intersectingRanges);
        }

        /// <summary>
        /// Each level of our population tree is split into groups of m elements but they are obtained as a flat array.
        /// This selects the nth element of every group of m.
        /// </summary>
        /// <returns></returns>
        public static NativeArray<IndexRange> SelectNthElementForEveryGroupOfM(NativeArray<IndexRange> inputList, int n, int m)
        {
            int start = n;
            int len = inputList.Length;
            int size = (len - start) / m;

            var outputList = new NativeArray<IndexRange>(size, Allocator.Temp);
            for (int i = start, count = 0; i < len; i += m, count++)
                outputList[count] = inputList[i];

            return outputList;
        }

        /// <summary>
        /// Each level of our population tree is split into groups of m elements but they are obtained as a flat array.
        /// This selects the nth element of every group of m.
        /// </summary>
        /// <returns></returns>
        public static List<IndexRange> SelectNthElementForEveryGroupOfM(List<IndexRange> inputList, int n, int m)
        {
            var outputList = new List<IndexRange>();
            for (int i = n; i < inputList.Count; i += m)
                outputList.Add(inputList[i]);

            return outputList;
        }
    }
}
