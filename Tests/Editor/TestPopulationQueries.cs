using NUnit.Framework;
using System.Globalization;

namespace ProceduralPopulationDatabase.Editor.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class TestPopulationQueries
    {
        #region Constants
        //raw numbers are too abstract, let's define some meaning for them
        enum Depths : int
        {
            Root,
            Gender,
            Race,
            Class,
        }

        enum Genders
        {
            Male,
            Female,
        }

        enum Races
        {
            Human,
            Orc,
            Elf,
        }

        enum Classes
        {
            Rogue,
            Mages,
            Paladin,
            Fighter,
            Assassin,
            Necromancer,
        }


        static readonly int DefaultPopSize = 10_000;
        static readonly float[] GenderSlices    = new float[] { 0.5f,   0.5f                                        };
        static readonly float[] RaceSlices      = new float[] { 0.35f,  0.40f,  0.25f                               };
        static readonly float[] ClassSlices     = new float[] { 0.1f,   0.1f,   0.25f,  0.30f,  0.1852f,    0.0648f };
        #endregion


        #region Helper Methods
        PopulationTree GenerateTree()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice((int)Depths.Gender - 1, GenderSlices);
            tree.Slice((int)Depths.Race - 1, RaceSlices);
            tree.Slice((int)Depths.Class - 1, ClassSlices);
            return tree;
        }
        #endregion


        [Test]
        public void BasicQueryIsNotNull()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(0, new float[] { 0.5f, 0.5f });
            tree.Slice(1, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(2, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            var sample = tree.Query;
            Assert.IsNotNull(sample);
        }

        [Test]
        public void RootQueryContainsFullRange()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(0, new float[] { 0.5f, 0.5f });
            tree.Slice(1, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(2, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            var sample = tree.Query;
            Assert.AreEqual(1, sample.Ranges.Count);
            Assert.AreEqual(0, sample.Ranges[0].StartIndex);
            Assert.AreEqual(DefaultPopSize-1, sample.Ranges[0].EndIndex);
            Assert.AreEqual(DefaultPopSize, sample.Ranges[0].Length);
        }

        [Test]
        public void RequeryOfRootContainsCorrectSlice()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(0, new float[] { 0.5f, 0.5f });
            tree.Slice(1, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(2, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            //requesting a query of the root level. we'll ask for the 0th element because there is only 1 at this level
            var sampleLeft = tree.Query.Query(0, 0);
            Assert.AreEqual(1, sampleLeft.Ranges.Count);
            Assert.AreEqual(0, sampleLeft.Ranges[0].StartIndex);
            Assert.AreEqual(DefaultPopSize - 1, sampleLeft.Ranges[0].EndIndex);
            Assert.AreEqual(DefaultPopSize, sampleLeft.Ranges[0].Length);
        }

        [Test]
        public void QueryDepth1ContainsCorrectRanges()
        {
            var tree = new PopulationTree(DefaultPopSize);
            tree.Slice(0, new float[] { 0.5f, 0.5f });
            tree.Slice(1, new float[] { 0.35f, 0.40f, 0.25f });
            tree.Slice(2, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });

            var sampleLeft = tree.Query.Query(1, 0);
            Assert.AreEqual(1,                          sampleLeft.Ranges.Count);
            Assert.AreEqual(0,                          sampleLeft.Ranges[0].StartIndex);
            Assert.AreEqual((DefaultPopSize / 2) - 1,   sampleLeft.Ranges[0].EndIndex);
            Assert.AreEqual(DefaultPopSize / 2,         sampleLeft.Ranges[0].Length);

            var sampleRight = tree.Query.Query(1, 1);
            Assert.AreEqual(1, sampleRight.Ranges.Count);
            Assert.AreEqual(DefaultPopSize / 2, sampleRight.Ranges[0].StartIndex);
            Assert.AreEqual(DefaultPopSize - 1, sampleRight.Ranges[0].EndIndex);
            Assert.AreEqual(DefaultPopSize / 2, sampleRight.Ranges[0].Length);
        }

        /// <summary>
        /// Proves that querying the race gives us a list of ranges for both genders.
        /// </summary>
        [Test]
        public void QueryForOrcPopulationIsAccurate()
        {
            var tree = GenerateTree();
            var orcPop = tree.Query.Query((int)Depths.Race, (int)Races.Orc);

            Assert.AreEqual(2, orcPop.Ranges.Count);

            Assert.AreEqual(1750, orcPop.Ranges[(int)Genders.Male].StartIndex);
            Assert.AreEqual(3749, orcPop.Ranges[(int)Genders.Male].EndIndex);
            Assert.AreEqual(2000, orcPop.Ranges[(int)Genders.Male].Length);

            Assert.AreEqual(6750, orcPop.Ranges[(int)Genders.Female].StartIndex);
            Assert.AreEqual(8749, orcPop.Ranges[(int)Genders.Female].EndIndex);
            Assert.AreEqual(2000, orcPop.Ranges[(int)Genders.Female].Length);
        }

        /// <summary>
        /// Proves that a query for a specific class gives a range for both genders multiplied by the number of races.
        /// </summary>
        [Test]
        public void QueryForPaladinPopulationIsAccurate()
        {
            var tree = GenerateTree();
            var paladinPop = tree.Query
                .Query((int)Depths.Class, (int)Classes.Paladin);

            int femaleHumanPaladin = 3;
            Assert.AreEqual(6, paladinPop.Ranges.Count);
            Assert.AreEqual(5350, paladinPop.Ranges[femaleHumanPaladin].StartIndex);
            Assert.AreEqual(5787, paladinPop.Ranges[femaleHumanPaladin].EndIndex);
            Assert.AreEqual(438, paladinPop.Ranges[femaleHumanPaladin].Length);
        }

        /// <summary>
        /// Proves that we can do a compound query for both race and class
        /// and get a list of ranges containing both genders.
        /// </summary>
        [Test]
        public void QueryForOrcPaldinPopulationIsAccurate()
        {
            var tree = GenerateTree();
            var orcPaladinPop = tree.Query
                .Query((int)Depths.Race, (int)Races.Orc)
                .Query((int)Depths.Class, (int)Classes.Paladin);

            
            Assert.AreEqual(2, orcPaladinPop.Ranges.Count);

            int male = 0;
            Assert.AreEqual(2150, orcPaladinPop.Ranges[male].StartIndex);
            Assert.AreEqual(2649, orcPaladinPop.Ranges[male].EndIndex);
            Assert.AreEqual(500, orcPaladinPop.Ranges[male].Length);

            int female = 1;
            Assert.AreEqual(7150, orcPaladinPop.Ranges[female].StartIndex);
            Assert.AreEqual(7649, orcPaladinPop.Ranges[female].EndIndex);
            Assert.AreEqual(500, orcPaladinPop.Ranges[female].Length);
        }

        /// <summary>
        /// This should have the exact same results as <see cref="QueryForOrcPaldinPopulationIsAccurate"/>
        /// which proves that queries are communicable.
        /// </summary>
        [Test]
        public void QueryForPaladinOrcPopulationIsAccurate()
        {
            var tree = GenerateTree();
            var paladinOrcPop = tree.Query
                .Query((int)Depths.Class, (int)Classes.Paladin)
                .Query((int)Depths.Race, (int)Races.Orc);

            Assert.AreEqual(2, paladinOrcPop.Ranges.Count);

            int male = 0;
            Assert.AreEqual(2150, paladinOrcPop.Ranges[male].StartIndex);
            Assert.AreEqual(2649, paladinOrcPop.Ranges[male].EndIndex);
            Assert.AreEqual(500, paladinOrcPop.Ranges[male].Length);

            int female = 1;
            Assert.AreEqual(7150, paladinOrcPop.Ranges[female].StartIndex);
            Assert.AreEqual(7649, paladinOrcPop.Ranges[female].EndIndex);
            Assert.AreEqual(500, paladinOrcPop.Ranges[female].Length);
        }

        /// <summary>
        /// Ensures that a query of all levels provides exact range and nothing more.
        /// </summary>
        [Test]
        public void QueryForFemaleHumanFighterPopulationIsAccurate()
        {
            var tree = GenerateTree();
            var paladinOrcPop = tree.Query
                .Query((int)Depths.Gender,  (int)Genders.Female)
                .Query((int)Depths.Race,    (int)Races.Human)
                .Query((int)Depths.Class,   (int)Classes.Fighter);

            Assert.AreEqual(1, paladinOrcPop.Ranges.Count);

            Assert.AreEqual(5788, paladinOrcPop.Ranges[0].StartIndex);
            Assert.AreEqual(6312, paladinOrcPop.Ranges[0].EndIndex);
            Assert.AreEqual(525,  paladinOrcPop.Ranges[0].Length);
        }

        /// <summary>
        /// Proves that if we make two queries that are mutually exclusive the returned range is empty.
        /// </summary>
        [Test]
        public void QueryForClassAfterExcludingItGivesEmptyPopulation()
        {
            var tree = GenerateTree();
            var paladinOrcPop = tree.Query
                .Query((int)Depths.Class, (int)Classes.Paladin)
                .Query((int)Depths.Class, (int)Classes.Fighter);

            Assert.AreEqual(0, paladinOrcPop.Ranges.Count);
        }

        /// <summary>
        /// Assert that we can exlude all of the first split in the tree.
        /// </summary>
        [Test]
        public void ExcludeAllFemales()
        {
            var tree = GenerateTree();
            var males = tree.Query
                .Exclude((int)Depths.Gender, (int)Genders.Female);

            Assert.AreEqual(1, males.Ranges.Count);
            Assert.AreEqual(5000, males.Count);
            Assert.AreEqual(0, males.Ranges[0].StartIndex);
            Assert.AreEqual(4999, males.Ranges[0].EndIndex);
            Assert.AreEqual(5000, males.Ranges[0].Length);
        }

        [Test]
        public void ExcludeAllPaladins()
        {
            var tree = GenerateTree();
            var nonPallys = tree.Query
                .Exclude((int)Depths.Class, (int)Classes.Paladin);

            //we've effectively taken a set of 36 ranges, removing 6, and then condensing the rest into as few as possible
            Assert.AreEqual(7, nonPallys.Ranges.Count);
        }
    }


}
