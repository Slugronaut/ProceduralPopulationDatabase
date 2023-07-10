using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Assertions;

namespace ProceduralPopulationDatabase
{
    /// <summary>
    /// A tree that stores a total population that can be subdivided recursively by various categories.
    /// The indices in the tree represent unique ids within the population range and map back to each value
    /// from 0 to the total population provided to the contructor.
    /// </summary>
    public class PopulationTree
    {
        static List<PopulationLevel> TempSlices = new(8);   //cached to help avoid garbage
        static readonly List<int> CachedRemap = new();
        static List<IndexRange> TempRanges = new(16);
        readonly PopulationLevel Levels;
        
        public int MaxDepth
        {
            get
            {
                var root = Levels;
                int depth = 1;
                while(root != null)
                {
                    if ((root.Children?.Length ?? 0) < 1)
                        break;
                    root = root.Children[0];
                    depth++;
                }
                return depth;
            }
        }
        public int PopulationSize => Levels.Count;


        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="popSize"></param>
        public PopulationTree(int popSize = 4_194_304)
        {
            Levels = new PopulationLevel(0, popSize);
        }

        /// <summary>
        /// Creates a series of slices of the total population at the given level.
        /// The depth must be greater than zero as that is the root of the tree. The depth is specifying what is to be created
        /// as a children of the previous depth, hence the reason zero cannot be given. The previous depth must already exist
        /// or an exception will be thrown.
        /// </summary>
        /// <param name="depth">The level that you would like to create. Effectively the level previous to this will be root of this one. Cannot be zero.</param>
        /// <param name="percents"></param>
        /// <exception cref="InvalidArgumentException"
        public void Slice(int depth, params float[] percents)
        {
            Assert.IsTrue(depth > 0, "A depth of zero cannot be specified as that already exists as the root.");
            Assert.IsTrue(depth <= this.MaxDepth, $"The depth specified is too great to be made the children of the current max depth. The current max depth allowed is {MaxDepth}, you specified {depth}.");

            TempSlices.Clear();
            GetSlicesAtDepth(depth-1, 0, Levels, TempSlices);

            var parentSlices = TempSlices;
            for(int sliceIndex = 0; sliceIndex < parentSlices.Count; sliceIndex++)
            {
                var parentSlice = parentSlices[sliceIndex];
                var parentRange = parentSlice.Range;
                parentSlice.Children = new PopulationLevel[percents.Length];
                var ranges = IndexRange.CalculatePercentageRanges(parentRange.StartIndex, parentRange.Length, percents);
                for (int rangeIndex = 0; rangeIndex < ranges.Length; rangeIndex++)
                    parentSlice.Children[rangeIndex] = new PopulationLevel(ranges[rangeIndex]);
            }
        }

        /// <summary>
        /// Creates a slice of the total population at a given level for a specific sibling of that level.
        /// This version allows specifying the percentages for each individual child.
        /// <seealso cref="Slice(int, float[])"/>
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <param name="percents"></param>
        public void JaggedSlice(int depth, float[][] percents)
        {
            Assert.IsTrue(depth > 0, "A depth of zero cannot be specified as that already exists as the root.");
            Assert.IsTrue(depth <= this.MaxDepth, $"The depth specified is too great to be made the children of the current max depth. The current max depth allowed is {MaxDepth}, you specified {depth}.");

            TempSlices.Clear();
            GetSlicesAtDepth(depth - 1, 0, Levels, TempSlices);

            var slices = TempSlices;
            for (int sliceIndex = 0; sliceIndex < slices.Count; sliceIndex++)
            {
                var parentSlice = slices[sliceIndex];
                var parentRange = parentSlice.Range;
                parentSlice.Children = new PopulationLevel[percents[sliceIndex].Length];
                var ranges = IndexRange.CalculatePercentageRanges(parentRange.StartIndex, parentRange.Length, percents[sliceIndex]);
                for (int rangeIndex = 0; rangeIndex < ranges.Length; rangeIndex++)
                    parentSlice.Children[rangeIndex] = new PopulationLevel(ranges[rangeIndex]);
            }
        }

        /// <summary>
        /// Creates a slice of the total population at a given level for a specific sibling of that level.
        /// This version allows specifying the percentages for each individual child.
        /// <seealso cref="Slice(int, float[])"/>
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <param name="percents"></param>
        public void JaggedSlice(int depth, double[][] percents)
        {
            Assert.IsTrue(depth > 0, "A depth of zero cannot be specified as that already exists as the root.");
            Assert.IsTrue(depth <= this.MaxDepth, $"The depth specified is too great to be made the children of the current max depth. The current max depth allowed is {MaxDepth}, you specified {depth}.");

            TempSlices.Clear();
            GetSlicesAtDepth(depth - 1, 0, Levels, TempSlices);

            var slices = TempSlices;
            for (int sliceIndex = 0; sliceIndex < slices.Count; sliceIndex++)
            {
                var parentSlice = slices[sliceIndex];
                var parentRange = parentSlice.Range;
                parentSlice.Children = new PopulationLevel[percents[sliceIndex].Length];
                var ranges = IndexRange.CalculatePercentageRanges(parentRange.StartIndex, parentRange.Length, percents[sliceIndex]);
                for (int rangeIndex = 0; rangeIndex < ranges.Length; rangeIndex++)
                    parentSlice.Children[rangeIndex] = new PopulationLevel(ranges[rangeIndex]);
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

        //// <summary>
        /// Returns a list of <see cref="IndexRange"/>s representing the indices of a given level in the population.
        /// This is a non-allocating version.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="slices"></param>
        public void GetPopulationIndexRangesList(int depth, ref List<IndexRange> output)
        {
            Assert.IsTrue(depth >= 0);
            TempSlices.Clear();
            GetSlicesAtDepth(depth, 0, Levels, TempSlices);

            var slices = TempSlices;
            output.Clear();
            for (int i = 0; i < slices.Count; i++)
                output.Add(slices[i].Range);
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
        public PopulationSample Query => new(this, new List<IndexRange>(1) { Levels.Range });

        /// <summary>
        /// Remaps a uid from the population back to the indicies of each level of the tree in which it lies.
        /// This can be used to transform a uid which is an index of the total population and get the individual
        /// queries used to create it.
        /// 
        /// Please note that the returned list is interally cached and should be considered volitile.
        /// Assign it's values before making any successive calls to this method. NEVER cache a reference to it.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public List<int> Remap(int uid)
        {
            int maxDepth = MaxDepth;
            CachedRemap.Clear();

            int depth = 1;
            int previousDepthSize = 1; //multiplier that we need to determin what the 'true' index is in an otherwise flattened depth of the hierarchy
            TempSlices.Clear();
            while(depth < maxDepth)
            {
                TempRanges.Clear();
                GetPopulationIndexRangesList(depth, ref TempRanges);
                int flattenedIndex = -1;
                var ranges = TempRanges;
                for(int i = 0; i < ranges.Count; i++)
                {
                    var range = ranges[i];
                    if (range.Contains(uid))
                        flattenedIndex = i;
                }

                if (flattenedIndex == -1)
                    throw new InaccessabePopulationException(uid);

                //the list of ranges is flattened but we need to translate back
                //into a set of sets that the original hierarchy represented.
                int setIndex = flattenedIndex % (TempRanges.Count / previousDepthSize);
                previousDepthSize *= GetSliceGroupCount(depth); //keep multiplying with the current group count so we can track sets
                CachedRemap.Add(setIndex);
                depth++;
            }

            return CachedRemap;
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
            TempSlices = new List<PopulationLevel>(8);
            TempRanges = new(16);
        }

        #endregion

    }

}
