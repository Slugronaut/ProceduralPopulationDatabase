using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace ProceduralPopulationDatabase
{

    /// <summary>
    /// A sample of a total population as the result of a query.
    /// This is a monadic type that can be used to chain queries together.
    /// </summary>
    public sealed class PopulationSample : IEnumerable<int>
    {
        static List<IndexRange> TempList = new(16);
        static List<IndexRange> TempList2 = new(16);

        readonly public List<IndexRange> Ranges;
        readonly public PopulationTree SourceTree;

        public int Count => Ranges.Select(range => range.Length).Sum();


        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            for (int i = 0; i < Ranges.Count; i++)
            {
                for (int j = Ranges[i].StartIndex; j <= Ranges[i].EndIndex; j++)
                    yield return j;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Ranges.Count; i++)
            {
                for (int j = Ranges[i].StartIndex; j <= Ranges[i].EndIndex; j++)
                    yield return j;
            }
        }


        #region Public
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceCensus"></param>
        public PopulationSample(PopulationTree sourceCensus, List<IndexRange> ranges)
        {
            SourceTree = sourceCensus;
            Ranges = ranges;
        }

        /// <summary>
        /// Performs a query, retrieving all IndexRanges for the given percentage split
        /// of the given level of the <see cref="SourceTree"/>
        /// and then finds the intersection of thise IndexRanges with this current sample's IndexRanges.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PopulationSample Query(int level, int value)
        {
            SourceTree.GetPopulationIndexRangesList(level, ref TempList2);
            int groupSize = SourceTree.GetSliceGroupCount(level);
            SelectNthElementForEveryGroupOfM(TempList2, value, groupSize, ref TempList);

            var intersectingRanges = IndexRange.IntersectingRanges(Ranges, TempList);
            return new PopulationSample(SourceTree, intersectingRanges);
        }

        /// <summary>
        /// Performs a query, retrieving all IndexRanges for the given percentage split
        /// of the given level of the <see cref="SourceTree"/>
        /// and then removes them from this current sample's IndexRanges.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PopulationSample Exclude(int level, int value)
        {
            SourceTree.GetPopulationIndexRangesList(level, ref TempList2); //get the exclusion list
            int groupSize = SourceTree.GetSliceGroupCount(level);
            SelectNthElementForEveryGroupOfM(TempList2, value, groupSize, ref TempList);

            //NOTE: TempList2 is no longer needed so we are recycling it here as output from the difference function
            IndexRange.DifferenceRanges(Ranges, TempList, ref TempList2); //exclude them from our current pop, note that
            return new PopulationSample(SourceTree, TempList2);
        }
        #endregion


        #region Static
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
            TempList.Clear();
            var outputList = TempList;// new List<IndexRange>();
            for (int i = n; i < inputList.Count; i += m)
                outputList.Add(inputList[i]);

            return outputList;
        }

        /// <summary>
        /// Each level of our population tree is split into groups of m elements but they are obtained as a flat array.
        /// This selects the nth element of every group of m.
        /// This is a non-allocating version.
        /// </summary>
        /// <returns></returns>
        public static void SelectNthElementForEveryGroupOfM(List<IndexRange> inputList, int n, int m, ref List<IndexRange> output)
        {
            output.Clear();
            for (int i = n; i < inputList.Count; i += m)
                output.Add(inputList[i]);

        }
        #endregion
    }
}
