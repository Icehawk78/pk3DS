using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using pk3DS.Core;
using pk3DS.Core.Structures;
using pk3DS.Core.Structures.PersonalInfo;

namespace pk3DS
{
    public partial class GameSummary7 : Form
    {
        public GameSummary7()
        {
            InitializeComponent();
            Setup();
        }
        #region Global Variables

        private readonly List<PersonalInfo> pkms = Main.Config.Personal.Table.ToList();
        private readonly List<Move> moves = Main.Config.Moves.ToList();
        private readonly List<int> types = Main.Config.Personal.Table.SelectMany(pkm => pkm.Types).ToHashSet().ToList();
        private readonly string[] typeNames = Main.Config.getText(TextName.Types);
        
        private readonly List<int> tms = TMEditor7.getTMHMList().Select(tm => Convert.ToInt32(tm)).ToList();
        private readonly int[][] tutors = TutorEditor7.getTutorList();
       
        private Dictionary<int, double> moveTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> pokemonMatchingTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> pokemonNonMatchingTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> matchingTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> nonMatchingTypeCounts = new Dictionary<int, double>();
        
        private Dictionary<int, double> tutorMoveTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> tutorPokemonMatchingTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> tutorPokemonNonMatchingTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> tutorMatchingTypeCounts = new Dictionary<int, double>();
        private Dictionary<int, double> tutorNonMatchingTypeCounts = new Dictionary<int, double>();
        
        #endregion
        
        private void Setup()
        {
            foreach (int type in types)
            {
                moveTypeCounts[type] = 0;
                pokemonMatchingTypeCounts[type] = 0;
                pokemonNonMatchingTypeCounts[type] = 0;
                matchingTypeCounts[type] = 0;
                nonMatchingTypeCounts[type] = 0;

                tutorMoveTypeCounts[type] = 0;
                tutorPokemonMatchingTypeCounts[type] = 0;
                tutorPokemonNonMatchingTypeCounts[type] = 0;
                tutorMatchingTypeCounts[type] = 0;
                tutorNonMatchingTypeCounts[type] = 0;
            }

            foreach (PersonalInfo pkm in Main.Config.Personal.Table)
            {
                for (int tmIndex = 0; tmIndex < tms.Count; tmIndex++)
                {
                    Move m = moves[tms[tmIndex]];
                    int t = m.Type;
                    moveTypeCounts[t] = moveTypeCounts[t] + 1;
                    int pkmHasTm = pkm.TMHM[tmIndex] ? 1 : 0;
                    bool isSTAB = pkm.Types.ToList().Contains(t);
                    if (isSTAB)
                    {
                        pokemonMatchingTypeCounts[t] = pokemonMatchingTypeCounts[t] + 1;
                        matchingTypeCounts[t] = matchingTypeCounts[t] + pkmHasTm;
                    }
                    else
                    {
                        pokemonNonMatchingTypeCounts[t] = pokemonNonMatchingTypeCounts[t] + 1;
                        nonMatchingTypeCounts[t] = nonMatchingTypeCounts[t] + pkmHasTm;
                    }
                }

                for (int tutorLocationIndex = 0; tutorLocationIndex < tutors.Length; tutorLocationIndex++)
                {
                    int[] tutorMoves = tutors[tutorLocationIndex];
                    
                    for (int tutorIndex = 0; tutorIndex < tutorMoves.Length; tutorIndex++)
                    {
                        Move m = moves[tutorMoves[tutorIndex]];
                        int t = m.Type;
                        tutorMoveTypeCounts[t] = tutorMoveTypeCounts[t] + 1;
                        int pkmHasTutor = pkm.SpecialTutors[tutorLocationIndex][tutorIndex] ? 1 : 0;
                        bool isSTAB = pkm.Types.ToList().Contains(t);
                        if (isSTAB)
                        {
                            tutorPokemonMatchingTypeCounts[t] = tutorPokemonMatchingTypeCounts[t] + 1;
                            tutorMatchingTypeCounts[t] = tutorMatchingTypeCounts[t] + pkmHasTutor;
                        }
                        else
                        {
                            tutorPokemonNonMatchingTypeCounts[t] = tutorPokemonNonMatchingTypeCounts[t] + 1;
                            tutorNonMatchingTypeCounts[t] = tutorNonMatchingTypeCounts[t] + pkmHasTutor;
                        }
                    }
                }
            }

            RTB_Summary.Clear();
            foreach (int type in types)
            {
                RTB_Summary.AppendText(typeNames[type] 
                                       + " TMs: " 
                                       + matchingTypeCounts[type] 
                                       + " (" + Math.Floor(100 * matchingTypeCounts[type] / pokemonMatchingTypeCounts[type]) + "%) STAB, " 
                                       + nonMatchingTypeCounts[type] 
                                       + " (" + Math.Floor(100 * nonMatchingTypeCounts[type] / pokemonNonMatchingTypeCounts[type]) + "%) Non-STAB\n");
                RTB_Summary.AppendText(typeNames[type] 
                                       + " Tutors: " 
                                       + tutorMatchingTypeCounts[type] 
                                       + " (" + Math.Floor(100 * tutorMatchingTypeCounts[type] / tutorPokemonMatchingTypeCounts[type]) + "%) STAB, " 
                                       + tutorNonMatchingTypeCounts[type] 
                                       + " (" + Math.Floor(100 * tutorNonMatchingTypeCounts[type] / tutorPokemonNonMatchingTypeCounts[type]) + "%) Non-STAB\n");
            }

            if (Main.Config.USUM)
            {
                // Do Stuff
            }
        }
    }
}