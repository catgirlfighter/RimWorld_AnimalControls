using System;
using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AnimalControls.Patch
{
    //make "max nutrition" filter count
    [HarmonyPatch]
    static class FoodUtility_BestFoodSourceOnMap_foodValidator_AnimalControlsPatch
    {
        static bool belowNutrition(Thing thing)
        {
            float statValue = thing.GetStatValue(StatDefOf.Nutrition, true);
            return statValue <= AnimalControls.BestFoodSourceOnMap_maxNutrition;
        }

        internal static MethodBase TargetMethod()
        {
            Type dc12_0 = AccessTools.Inner(typeof(FoodUtility), "<>c__DisplayClass12_0");
            MethodInfo b_0 = AccessTools.Method(dc12_0, "<BestFoodSourceOnMap>b__0");
            return b_0;
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            bool b0 = false;
            CodeInstruction oldi = null;
            foreach (var i in instrs)
            {
                MethodInfo LWillEatThing = AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool) });
                MethodInfo LWillEatDef = AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool) });
                MethodInfo LbelowNutrition = AccessTools.Method(typeof(FoodUtility_BestFoodSourceOnMap_foodValidator_AnimalControlsPatch), nameof(FoodUtility_BestFoodSourceOnMap_foodValidator_AnimalControlsPatch.belowNutrition));

                if (oldi != null)
                {
                    yield return oldi;

                    if (i.opcode == OpCodes.Brfalse && oldi.opcode == OpCodes.Call && (oldi.operand == (object)LWillEatDef || oldi.operand == (object)LWillEatThing))
                    {
                        Label l = (Label)i.operand;
                        yield return new CodeInstruction(OpCodes.Brfalse, l);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, LbelowNutrition);
                        b0 = true;
                    }
                }
                oldi = i;
            }
            yield return oldi;
            if (!b0) Log.Warning("[Animal Controls] BestFoodSourceOnMap patch 0 didn't work");
        }
    }

    //make "plants" category count
    /*
    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool) })]
    static class FoodUtility_WillEat_Thing_AnimalControlsPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            FieldInfo Ldef = AccessTools.Field(typeof(Thing), nameof(Thing.def));
            //FieldInfo Lfoods = AccessTools.Field(typeof(ThingCategoryDefOf), nameof(ThingCategoryDefOf.Foods));
            FieldInfo Lplants = AccessTools.Field(typeof(AnimalControlsDefOf), nameof(AnimalControlsDefOf.Plants));
            MethodInfo LisWithinCategory = AccessTools.Method(typeof(ThingDef), nameof(ThingDef.IsWithinCategory));
            CodeInstruction oldi = null;
            bool b0 = false;
            foreach (var i in instrs)
            {
                if (oldi != null)
                {
                    yield return oldi;

                    if (i.opcode == OpCodes.Brtrue_S && oldi.opcode == OpCodes.Callvirt && oldi.operand == (object)LisWithinCategory)
                    {
                        Label l = (Label)i.operand;
                        yield return new CodeInstruction(OpCodes.Brtrue_S, l);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldfld, Ldef);
                        yield return new CodeInstruction(OpCodes.Ldsfld, Lplants);
                        yield return new CodeInstruction(OpCodes.Callvirt, LisWithinCategory);
                        b0 = true;
                    }
                }
                oldi = i;
            }
            yield return oldi;
            if (!b0) Log.Warning("[Animal Controls] WillEat patch 0 didn't work");
        }
    }
    */

    //add priority to eat growing plants
    //add priority to eat hay (and alike)
    //add priority to eat food with unconditional bad mood effects
    [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
    static class FoodUtility_FoodOptimality
    {
        static void Postfix(ref float __result, Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
        {
            float modifier = 0f;

            if (eater.needs != null && eater.needs.mood != null)
                return;
            //
            if (Settings.allow_feeding_with_plants)
            {
                if (eater.RaceProps.Eats(FoodTypeFlags.Plant))
                {
                    if (foodDef.ingestible.foodType == FoodTypeFlags.Plant)
                        modifier += 5f;
                    if (foodSource is Plant)
                        modifier += 25f;
                }
            }
            //
            FoodPreferability pref = foodDef.ingestible.preferability;
            switch (pref)
            {
                case FoodPreferability.DesperateOnlyForHumanlikes:
                    modifier += 5f;
                    break;
                case FoodPreferability.RawBad:
                    break;
                case FoodPreferability.RawTasty:
                    modifier -= 5f;
                    break;
                case FoodPreferability.MealAwful:
                    modifier += 5f;
                    break;
                case FoodPreferability.MealFine:
                    modifier -= 10f;
                    break;
                case FoodPreferability.MealLavish:
                    modifier -= 15f;
                    break;
                default:
                    modifier -= 5f;
                    break;
            }
            //
            if (foodDef.ingestible?.specialThoughtAsIngredient?.stages?.Count > 0)
            {
                modifier -= foodDef.ingestible.specialThoughtAsIngredient.stages[0].baseMoodEffect;
            }
            //
            if (foodSource != null)
            {
                CompIngredients ings = foodSource.TryGetComp<CompIngredients>();

                if (ings != null)
                {
                    foreach(var ing in ings.ingredients)
                    if (ing.ingestible?.specialThoughtAsIngredient?.stages?.Count > 0)
                    {
                        modifier -= ing.ingestible.specialThoughtAsIngredient.stages[0].baseMoodEffect * 3;
                    }
                }
            }
            //
            __result += modifier;
        }
    }
}