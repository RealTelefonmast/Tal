using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VilousTal
{
    [StaticConstructorOnStartup]
    public static class TalPatches
    {
        static TalPatches()
        {

        }

        [HarmonyPatch(typeof(PawnCapacityWorker_Manipulation), "CalculateCapacityLevel")]
        public static class CalculateCapacityLevel_Patch
        {
            public static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
            {
                //Get the affected pawn's PawnKindDef
                if (diffSet.pawn.kindDef == PawnKindDef.Named("TalSergal"))
                {
                    //Do Race-Specific patches
                    __result *= 1.5f;
                }
            }
        }
    }
}
