using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProceduralPopulationDatabase
{
    /// <summary>
    /// A tree that stores a total population that can be subdivided recursively by various categories.
    /// The indices in the tree represent unique ids within the population range and map back to each value
    /// from 0 to the total population provided to the contructor. An internal array also
    /// stores a 64-bit value for each unique id in the population that can store packed state info.
    /// 
    /// Thus an entire population can be stored in a simple tree and a native array that is 8-bytes x populationSize.
    /// </summary>
    public class PopulationTree
    {
        static List<PopulationLevel> TempSlices = new(8);   //cached to help avoid garbage
        readonly PopulationLevel Levels;
        

        public int PopulationSize => Levels.Count;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="popSize"></param>
        public PopulationTree(int popSize = 4_194_304)
        {
            Levels = new PopulationLevel(0, popSize);
        }


        #region Public Methods
        /// <summary>
        /// Creates a series of slices of the total population at the given level.
        /// </summary>
        /// <param name="depth">The level that should be further sliced into sublevels.</param>
        /// <param name="percents"></param>
        public void Slice(int depth, params float[] percents)
        {
            if (depth < 0)
                throw new UnityException("Cannot slice the first level of the population. That is the full pop.");

            TempSlices.Clear();
            GetSlicesAtDepth(depth, 0, Levels, TempSlices);

            var slices = TempSlices;
            for(int si = 0; si < slices.Count; si++)
            {
                var parentSlice = slices[si];
                var parentRange = parentSlice.Range;
                parentSlice.Children = new PopulationLevel[percents.Length];
                var ranges = IndexRange.CalculatePercentageRanges(parentRange.StartIndex, parentRange.Length, percents);
                for (int ri = 0; ri < ranges.Length; ri++)
                    parentSlice.Children[ri] = new PopulationLevel(ranges[ri]);
            }
        }

        /// <summary>
        /// Returns a list of <see cref="IndexRange"/>s representing the indices of a given level in the population.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="slices"></param>
        public NativeArray<IndexRange> GetPopulationIndexRanges(int depth)
        {
            Assert.IsTrue(depth >= 0);
            TempSlices.Clear();
            GetSlicesAtDepth(depth, 0, Levels, TempSlices);

            var slices = TempSlices;
            var ranges = new NativeArray<IndexRange>(slices.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for(int i = 0; i < slices.Count; i++)
                ranges[i] = slices[i].Range;

            return ranges;
        }

        //// <summary>
        /// Returns a list of <see cref="IndexRange"/>s representing the indices of a given level in the population.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="slices"></param>
        public List<IndexRange> GetPopulationIndexRangesList(int depth)
        {
            Assert.IsTrue(depth >= 0);
            TempSlices.Clear();
            GetSlicesAtDepth(depth, 0, Levels, TempSlices);

            var slices = TempSlices;
            var ranges = new List<IndexRange>(slices.Count);
            for (int i = 0; i < slices.Count; i++)
                ranges.Add(slices[i].Range);

            return ranges;
        }

        /// <summary>
        /// Returns the number of slices at the given depth.
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public int GetSliceGroupCount(int depth)
        {
            Assert.IsTrue(depth >= 0);

            //we always have exactly 1 at the root
            if (depth == 0)
                return 1;

            var parent = Levels;
            int currDepth = 1;
            while (currDepth < depth)
            {
                parent = parent.Children[0];
                currDepth++;
            }

            return parent.Children.Length;
        }

        /// <summary>
        /// Creates a monadic type that can be queiried to extract specific ranges of the total population in this tree.
        /// </summary>
        /// <returns></returns>
        public PopulationSample Query()
        {
            return new PopulationSample(this, new List<IndexRange>(1)
                {
                    Levels.Range
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int RandomId()
        {
            throw new UnityException("Not yet implemented");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Helper method for seeking the PopulationSlices at a given level in this tree.
        /// </summary>
        /// <param name="targetDepth"></param>
        /// <param name="currDepth"></param>
        /// <param name="aggregatedSlices"></param>
        void GetSlicesAtDepth(int targetDepth, int currDepth, PopulationLevel node, List<PopulationLevel> aggregatedSlices)
        {
            Assert.IsNotNull(node);
            Assert.IsNotNull(aggregatedSlices);

            if (currDepth > targetDepth)
                return;

            if (currDepth == targetDepth)
            {
                aggregatedSlices.Add(node);
                return;
            }

            Assert.IsNotNull(node.Children);
            Assert.IsTrue(node.Children.Length > 0);
            var nodes = node.Children;
            for (int i = 0; i < nodes.Length; i++)
                GetSlicesAtDepth(targetDepth, currDepth + 1, nodes[i], aggregatedSlices);

        }
        #endregion


        #region Static Methods
        /// <summary>
        /// Helper method for resetting internally cached arrays. Mostly usef for testing purposes.
        /// </summary>
        public static void ResetCache()
        {
            TempSlices = null;
        }

        #endregion

    }

}
