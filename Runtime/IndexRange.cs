using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;


namespace ProceduralPopulationDatabase
{

    /// <summary>
    /// Represents a range of indicies in an array.
    /// </summary>
    readonly public struct IndexRange : IEnumerable<int>
    {
        readonly public int StartIndex;
        readonly public int Length;
        public int EndIndex => StartIndex + Length - 1;
        public bool Contains(int index) => Length > 0 && index >= StartIndex && index <= EndIndex;


        public IndexRange(int startIndex, int length)
        {
            Assert.IsTrue(startIndex >= 0);
            Assert.IsTrue(length >= 0);
            StartIndex = startIndex;
            Length = length;
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            for (int i = StartIndex; i <= EndIndex; i++)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = StartIndex; i <= EndIndex; i++)
                yield return i;
        }


        #region Static Methods
        /// <summary>
        /// Given a start value and range, return a list of IndexRanges representing a set of percentages of that range.
        /// The list of percentages must add up to approximately 1.0
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="percentages"></param>
        /// <returns></returns>
        public static IndexRange[] CalculatePercentageRanges(int start, int len, float[] percentages)
        {
            float totalPercentage = percentages.Sum();
            Assert.IsTrue(Mathf.Approximately(totalPercentage, 1.0f));


            IndexRange[] percentageRanges = new IndexRange[percentages.Length];
            int totalRangeLen = len;
            int accum = totalRangeLen;

            for (int i = 0; i < percentages.Length; i++)
            {
                int rangeLength = (int)Math.Round(totalRangeLen * percentages[i]);
                percentageRanges[i] = new(start, rangeLength);
                start += rangeLength;
                accum -= rangeLength;
            }

            //adjust the last range to cover any remaining values, not technically accurate but hey, we just need something close enough
            if (percentageRanges.Length > 0 && accum > 0)
            {
                IndexRange lastRange = percentageRanges[^1];
                lastRange = new IndexRange(lastRange.StartIndex, lastRange.Length + accum);
                percentageRanges[^1] = lastRange;
            }

            return percentageRanges;
        }

        /// <summary>
        /// Given a start value and range, return a list of IndexRanges representing a set of percentages of that range.
        /// The list of percentages must add up to approximately 1.0
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="percentages"></param>
        /// <returns></returns>
        public static IndexRange[] CalculatePercentageRanges(int start, int len, double[] percentages)
        {
            float totalPercentage = (float)percentages.Sum();
            Assert.IsTrue(Mathf.Approximately(totalPercentage, 1.0f));


            IndexRange[] percentageRanges = new IndexRange[percentages.Length];
            int totalRangeLen = len;
            int accum = totalRangeLen;

            for (int i = 0; i < percentages.Length; i++)
            {
                int rangeLength = (int)Math.Round(totalRangeLen * percentages[i]);
                percentageRanges[i] = new(start, rangeLength);
                start += rangeLength;
                accum -= rangeLength;
            }

            //adjust the last range to cover any remaining values, not technically accurate but hey, we just need something close enough
            if (percentageRanges.Length > 0 && accum > 0)
            {
                IndexRange lastRange = percentageRanges[^1];
                lastRange = new IndexRange(lastRange.StartIndex, lastRange.Length + accum);
                percentageRanges[^1] = lastRange;
            }

            return percentageRanges;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstList"></param>
        /// <param name="secondList"></param>
        /// <returns></returns>
        public static List<int> IntersectingIndices(List<IndexRange> firstList, List<IndexRange> secondList)
        {
            firstList.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));
            secondList.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));

            List<int> intersection = new();
            int i = 0, j = 0;

            while (i < firstList.Count && j < secondList.Count)
            {
                if (firstList[i].StartIndex + firstList[i].Length >= secondList[j].StartIndex && secondList[j].StartIndex + secondList[j].Length >= firstList[i].StartIndex)
                {
                    int start = math.max(firstList[i].StartIndex, secondList[j].StartIndex);
                    int end = math.min(firstList[i].StartIndex + firstList[i].Length - 1, secondList[j].StartIndex + secondList[j].Length - 1);

                    for (int index = start; index <= end; index++)
                    {
                        intersection.Add(index);
                    }
                }

                if (firstList[i].StartIndex + firstList[i].Length < secondList[j].StartIndex + secondList[j].Length)
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }

            return intersection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstList"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static List<IndexRange> IntersectingRanges(List<IndexRange> firstList, IndexRange second)
        {
            return IntersectingRanges(firstList, new List<IndexRange>(1) { second });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstList"></param>
        /// <param name="secondList"></param>
        /// <returns></returns>
        public static List<IndexRange> IntersectingRanges(List<IndexRange> firstList, List<IndexRange> secondList)
        {
            firstList.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));
            secondList.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));

            List<IndexRange> intersection = new(Math.Max(firstList.Count, secondList.Count));
            int i = 0, j = 0;

            while (i < firstList.Count && j < secondList.Count)
            {
                if (firstList[i].StartIndex + firstList[i].Length >= secondList[j].StartIndex && secondList[j].StartIndex + secondList[j].Length >= firstList[i].StartIndex)
                {
                    int start = math.max(firstList[i].StartIndex, secondList[j].StartIndex);
                    int end = math.min(firstList[i].StartIndex + firstList[i].Length - 1, secondList[j].StartIndex + secondList[j].Length - 1);
                    int count = end - start + 1;

                    if (count > 0 && firstList[i].Length > 0 && secondList[j].Length > 0)
                    {
                        intersection.Add(new IndexRange(start, count));
                    }
                }

                if (firstList[i].StartIndex + firstList[i].Length < secondList[j].StartIndex + secondList[j].Length)
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }

            return intersection;
        }

        /// <summary>
        /// Non-allocating version.
        /// </summary>
        /// <param name="firstList"></param>
        /// <param name="secondList"></param>
        /// <returns></returns>
        public static void IntersectingRanges(List<IndexRange> firstList, List<IndexRange> secondList, ref List<IndexRange> output)
        {
            firstList.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));
            secondList.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));

            output.Clear();
            int i = 0, j = 0;

            while (i < firstList.Count && j < secondList.Count)
            {
                if (firstList[i].StartIndex + firstList[i].Length >= secondList[j].StartIndex && secondList[j].StartIndex + secondList[j].Length >= firstList[i].StartIndex)
                {
                    int start = math.max(firstList[i].StartIndex, secondList[j].StartIndex);
                    int end = math.min(firstList[i].StartIndex + firstList[i].Length - 1, secondList[j].StartIndex + secondList[j].Length - 1);
                    int count = end - start + 1;

                    if (count > 0 && firstList[i].Length > 0 && secondList[j].Length > 0)
                    {
                        output.Add(new IndexRange(start, count));
                    }
                }

                if (firstList[i].StartIndex + firstList[i].Length < secondList[j].StartIndex + secondList[j].Length)
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstList"></param>
        /// <param name="secondList"></param>
        /// <returns></returns>
        public static List<IndexRange> IntersectingRanges(List<IndexRange> ranges1, List<int> list)
        {
            ranges1.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));
            list.Sort();

            List<IndexRange> intersection = new(ranges1.ToList());

            for (int i = 0; i < list.Count; i++)
            {
                int element = list[i];

                for (int k = 0; k < intersection.Count; k++)
                {
                    var inter = intersection[k];
                    //check to see if it's already in the intersection list, if so break out
                    if (inter.Contains(element))
                        goto next_element;

                    //is it directly before a range?
                    if (inter.StartIndex - 1 == element)
                    {
                        intersection[k] = new IndexRange(inter.StartIndex - 1, inter.Length + 1);
                        goto next_element;
                    }
                    //how about directly after?
                    if (inter.EndIndex + 1 == element)
                    {
                        intersection[k] = new IndexRange(inter.StartIndex, inter.Length + 1);
                        goto next_element;
                    }
                }

                //looks like this was totally unique, time to add a new intersection
                intersection.Add(new IndexRange(element, 1));

            next_element:;
            }

            return intersection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ranges1"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<IndexRange> IntersectingRangesBinary(List<IndexRange> ranges1, List<int> list)
        {
            ranges1.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));
            list.Sort();

            List<IndexRange> intersection = new(ranges1.ToList());

            for (int i = 0; i < list.Count; i++)
            {
                int element = list[i];

                // Find the index where the current element belongs in the sorted intersection list
                int index = intersection.BinarySearch(new IndexRange(element, 1), IndexRangeComparer.Instance);

                if (index >= 0)
                {
                    // The element is already in an existing range, so skip it
                    continue;
                }

                // Get the insertion point of the element in the intersection list
                index = ~index;

                // Try merging the new range with the adjacent ranges, if possible
                bool merged = false;
                if (index > 0)
                {
                    var prev = intersection[index - 1];
                    if (prev.TryMerge(new IndexRange(element, 1), out var mergedRange))
                    {
                        intersection[index - 1] = mergedRange;
                        merged = true;
                    }
                }
                if (index < intersection.Count)
                {
                    var next = intersection[index];
                    if (next.TryMerge(new IndexRange(element, 1), out var mergedRange))
                    {
                        intersection[index] = mergedRange;
                        merged = true;
                    }
                }

                // If the new range couldn't be merged, add it as a new range
                if (!merged)
                {
                    intersection.Insert(index, new IndexRange(element, 1));
                }
            }

            return intersection;
        }

        /// <summary>
        /// Performs a logical difference operation where the second list of ranges have all of their indices removed from any
        /// overlapping ranges in the first list.
        /// </summary>
        /// <param name="primaryList"></param>
        /// <param name="removedList"></param>
        /// <returns></returns>
        public static List<IndexRange> DifferenceRanges(List<IndexRange> primaryList, List<IndexRange> removedList)
        {
            List<IndexRange> result = new();

            //flatten out the removal list
            var numbersToRemove = removedList.SelectMany(r => r);
            foreach (IndexRange range in primaryList)
            {

                if (range.Length > 0)
                {
                    var remainingNumbers = range.Except(numbersToRemove).ToList(); //can we get rid of the ToList() somehow?

                    //reconstruct the contiguous ranges from what remains
                    foreach (IndexRange remainingRange in Condense(remainingNumbers))
                        result.Add(remainingRange);
                }
            }

            return result;
        }

        /// <summary>
        /// Performs a logical difference operation where the second list of ranges have all of their indices removed from any
        /// overlapping ranges in the first list.
        /// </summary>
        /// <param name="primaryList"></param>
        /// <param name="removedList"></param>
        /// <returns></returns>
        public static void DifferenceRanges(List<IndexRange> primaryList, List<IndexRange> removedList, ref List<IndexRange> output)
        {
            output.Clear();

            //flatten out the removal list
            var numbersToRemove = removedList.SelectMany(r => r);
            foreach (IndexRange range in primaryList)
            {

                if (range.Length > 0)
                {
                    var remainingNumbers = range.Except(numbersToRemove).ToList(); //can we get rid of the ToList() somehow?

                    //reconstruct the contiguous ranges from what remains
                    foreach (IndexRange remainingRange in CondenseEnumerable(remainingNumbers))
                        output.Add(remainingRange);
                }
            }
        }

        /// <summary>
        /// Condenses a list of indices into as few IndexRanges as possible by combining
        /// contiguous indicies into a single range.
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static IEnumerable<IndexRange> CondenseEnumerable(List<int> numbers)
        {
            if (numbers.Count == 0)
            {
                yield break;
            }

            //numbers.Sort();

            int startIndex = numbers[0];
            int length = 1;

            for (int i = 1; i < numbers.Count; i++)
            {
                if (numbers[i] == numbers[i - 1] + 1)
                {
                    length++;
                }
                else
                {
                    yield return new IndexRange(startIndex, length);
                    startIndex = numbers[i];
                    length = 1;
                }
            }

            yield return new IndexRange(startIndex, length);
        }

        /// <summary>
        /// Condenses a list of indices into as few IndexRanges as possible by combining
        /// contiguous indicies into a single range.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<IndexRange> Condense(List<int> list)
        {
            List<IndexRange> intersection = new();

            for (int i = 0; i < list.Count; i++)
            {
                int element = list[i];

                for (int k = 0; k < intersection.Count; k++)
                {
                    var inter = intersection[k];
                    //check to see if it's already in the intersection list, if so break out
                    if (inter.Contains(element))
                        goto next_element;

                    //is it directly before a range?
                    if (inter.StartIndex - 1 == element)
                    {
                        intersection[k] = new IndexRange(inter.StartIndex - 1, inter.Length + 1);
                        goto next_element;
                    }
                    //how about directly after?
                    if (inter.EndIndex + 1 == element)
                    {
                        intersection[k] = new IndexRange(inter.StartIndex, inter.Length + 1);
                        goto next_element;
                    }
                }

                //looks like this was totally unique, time to add a new intersection
                intersection.Add(new IndexRange(element, 1));

            next_element:;
            }

            return intersection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="mergedRange"></param>
        /// <returns></returns>
        public bool TryMerge(IndexRange other, out IndexRange mergedRange)
        {
            mergedRange = this;

            if (other.StartIndex > EndIndex + 1 || other.EndIndex < StartIndex - 1)
            {
                // Ranges don't overlap, cannot merge
                return false;
            }

            mergedRange = new IndexRange(
                Math.Min(StartIndex, other.StartIndex),
                Math.Max(EndIndex, other.EndIndex) - Math.Min(StartIndex, other.StartIndex) + 1
            );

            return true;
        }

        /// <summary>
        /// Comparer helper class for sorting IndexRange objects by StartIndex
        /// </summary>
        private class IndexRangeComparer : IComparer<IndexRange>
        {
            public static readonly IndexRangeComparer Instance = new();

            public int Compare(IndexRange x, IndexRange y)
            {
                return x.StartIndex.CompareTo(y.StartIndex);
            }
        }

        #endregion

    }
}
