using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace ProceduralPopulationDatabase
{
    /// <summary>
    /// Stores queryable state information for each unique id within a population.
    /// States are stored in a native array with 64-bits per uid.
    /// </summary>
    public class Database
    {
        public readonly PopulationTree Tree;
        protected NativeArray<ulong> States;
        protected Random Randomizer;

        static readonly List<int> TempInUse = new(16);
        static readonly List<int> TempNotInUse = new(16); 
        public static readonly ulong InUseStateMask = 0x0000_0000_0000_0001;


        #region Public Methods
        public Database(int seed, PopulationTree totalPopulation)
        {
            Tree = totalPopulation;
            States = new(totalPopulation.PopulationSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            Randomizer = new Random(seed);
        }

        /// <summary>
        /// Can be used to dynamically change the seed for the internal randomization system.
        /// </summary>
        /// <param name="seed"></param>
        public void SetSeed(int seed)
        {
            Randomizer = new Random(seed);
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe void ResetPopulationStates()
        {
            UnsafeUtility.MemClear(
                    NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(States),
                    UnsafeUtility.SizeOf<ulong>() * Tree.PopulationSize);
        }

        /// <summary>
        /// Returns the use-state of the given id in the population.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool IsIdInUse(int uid) => (States[uid] & InUseStateMask) > 0;

        /// <summary>
        /// Sets the use-state of the given id in the population.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="inUse"></param>
        public void SetIdUse(int uid, bool inUse)
        {
            if (inUse) States[uid] |= InUseStateMask;
            else States[uid] &= ~InUseStateMask;
        }

        /// <summary>
        /// Returns a random id that has the preferred in-use state in the database.
        /// </summary>
        /// <param name="pop"></param>
        /// <returns></returns>
        public int RandomId(PopulationSample pop, InUseStates preferredState)
        {
            Assert.IsNotNull(pop);
            Assert.IsNotNull(pop.Ranges);
            if (pop.Count < 1)
                throw new EmptyPopulationException();

            //if we don't care about in-use states, then just pick any random index
            if (preferredState == InUseStates.Either)
            {
                int randomIndex = Randomizer.NextWeighted(pop.Ranges);
                return randomIndex;
            }

            TempInUse.Clear();
            TempNotInUse.Clear();
            var ranges = pop.Ranges;
            for(int rangeIndex = 0; rangeIndex < ranges.Count; rangeIndex++)
            {
                var range = ranges[rangeIndex];
                for (int i = range.StartIndex; i <= range.EndIndex; i++)
                {
                    if (IsIdInUse(i))
                        TempInUse.Add(i);
                    else TempNotInUse.Add(i);
                }
            }

            if (preferredState == InUseStates.InUse)
            {
                if (TempInUse.Count < 1)
                    throw new EmptyPopulationException();
                return TempInUse[Randomizer.Next(TempInUse.Count)];
            }
            if (preferredState == InUseStates.NotInUse)
            {
                if (TempNotInUse.Count < 1)
                    throw new EmptyPopulationException();
                return TempNotInUse[Randomizer.Next(TempNotInUse.Count)];
            }

            //this should never actually get called but the static analyizer is being stupid.
            //if we make it this far we're fucked anyway so....
            throw new EmptyPopulationException();
        }

        /// <summary>
        /// Returns a list of ids that have the preferred in-use state in the database.
        /// This version is non-allocating.
        /// </summary>
        /// <param name="preferredState"></param>
        /// <returns></returns>
        /// <exception cref="EmptyPopulationException"></exception>
        public void RandomIds(PopulationSample pop, InUseStates preferredState, ref List<int> ids, int count)
        {
            Assert.IsNotNull(pop);
            Assert.IsNotNull(pop.Ranges);
            var popCount = pop.Count;
            if (popCount < 1)
                throw new EmptyPopulationException();

            int left = popCount;
            Assert.IsTrue(count <= left, $"Requested more ids than are available. Requested: {count}  Available: {left}");
            int needed = count;
            ids.Clear();
            var ranges = pop.Ranges;
            for (int rangeIndex = 0; rangeIndex < ranges.Count; rangeIndex++)
            {
                var range = ranges[rangeIndex];
                for (int uid = range.StartIndex; uid <= range.EndIndex; uid++)
                {
                    bool inUse = IsIdInUse(uid);
                    if (preferredState == InUseStates.NotInUse && inUse) continue;
                    if (preferredState == InUseStates.InUse && !inUse) continue;

                    float chance = (float)needed / (float)left;
                    left--;
                    if (Randomizer.NextDouble() < chance)
                    {
                        needed--;
                        ids.Add(uid);
                    }
                    if (left < 1 || needed < 1)
                        break;
                }
            }

            if (ids.Count > 0) return;
            throw new EmptyPopulationException();
        }

        /// <summary>
        /// Returns the id for the next unused uid in the population.
        /// </summary>
        /// <exception cref="EmptyPopulationException"></exception>
        /// <returns></returns>
        public int NextUnused(PopulationSample pop)
        {
            Assert.IsNotNull(pop);
            Assert.IsNotNull(pop.Ranges);
            var popCount = pop.Count;
            if (popCount < 1)
                throw new EmptyPopulationException();

            var statesSpan = States;
            for (int uid = 0; uid < statesSpan.Length; uid++)
            {
                if (!IsIdInUse(uid))
                    return uid;
            }

            throw new EmptyPopulationException();
        }

        /// <summary>
        /// Returns the id for the next in-use uid in the population.
        /// </summary>
        /// <returns></returns>
        public int NextInUse(PopulationSample pop)
        {
            Assert.IsNotNull(pop);
            Assert.IsNotNull(pop.Ranges);
            var popCount = pop.Count;
            if (popCount < 1)
                throw new EmptyPopulationException();

            var statesSpan = States;
            for (int uid = 0; uid < statesSpan.Length; uid++)
            {
                if (IsIdInUse(uid))
                    return uid;
            }

            throw new EmptyPopulationException();
        }

        /// <summary>
        /// Fills a list with a range of ids that are un-used up to the maximum requested.
        /// The resulting list can be smaller than the request amount if the population
        /// of un-used ids is smaller.
        /// This method is non-allocating.
        /// </summary>
        /// <param name="maxIds"></param>
        /// <param name="ids"></param>
        public void UnusedIdsRange(PopulationSample pop, int maxIds, ref List<int> ids)
        {
            Assert.IsNotNull(pop);
            Assert.IsNotNull(pop.Ranges);
            var popCount = pop.Count;
            if (popCount < 1)
                throw new EmptyPopulationException();

            ids.Clear();
            var statesSpan = States;
            for (int uid = 0; uid < statesSpan.Length; uid++)
            {
                if (!IsIdInUse(uid))
                    ids.Add(uid);

                if (ids.Count >= maxIds) break;
            }
        }

        /// <summary>
        /// Fills a list with a range of ids that are used up to the maximum requested.
        /// The resulting list can be smaller than the request amount if the population
        /// of un-used ids is smaller.
        /// This method is non-allocating.
        /// </summary>
        /// <param name="maxIds"></param>
        /// <param name="ids"></param>
        public void UsedIdsRange(PopulationSample pop, int maxIds, ref List<int> ids)
        {
            Assert.IsNotNull(pop);
            Assert.IsNotNull(pop.Ranges);
            var popCount = pop.Count;
            if (popCount < 1)
                throw new EmptyPopulationException();

            ids.Clear();
            var statesSpan = States;
            for (int uid = 0; uid < statesSpan.Length; uid++)
            {
                if (!IsIdInUse(uid))
                    ids.Add(uid);

                if (ids.Count >= maxIds) break;
            }
        }
        #endregion
    }
}
