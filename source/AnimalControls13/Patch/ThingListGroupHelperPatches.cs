using System;
using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AnimalControls.Patch
{
    //to make it possible to feed other pawns with hay (and alike)
    [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
    public static class ThingListGroupHelper_Includes_CommonSensePatch
    {
        static bool Prefix(ref bool __result, ThingRequestGroup group, ThingDef def)
        {
            if (!Settings.allow_feeding_with_plants || group != ThingRequestGroup.FoodSourceNotPlantOrTree)
                return true;

            Type TPlant = typeof(Plant);
            __result = (def.IsNutritionGivingIngestible && def.thingClass != TPlant && !def.thingClass.IsSubclassOf(TPlant)) || def.thingClass == typeof(Building_NutrientPasteDispenser);
            return false;
        }
    }
}
