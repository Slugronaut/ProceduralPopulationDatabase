using System.Collections.Generic;
using NUnit.Framework;

namespace ProceduralPopulationDatabase.Editor.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class TestRangeIntersection
    {

        static List<IndexRange> CreateRangeList(params IndexRange[] ranges)
        {
            var list = new List<IndexRange>();
            foreach (var range in ranges)
                list.Add(range);

            return list;
        }

        [Test]
        public void TrivialRangeIntersectionFound1()
        {
            var ranges1 = CreateRangeList(new IndexRange(0, 10));
            var ranges2 = CreateRangeList(new IndexRange(0, 5));

            var result = IndexRange.IntersectingRanges(ranges1, ranges2);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].StartIndex);
            Assert.AreEqual(5, result[0].Length);
        }

        [Test]
        public void TrivialRangeIntersectionFound2()
        {
            var ranges1 = CreateRangeList(new IndexRange(1, 10));
            var ranges2 = CreateRangeList(new IndexRange(0, 5));

            var result = IndexRange.IntersectingRanges(ranges1, ranges2);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].StartIndex);
            Assert.AreEqual(4, result[0].Length);
        }

        [Test]
        public void TrivialRangeIntersectionFound3()
        {
            var ranges1 = CreateRangeList(new IndexRange(0, 10));
            var ranges2 = CreateRangeList(new IndexRange(1, 5));

            var result = IndexRange.IntersectingRanges(ranges1, ranges2);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].StartIndex);
            Assert.AreEqual(5, result[0].Length);
        }

        [Test]
        public void NonIntersectingRangesNotFound()
        {
            var ranges1 = CreateRangeList(new IndexRange(5, 10));
            var ranges2 = CreateRangeList(new IndexRange(0, 5));

            var result = IndexRange.IntersectingRanges(ranges1, ranges2);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ComplexIntersectingRangesFound1()
        {
            var genders = CreateRangeList(new IndexRange(0, 25), new IndexRange(50, 25));
            var homes = CreateRangeList(new IndexRange(50, 50));

            var result = IndexRange.IntersectingRanges(genders, homes);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(50, result[0].StartIndex);
            Assert.AreEqual(25, result[0].Length);
        }

        [Test]
        public void ComplexIntersectingRangesFound2()
        {
            var genders = CreateRangeList(
                new IndexRange(0, 3),
                new IndexRange(12, 5),
                new IndexRange(50, 3),
                new IndexRange(75, 3)
                );
            var homes = CreateRangeList(
                new IndexRange(0, 50),
                new IndexRange(75, 1)
                );

            var result = IndexRange.IntersectingRanges(genders, homes);
            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(0, result[0].StartIndex);
            Assert.AreEqual(3, result[0].Length);

            Assert.AreEqual(12, result[1].StartIndex);
            Assert.AreEqual(5, result[1].Length);

            Assert.AreEqual(75, result[2].StartIndex);
            Assert.AreEqual(1, result[2].Length);
        }

        [Test]
        public void ComplexIntersectingRangesFound2Reversed()
        {
            var genders = CreateRangeList(
                new IndexRange(0, 3),
                new IndexRange(12, 5),
                new IndexRange(50, 3),
                new IndexRange(75, 3)
                );
            var homes = CreateRangeList(
                new IndexRange(0, 50),
                new IndexRange(75, 1)
                );

            var result = IndexRange.IntersectingRanges(homes, genders);
            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(0, result[0].StartIndex);
            Assert.AreEqual(3, result[0].Length);

            Assert.AreEqual(12, result[1].StartIndex);
            Assert.AreEqual(5, result[1].Length);

            Assert.AreEqual(75, result[2].StartIndex);
            Assert.AreEqual(1, result[2].Length);
        }

        [Test]
        public void CompoundComplexRangesFound()
        {
            var genders = CreateRangeList(
                new IndexRange(0, 3),
                new IndexRange(12, 5),
                new IndexRange(50, 3),
                new IndexRange(75, 3)
                );
            var homes = CreateRangeList(
                new IndexRange(0, 50),
                new IndexRange(75, 1)
                );

            //will have ranges of 0-3, 12-5, and 75-1
            var result = IndexRange.IntersectingRanges(homes, genders);

            //let's create yet another 'query' and then intersect. this will be for the back half: 50 to 100
            var races = CreateRangeList(
                new IndexRange(50, 50)
                );

            result = IndexRange.IntersectingRanges(result, races);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(75, result[0].StartIndex);
            Assert.AreEqual(1, result[0].Length);
        }

        [Test]
        public void ZeroContentsOverlap()
        {
            var totalPop = CreateRangeList(
                new IndexRange(0, 100)
                );

            var subPops = CreateRangeList(
                new IndexRange(0, 0),
                new IndexRange(50, 0)
                );

            var result = IndexRange.IntersectingRanges(totalPop, subPops);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void TrivialRangeVsListNoOverlap()
        {
            var ranges1 = CreateRangeList(new IndexRange(1, 10));
            var list = new List<int>(new int[] { 12, 14});

            //will have ranges of 0-3, 12-5, and 75-1
            var result = IndexRange.IntersectingRangesBinary(ranges1, list);

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(1, result[0].StartIndex);
            Assert.AreEqual(10, result[0].EndIndex);

            Assert.AreEqual(12, result[1].StartIndex);
            Assert.AreEqual(12, result[1].EndIndex);

            Assert.AreEqual(14, result[2].StartIndex);
            Assert.AreEqual(14, result[2].EndIndex);
        }

        [Test]
        public void TrivialRangeVsListOverlapListOverlap()
        {
            var ranges1 = CreateRangeList(new IndexRange(1, 10));
            var list = new List<int>(new int[] { 12, 13 });

            //will have ranges of 0-3, 12-5, and 75-1
            var result = IndexRange.IntersectingRangesBinary(ranges1, list);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].StartIndex);
            Assert.AreEqual(10, result[0].EndIndex);

            Assert.AreEqual(12, result[1].StartIndex);
            Assert.AreEqual(13, result[1].EndIndex);
        }

        [Test]
        public void TrivialRangeVsListOverlapRangeBeginning()
        {
            var ranges1 = CreateRangeList(new IndexRange(1, 10));
            var list = new List<int>(new int[] { 0, 13 });

            //will have ranges of 0-3, 12-5, and 75-1
            var result = IndexRange.IntersectingRangesBinary(ranges1, list);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(0, result[0].StartIndex);
            Assert.AreEqual(10, result[0].EndIndex);

            Assert.AreEqual(13, result[1].StartIndex);
            Assert.AreEqual(13, result[1].EndIndex);
        }

        [Test]
        public void TrivialRangeVsListOverlapRangeEnd()
        {
            var ranges1 = CreateRangeList(new IndexRange(1, 10));
            var list = new List<int>(new int[] { 11, 14 });

            //will have ranges of 0-3, 12-5, and 75-1
            var result = IndexRange.IntersectingRangesBinary(ranges1, list);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].StartIndex);
            Assert.AreEqual(11, result[0].EndIndex);

            Assert.AreEqual(14, result[1].StartIndex);
            Assert.AreEqual(14, result[1].EndIndex);
        }

        [Test]
        public void TrivialRangeVsListExtendsEndTwice()
        {
            var ranges1 = CreateRangeList(new IndexRange(1, 10));
            var list = new List<int>(new int[] { 11, 12 });

            //will have ranges of 0-3, 12-5, and 75-1
            var result = IndexRange.IntersectingRangesBinary(ranges1, list);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(1, result[0].StartIndex);
            Assert.AreEqual(12, result[0].EndIndex);
        }

    }
}
