using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using pk3DS.Core.Structures;
using pk3DS.Core.Structures.PersonalInfo;

namespace pk3DS.Core.Randomizers
{
    public class PersonalRandomizer : IRandomizer
    {
        private readonly Random rnd = Util.rand;

        private const decimal LearnTMPercent = 35; // Average Learnable TMs is 35.260.
        private const decimal LearnSTABTMPercent = 80; // 
        private const decimal LearnTypeTutorPercent = 2; //136 special tutor moves learnable by species in Untouched ORAS.
        private const decimal LearnMoveTutorPercent = 30; //10001 tutor moves learnable by 826 species in Untouched ORAS.
        private const int tmcount = 100;
        private const int eggGroupCount = 16;

        private readonly GameConfig Game;
        private readonly PersonalInfo[] Table;

        // Randomization Settings
        public int TypeCount;
        public bool ModifyCatchRate = true;
        public bool ModifyLearnsetTM = true;
        public bool ModifyLearnsetHM = true;
        public bool ModifyLearnsetTypeTutors = true;
        public bool ModifyLearnsetMoveTutors = true;
        public bool ModifyHeldItems = true;

        public bool ModifyAbilities = true;
        public bool AllowWonderGuard = true;

        public bool ModifyStats = true;
        public bool ShuffleStats = true;
        public decimal StatDeviation = 25;
        public bool[] StatsToRandomize = { true, true, true, true, true, true };

        public bool ModifyTypes = true;
        public decimal SameTypeChance = 50;
        public bool ModifyEggGroup = true;
        public decimal SameEggGroupChance = 50;

        public decimal TMInheritanceDeviation = 10;
        public decimal AbilityInheritanceDeviation = 10;
        public decimal TypeInheritanceDeviation = 10;

        private const bool TMInheritance = true;
        private const bool AbilityInheritance = false;
        private const bool TypeInheritance = false;
        private const bool ModifyLearnsetSmartly = false;

        public ushort[] MoveIDsTMs { private get; set; }
        
        public Move[] Moves => Game.Moves;
        public EvolutionSet[] Evos => Game.Evolutions;

        public PersonalRandomizer(PersonalInfo[] table, GameConfig game)
        {
            Game = game;
            Table = table;
            if (File.Exists("bannedabilites.txt"))
            {
                var data = File.ReadAllLines("bannedabilities.txt");
                var list = new List<int>(BannedAbilities);
                list.AddRange(data.Select(z => Convert.ToInt32(z)));
                BannedAbilities = list;
            }
        }

        public void Execute()
        {
            for (var i = 1; i < Table.Length; i++)
                Randomize(Table[i], i);
            
            List<Action<PersonalInfo, PersonalInfo>> propagations = new List<Action<PersonalInfo, PersonalInfo>>();
            List<Action<PersonalInfo>> propagationDeviations = new List<Action<PersonalInfo>>();

            if (TMInheritance)
            {
                propagations.Add((a, b) =>
                {
                    for (var i = 0; i < a.TMHM.Length; i++)
                    {
                        b.TMHM[i] = a.TMHM[i];
                    }
                });
                if (TMInheritanceDeviation > 0)
                {
                    propagationDeviations.Add(a =>
                    {
                        for (int i = 0; i < a.TMHM.Length; i++)
                        {
                            // Randomly flip whether TMs are learned TMInheritanceDeviation% of the time
                            a.TMHM[i] ^= (rnd.Next(100) < TMInheritanceDeviation);
                        }
                    });
                }
            }


            if (AbilityInheritance)
            {
                propagations.Add((a, b) => a.Abilities = b.Abilities);
                if (AbilityInheritanceDeviation > 0)
                {
                    // TODO: Ability Deviation
                }
            }

            if (TypeInheritance)
            {
                propagations.Add((a, b) => a.Types = b.Types);
                if (TypeInheritanceDeviation > 0)
                {
                    // TODO: Type Deviation
                }
            }
            PropagateAttribute(Table, Evos, propagations, propagationDeviations);
        }

        private void PropagateAttribute(PersonalInfo[] table, 
            EvolutionSet[] evos, 
            List<Action<PersonalInfo, PersonalInfo>> propagations,
            List<Action<PersonalInfo>> propagationDeviations)
        {
            var specCount = Game.MaxSpeciesID;
            var handledIndexes = new HashSet<int>();

            for (var species = 1; species <= specCount; species++)
            {
                var current = table[species];
                if (current.EvoStage == 1)
                {
                    Cascade(species);
                }
            }

            void Cascade(int baseId)
            {
                if (!handledIndexes.Contains(baseId))
                {
                    handledIndexes.Add(baseId);
                    var basePi = table[baseId];
                    foreach (var evoId in evos[baseId].PossibleEvolutions.Where(e => e.Species != 0)
                        .Select(e => e.Species))
                    {
                        var evoPi = table[evoId];
                        DoCascade(ref basePi, ref evoPi);
                        if (evoPi.FormeCount > 1)
                        {
                            CascadeFormes(baseId, evoId);
                        }

                        Cascade(evoId);
                    }
                }
            }

            void CascadeFormes(int baseId, int currentId)
            {
                var basePi = table[baseId];
                var currentPi = table[currentId];
                for (var formeIndex = 0; formeIndex < currentPi.FormeCount; formeIndex++)
                {
                    int formeId = currentPi.FormeIndex(currentId, formeIndex);
                    if (formeId != currentId && formeIndex != 0 && !handledIndexes.Contains(formeId))
                    {
                        handledIndexes.Add(formeId);
                        DoCascade(ref basePi, ref table[formeId]);
                    }
                }
            }
            
            void DoCascade(ref PersonalInfo basePi, ref PersonalInfo cascadeTo)
            {
                foreach (var p in propagations)
                {
                    p(basePi, cascadeTo);
                }

                foreach (var pd in propagationDeviations)
                {
                    pd(cascadeTo);
                }
            }
        }

        public void Randomize(PersonalInfo z, int index)
        {
            // Fiddle with Learnsets
            if (ModifyLearnsetTM || ModifyLearnsetHM)
            {
                if (!ModifyLearnsetSmartly)
                    RandomizeTMHMSimple(z);
                else
                    RandomizeTMHMAdvanced(z);
            }
            if (ModifyLearnsetTypeTutors)
                RandomizeTypeTutors(z, index);
            if (ModifyLearnsetMoveTutors)
                RandomizeSpecialTutors(z);
            if (ModifyStats)
                RandomizeStats(z);
            if (ShuffleStats)
                RandomShuffledStats(z);
            if (ModifyAbilities)
                RandomizeAbilities(z);
            if (ModifyEggGroup)
                RandomizeEggGroups(z);
            if (ModifyHeldItems)
                RandomizeHeldItems(z);
            if (ModifyTypes)
                RandomizeTypes(z);
            if (ModifyCatchRate)
                z.CatchRate = rnd.Next(3, 251); // Random Catch Rate between 3 and 250.
        }

        private void RandomizeTMHMAdvanced(PersonalInfo z)
        {
            var tms = z.TMHM;
            var types = z.Types;

            bool CanLearn(Move m)
            {
                var type = m.Type;
                bool typeMatch = types.Any(t => t == type);
                // todo: how do I learn move?
                return rnd.Next(100) < (typeMatch ? LearnSTABTMPercent : LearnTMPercent);
            }

            if (ModifyLearnsetTM)
            {
                for (int j = 0; j < tmcount; j++)
                {
                    var moveID = MoveIDsTMs[j];
                    var move = Moves[moveID];
                    tms[j] = CanLearn(move);
                }
            }
            if (ModifyLearnsetHM)
            {
                for (int j = tmcount; j < tms.Length; j++)
                {
                    var moveID = MoveIDsTMs[j];
                    var move = Moves[moveID];
                    tms[j] = CanLearn(move);
                }
            }

            z.TMHM = tms;
        }

        private void RandomizeTMHMSimple(PersonalInfo z)
        {
            var tms = z.TMHM;

            if (ModifyLearnsetTM)
            for (int j = 0; j < tmcount; j++)
                tms[j] = rnd.Next(0, 100) < LearnTMPercent;

            if (ModifyLearnsetHM)
            for (int j = tmcount; j < tms.Length; j++)
                tms[j] = rnd.Next(0, 100) < LearnTMPercent;

            z.TMHM = tms;
        }

        private void RandomizeTypeTutors(PersonalInfo z, int index)
        {
            var t = z.TypeTutors;
            for (int i = 0; i < t.Length; i++)
                t[i] = rnd.Next(0, 100) < LearnTypeTutorPercent;

            // Make sure Rayquaza can learn Dragon Ascent.
            if (!Game.XY && (index == 384 || index == 814))
                t[7] = true;

            z.TypeTutors = t;
        }

        private void RandomizeSpecialTutors(PersonalInfo z)
        {
            var tutors = z.SpecialTutors;
            foreach (bool[] tutor in tutors)
                for (int i = 0; i < tutor.Length; i++)
                    tutor[i] = rnd.Next(0, 100) < LearnMoveTutorPercent;
            z.SpecialTutors = tutors;
        }

        private void RandomizeAbilities(PersonalInfo z)
        {
            var abils = z.Abilities;
            for (int i = 0; i < abils.Length; i++)
                abils[i] = GetRandomAbility();
            z.Abilities = abils;
        }

        private void RandomizeEggGroups(PersonalInfo z)
        {
            var egg = z.EggGroups;
            egg[0] = GetRandomEggGroup();
            egg[1] = rnd.Next(0, 100) < SameEggGroupChance ? egg[0] : GetRandomEggGroup();
            z.EggGroups = egg;
        }

        private void RandomizeHeldItems(PersonalInfo z)
        {
            var item = z.Items;
            for (int j = 0; j < item.Length; j++)
                item[j] = GetRandomHeldItem();
            z.Items = item;
        }

        private void RandomizeTypes(PersonalInfo z)
        {
            var t = z.Types;
            t[0] = GetRandomType();
            t[1] = rnd.Next(0, 100) < SameTypeChance ? t[0] : GetRandomType();
            z.Types = t;
        }

        private void RandomizeStats(PersonalInfo z)
        {
            // Fiddle with Base Stats, don't muck with Shedinja.
            var stats = z.Stats;
            if (stats[0] == 1)
                return;
            for (int i = 0; i < stats.Length; i++)
            {
                if (!StatsToRandomize[i])
                    continue;
                var l = Math.Min(255, (int) (stats[i] * (1 - (StatDeviation / 100))));
                var h = Math.Min(255, (int) (stats[i] * (1 + (StatDeviation / 100))));
                stats[i] = Math.Max(5, rnd.Next(l, h));
            }
            z.Stats = stats;
        }

        private void RandomShuffledStats(PersonalInfo z)
        {
            // Fiddle with Base Stats, don't muck with Shedinja.
            var stats = z.Stats;
            if (stats[0] == 1)
                return;
            for (int i = 0; i < stats.Length; i++)
                Util.Shuffle(stats);
            z.Stats = stats;
        }

        private int GetRandomType() => rnd.Next(0, TypeCount);
        private int GetRandomEggGroup() => rnd.Next(1, eggGroupCount);
        private int GetRandomHeldItem() => Game.Info.HeldItems[rnd.Next(1, Game.Info.HeldItems.Length)];
        private readonly IList<int> BannedAbilities = new int[0];

        private int GetRandomAbility()
        {
            const int WonderGuard = 25;
            int newabil;
            do newabil = rnd.Next(1, Game.Info.MaxAbilityID + 1);
            while ((newabil == WonderGuard && !AllowWonderGuard) || BannedAbilities.Contains(newabil));
            return newabil;
        }
    }
}
