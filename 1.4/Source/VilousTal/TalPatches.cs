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

        [HarmonyPatch(typeof(Pawn), "BodySize", MethodType.Getter)]
        public static class PawnBodySizePatch
        {
            public static void Postfix(ref float __result, Pawn __instance)
            {
                //Get the affected pawn's PawnKindDef
                if (__instance?.kindDef == TalDefOf.SergalColonist)
                {
                    __result *= TalConfigDef.Def.sergProps.GetSizeUsed;
                }
            }
        }

        [HarmonyPatch(typeof(PawnCapacityUtility), "CalculateCapacityLevel")]
        public static class PawnCapacityUtilityCalculateCapacityLevelPatch
        {
            public static void Postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity)
            {
                //Get the affected pawn's PawnKindDef
                var pawn = diffSet.pawn;
                if (pawn?.kindDef == TalDefOf.SergalColonist)
                {
                    var capOffset = TalConfigDef.Def.sergProps.GetCapModFor(capacity);
                    if (capOffset != null)
                    {
                        __result += capOffset.offset;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(VerbProperties), "AdjustedCooldown", new []{typeof(Tool), typeof(Pawn), typeof(Thing)})]
        public static class VerbPropertiesAdjustedCooldownPatch
        {
            public static void Postfix(ref float __result, Tool tool, Pawn attacker, Thing equipment)
            {
                //Get the affected pawn's PawnKindDef
                if (attacker?.kindDef == TalDefOf.SergalColonist)
                {
                    __result *= TalConfigDef.Def.sergProps.meleeCDMultiplier;
                }
            }
        }
    }
}
