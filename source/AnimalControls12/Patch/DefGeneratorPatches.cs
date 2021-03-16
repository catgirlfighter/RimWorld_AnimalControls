using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalControls.Patch
{
    //moving all plant defs into their categories
    [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
    static class DefGeneratorr_GenerateImpliedDefs_PreResolve_AnimalControlsPatch
    {
        static bool Prefix()
        {
            Type TPlant = typeof(Plant);
            IEnumerable<ThingDef> list = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => (((x.thingClass == TPlant || x.thingClass.IsSubclassOf(TPlant)) && x.IsIngestible)));
            foreach (var i in list)
            {
                if (i.ingestible.foodType == FoodTypeFlags.Tree)
                {
                    if (i.thingCategories == null) i.thingCategories = new List<ThingCategoryDef>();
                    DirectXmlCrossRefLoader.RegisterListWantsCrossRef(i.thingCategories, AnimalControlsDefOf.Trees.defName, i, null);
                }
                else if (i.plant != null && i.plant.Sowable)
                {
                    if (i.thingCategories == null) i.thingCategories = new List<ThingCategoryDef>();
                    DirectXmlCrossRefLoader.RegisterListWantsCrossRef(i.thingCategories, AnimalControlsDefOf.Crops.defName, i, null);
                }
                else if (i.ingestible.foodType == FoodTypeFlags.Plant)
                {
                    if (i.thingCategories == null) i.thingCategories = new List<ThingCategoryDef>();
                    DirectXmlCrossRefLoader.RegisterListWantsCrossRef(i.thingCategories, AnimalControlsDefOf.Plants.defName, i, null);
                }
            }

            return true;
        }
    }
}
