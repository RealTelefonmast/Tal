using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TALMOD

[HarmonyPatch(typeof(PawnCapacityWorker_Manipulation))]
[HarmonyPatch("CalculateCapacityLevel")]
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