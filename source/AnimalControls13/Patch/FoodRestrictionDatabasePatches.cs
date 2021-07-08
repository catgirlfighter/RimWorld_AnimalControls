using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalControls.Patch
{
    /*
    //automatically allow eating plants for new restrictions
    [HarmonyPatch(typeof(FoodRestrictionDatabase), "MakeNewFoodRestriction")]
    static class FoodRestrictionDatabase_MakeNewFoodRestriction_AnimalControlsPatch
    {
        static void Postfix(ref FoodRestriction __result)
        {
            __result.filter.SetAllow(AnimalControlsDefOf.Plants, true, null, null);
        }
    }
    */
}
