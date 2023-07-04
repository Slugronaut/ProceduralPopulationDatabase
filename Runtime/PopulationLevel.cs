using System.Collections.Generic;
using System.Linq;

namespace ProceduralPopulationDatabase
{
    /// <summary>
    /// A recursive data structure that represents a depth level of a <see cref="PopulationTree"/>.
    /// </summary>
    public class PopulationLevel
    {
        readonly public IndexRange Range;
        public PopulationLevel[] Children;
        public int Count => Range.Length;
        public IEnumerable<IndexRange> ChildRanges => Children.Select(child => child.Range);


        public PopulationLevel(int start, int length)
        {
            Range = new IndexRange(start, length);
        }

        public PopulationLevel(IndexRange range)
        {
            Range = range;
        }
    }
}
