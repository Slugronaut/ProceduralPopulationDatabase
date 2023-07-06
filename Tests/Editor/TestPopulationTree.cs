using NUnit.Framework;
using System.Linq;

namespace ProceduralPopulationDatabase.Editor.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class TestPopulationTree
    {
        static readonly int DefaultPopSize = 10_000;
        static readonly float[] LevelOneSlices = new float[] {0.5f, 0.5f};


        [Test]
        public void TestIndexRangePercentages()
        {
            int start = 47;
            int len = 45; //92 - 47 = (45) -> 4.5, 11.25, 28.8, 0.45 -> Rounded(4, 11, 28, 0)
            float[] percentages = { 0.1f, 0.25f, 0.64f, 0.01f };

            var result = IndexRange.CalculatePercentageRanges(start, len, percentages);

            Assert.AreEqual(5, result[0].Length);
            Assert.AreEqual(11, result[1].Length);
            Assert.AreEqual(29, result[2].Length);
            Assert.AreEqual(0, result[3].Length);

            var tally = result.Sum(v => v.Length);
            Assert.AreEqual(len, tally);
        }

        [Test]
        public void PopSizeCorrectOnCreation()
        {
            PopulationTree tree = new PopulationTree(DefaultPopSize);
            
            Assert.AreEqual(DefaultPopSize, tree.PopulationSize);
        }

        [Test]
        public void DefaultPopSliceIsCorrect()
        {
            var tree = new PopulationTree(DefaultPopSize);
            var ranges = tree.GetPopulationIndexRanges(0);

            Assert.AreEqual(1, ranges.Length);
            Assert.AreEqual(DefaultPopSize, ranges[0].Length);
        }

        [Test]
        public void NegativeGetPopDepthThrows()
        {
            var tree = new PopulationTree(DefaultPopSize);
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => tree.GetPopulationIndexRanges(-1));
        }

        [Test]
        public void DepthZeroIsValidAfterSlicing()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(1, LevelOneSlices);

            var levelZeroRanges = tree.GetPopulationIndexRanges(0);
            var total = levelZeroRanges.Sum(range => range.Length);
            Assert.AreEqual(DefaultPopSize, levelZeroRanges[0].Length);
            Assert.AreEqual(DefaultPopSize, total);
        }

        [Test]
        public void DepthOneIsValidAfterSlicing()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(1, LevelOneSlices);


            var levelOneRanges = tree.GetPopulationIndexRanges(1).ToArray();
            Assert.AreEqual(2, levelOneRanges.Length);
            Assert.AreEqual(5000, levelOneRanges[0].Length);
            Assert.AreEqual(5000, levelOneRanges[1].Length);
        }

        [Test]
        public void DepthZeroValidAfter3Slices()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(1, new float[] { 0.5f, 0.5f });
            tree.Slice(2, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(3, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            var levelZeroRanges = tree.GetPopulationIndexRanges(0);
            var total = levelZeroRanges.Sum(range => range.Length);
            Assert.AreEqual(10_000, levelZeroRanges[0].Length);
            Assert.AreEqual(10_000, total);
        }

        [Test]
        public void DepthOneValidAfter3Slices()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(1, new float[] { 0.5f, 0.5f });
            tree.Slice(2, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(3, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            //level 1
            var levelOneRanges = tree.GetPopulationIndexRanges(1);
            Assert.AreEqual(2, levelOneRanges.Length); //1 set of 2
            Assert.AreEqual(5000, levelOneRanges[0].Length);
            Assert.AreEqual(5000, levelOneRanges[1].Length);

        }

        [Test]
        public void DepthTwoValidAfter3Slices()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(1, new float[] { 0.5f, 0.5f });
            tree.Slice(2, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(3, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            //level 2
            var levelTwoRanges = tree.GetPopulationIndexRanges(2).ToArray();
            Assert.AreEqual(6, levelTwoRanges.Length); //2 sets of 3
            Assert.AreEqual(10_000, levelTwoRanges.Sum(range => range.Length));

            Assert.AreEqual(0, levelTwoRanges[0].StartIndex);
            Assert.AreEqual(1749, levelTwoRanges[0].EndIndex);
            Assert.AreEqual(1750, levelTwoRanges[0].Length);

            Assert.AreEqual(1750, levelTwoRanges[1].StartIndex);
            Assert.AreEqual(3749, levelTwoRanges[1].EndIndex);
            Assert.AreEqual(2000, levelTwoRanges[1].Length);

            Assert.AreEqual(3750, levelTwoRanges[2].StartIndex);
            Assert.AreEqual(4999, levelTwoRanges[2].EndIndex);
            Assert.AreEqual(1250, levelTwoRanges[2].Length);

            Assert.AreEqual(5000, levelTwoRanges[3].StartIndex);
            Assert.AreEqual(6749, levelTwoRanges[3].EndIndex);
            Assert.AreEqual(1750, levelTwoRanges[3].Length);

            Assert.AreEqual(6750, levelTwoRanges[4].StartIndex);
            Assert.AreEqual(8749, levelTwoRanges[4].EndIndex);
            Assert.AreEqual(2000, levelTwoRanges[4].Length);

            Assert.AreEqual(8750, levelTwoRanges[5].StartIndex);
            Assert.AreEqual(9999, levelTwoRanges[5].EndIndex);
            Assert.AreEqual(1250, levelTwoRanges[5].Length);

        }

        [Test]
        public void DepthThreeValidAfter3Slices()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(1, new float[] { 0.5f, 0.5f });
            tree.Slice(2, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(3, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            //level 3
            var levelThreeRanges = tree.GetPopulationIndexRanges(3).ToArray();
            Assert.AreEqual(36, levelThreeRanges.Length); //2 sets of 3 sets of 6
            int indexOffset = 6;

            Assert.AreEqual(1750, levelThreeRanges[indexOffset + 0].StartIndex);
            Assert.AreEqual(1949, levelThreeRanges[indexOffset + 0].EndIndex);
            Assert.AreEqual(200, levelThreeRanges[indexOffset + 0].Length);

            Assert.AreEqual(1950, levelThreeRanges[indexOffset + 1].StartIndex);
            Assert.AreEqual(2149, levelThreeRanges[indexOffset + 1].EndIndex);
            Assert.AreEqual(200, levelThreeRanges[indexOffset + 1].Length);

            Assert.AreEqual(2150, levelThreeRanges[indexOffset + 2].StartIndex);
            Assert.AreEqual(2649, levelThreeRanges[indexOffset + 2].EndIndex);
            Assert.AreEqual(500, levelThreeRanges[indexOffset + 2].Length);

            Assert.AreEqual(2650, levelThreeRanges[indexOffset + 3].StartIndex);
            Assert.AreEqual(3249, levelThreeRanges[indexOffset + 3].EndIndex);
            Assert.AreEqual(600, levelThreeRanges[indexOffset + 3].Length);

            Assert.AreEqual(3250, levelThreeRanges[indexOffset + 4].StartIndex);
            Assert.AreEqual(3619, levelThreeRanges[indexOffset + 4].EndIndex);
            Assert.AreEqual(370, levelThreeRanges[indexOffset + 4].Length);

            Assert.AreEqual(3620, levelThreeRanges[indexOffset + 5].StartIndex);
            Assert.AreEqual(3749, levelThreeRanges[indexOffset + 5].EndIndex);
            Assert.AreEqual(130, levelThreeRanges[indexOffset + 5].Length); //this is 1 higher than might be expected due to the fact that the last element will use whatever is leftover in the range after flooring to ints

            //just to make sure even with rounding we get to where we need to be
            //our next range is scaled over 1250 so we need to acount for that
            Assert.AreEqual(3750, levelThreeRanges[indexOffset + 6].StartIndex);
            Assert.AreEqual(3874, levelThreeRanges[indexOffset + 6].EndIndex);
            Assert.AreEqual(125, levelThreeRanges[indexOffset + 6].Length);

            //and one more random sample for funsies, on the backhalf
            Assert.AreEqual(5000, levelThreeRanges[18].StartIndex);
            Assert.AreEqual(5174, levelThreeRanges[18].EndIndex);
            Assert.AreEqual(175, levelThreeRanges[18].Length);

            Assert.AreEqual(5175, levelThreeRanges[19].StartIndex);
            Assert.AreEqual(5349, levelThreeRanges[19].EndIndex);
            Assert.AreEqual(175, levelThreeRanges[19].Length);

            Assert.AreEqual(5350, levelThreeRanges[20].StartIndex);
            Assert.AreEqual(5787, levelThreeRanges[20].EndIndex);
            Assert.AreEqual(438, levelThreeRanges[20].Length);     //437.5 rounded up

            Assert.AreEqual(5788, levelThreeRanges[21].StartIndex);
            Assert.AreEqual(6312, levelThreeRanges[21].EndIndex);
            Assert.AreEqual(525, levelThreeRanges[21].Length);

            Assert.AreEqual(6313, levelThreeRanges[22].StartIndex);
            Assert.AreEqual(6636, levelThreeRanges[22].EndIndex);
            Assert.AreEqual(324, levelThreeRanges[22].Length);     //324.1 rounded down

            Assert.AreEqual(6637, levelThreeRanges[23].StartIndex);
            Assert.AreEqual(6749, levelThreeRanges[23].EndIndex);
            Assert.AreEqual(113, levelThreeRanges[23].Length);     //113.4 rounded down. no accum leftover so we don't add an extra to the end either

        }

        [Test]
        public void JaggedSlice()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(1, new float[] { 0.5f, 0.5f });
            tree.JaggedSlice(2, new float[][] {
                new float[] { 0.4f, 0.6f },
                new float[] { 0.6f, 0.4f },
            });
            Assert.AreEqual(10_000, tree.PopulationSize);


            var l1 = tree.GetPopulationIndexRanges(1).ToArray();
            Assert.AreEqual(2, l1.Length);
            Assert.AreEqual(5_000, l1[0].Length);
            Assert.AreEqual(5_000, l1[1].Length);

            
            var l2 = tree.GetPopulationIndexRanges(2).ToArray();
            Assert.AreEqual(4, l2.Length);
            Assert.AreEqual(2_000, l2[0].Length);
            Assert.AreEqual(3_000, l2[1].Length);
            Assert.AreEqual(3_000, l2[2].Length);
            Assert.AreEqual(2_000, l2[3].Length);
            
        }

    }
}
