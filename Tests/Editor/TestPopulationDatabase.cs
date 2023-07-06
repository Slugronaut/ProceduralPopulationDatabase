using NUnit.Framework;

namespace ProceduralPopulationDatabase.Editor.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class TestPopulationDatabase
    {

        #region Constants
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

        static readonly int Seed = 69_420;
        static readonly int PopSize = 10_000;
        static readonly float[] GenderSlices    = new float[] { 0.5f, 0.5f };
        static readonly float[] RaceSlices      = new float[] { 0.35f, 0.40f, 0.25f };
        static readonly float[] ClassSlices     = new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f };
        #endregion


        #region Helper Methods
        PopulationTree GenerateTree()
        {
            var tree = new PopulationTree(PopSize);
            tree.Slice((int)Depths.Gender - 1, GenderSlices);
            tree.Slice((int)Depths.Race - 1, RaceSlices);
            tree.Slice((int)Depths.Class - 1, ClassSlices);
            return tree;
        }

        (Database, PopulationTree) GenerateDb()
        {
            var tree = GenerateTree();
            var db = new Database(Seed, tree);
            return (db, tree);
        }
        #endregion


        /// <summary>
        /// Asserts creation of the database.
        /// </summary>
        [Test]
        public void DatabaseCreation()
        {
            var (db, tree) = GenerateDb();
            var orcPallys = tree.Query
                .Query((int)Depths.Race, (int)Races.Orc)
                .Query((int)Depths.Class, (int)Classes.Paladin);

            Assert.IsNotNull(db);
            Assert.IsNotNull(tree);
            Assert.IsNotNull(orcPallys);
        }

        /// <summary>
        /// Asserts expected default values for states.
        /// </summary>
        [Test]
        public void ReadDefaultUsedStateAsUnused()
        {
            var (db, tree) = GenerateDb();
            var orcPallys = tree.Query
                .Query((int)Depths.Race, (int)Races.Orc)
                .Query((int)Depths.Class, (int)Classes.Paladin);

            foreach (var range in orcPallys.Ranges)
            {
                foreach(var uid in range)
                    Assert.IsFalse(db.IsIdInUse(uid));
            }
        }

        /// <summary>
        /// Asserts that writing to a range of states can be read back.
        /// </summary>
        [Test]
        public void ReadWriteUsedState()
        {
            var (db, tree) = GenerateDb();
            var orcPallys = tree.Query
                .Query((int)Depths.Race, (int)Races.Orc)
                .Query((int)Depths.Class, (int)Classes.Paladin);

            foreach (var range in orcPallys.Ranges)
            {
                foreach (var uid in range)
                {
                    db.SetIdUse(uid, true);
                    //check as we go
                    Assert.IsTrue(db.IsIdInUse(uid));
                }
            }

            //let's also loop back through again and check
            foreach (var range in orcPallys.Ranges)
            {
                foreach (var uid in range)
                    Assert.IsTrue(db.IsIdInUse(uid));
            }
        }

        /// <summary>
        /// Asserts that writing to one range of states doesn't affect any others.
        /// </summary>
        [Test]
        public void WritingToOneSampleDoesntAffectAnotherSample()
        {
            var (db, tree) = GenerateDb();
            var orcPallys = tree.Query
                .Query((int)Depths.Race, (int)Races.Orc)
                .Query((int)Depths.Class, (int)Classes.Paladin);

            var allButOrcPallys = tree.Query
                .Exclude((int)Depths.Race, (int)Races.Orc)
                .Exclude((int)Depths.Class, (int)Classes.Paladin);

            foreach (var range in orcPallys.Ranges)
            {
                foreach (var uid in range)
                {
                    db.SetIdUse(uid, true);
                    Assert.IsTrue(db.IsIdInUse(uid));
                }
            }

            int inc = 0;
            //Assert.AreEqual()
            foreach (var range in allButOrcPallys.Ranges)
            {
                foreach (var uid in range)
                {
                    Assert.IsFalse(db.IsIdInUse(uid), $"Failed on uid: {uid} on inc: {inc}");
                    inc++;
                }
            }

        }


    }
}
