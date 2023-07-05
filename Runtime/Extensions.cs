using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralPopulationDatabase
{
    /// <summary>
    /// Adds a function to system.Random that allows us to pick IndexRanges at random with weightings for larger spans.
    /// </summary>
    public static class RandomExtensions
    {
        public static int NextWeighted(this System.Random random, List<IndexRange> ranges)
        {
            int totalSize = ranges.Sum(range => range.Length);
            int randomNumber = random.Next((int)totalSize);
            int currentSize = 0;

            for(int i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];
                currentSize += range.Length;
                if (randomNumber < currentSize)
                {
                    int rangeRandomNumber = random.Next(range.Length);
                    return range.StartIndex + rangeRandomNumber;
                }
            }

            throw new InvalidOperationException("List of index ranges is empty.");
        }
    }
}