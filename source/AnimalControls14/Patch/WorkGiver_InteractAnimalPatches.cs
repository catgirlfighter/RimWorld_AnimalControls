using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AnimalControls.Patch
{
    //normally handlers take only the food that's below meal in quality
    //changing "not a meal" restriction to "<=0.1f nutition"
    [HarmonyPatch(typeof(WorkGiver_InteractAnimal), "HasFoodToInteractAnimal")]
    static class WorkGiver_InteractAnimal_HasFoodToInteractAnimal_AnimalControlsPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            CodeInstruction oldi = null;
            FieldInfo Lpref = AccessTools.Field(typeof(IngestibleProperties), "preferability");
            MethodInfo Lnutrition = AccessTools.Method(typeof(IngestibleProperties), "get_CachedNutrition");
            bool b0 = false;
            foreach (var i in instrs)
            {
                if (oldi != null)
                {
                    if (oldi.opcode == OpCodes.Ldfld && oldi.operand == (object)Lpref && i.opcode == OpCodes.Ldc_I4_5)
                    {
                        oldi.opcode = OpCodes.Callvirt;
                        oldi.operand = Lnutrition;
                        i.opcode = OpCodes.Ldc_R4;
                        i.operand = AnimalControls.TrainAnimalNutritionLimit;
                        b0 = true;
                    }
                    yield return oldi;
                }
                oldi = i;
            }
            yield return oldi;
            if (!b0) Log.Warning("[Animal Controls] HasFoodToInteractAnimal patch 0 didn't work");
        }
    }

    //change filter requirement for training and taming food from "rawtasty" to "nutrition <= 0.1" when looking for it
    //we've made a new filter for it, based off of bestFoodSourceOnMap_minNutrition_NewTemp
    [HarmonyPatch(typeof(WorkGiver_InteractAnimal), "TakeFoodForAnimalInteractJob")]
    static class WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob_AnimalControlsPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            MethodInfo LbestFoodSourceOnMap = AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap));
            MethodInfo LbestFoodSourceOnMap_NewTemp = AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap_NewTemp));
            MethodInfo LsetMaxNutrition = AccessTools.Method(typeof(AnimalControls), nameof(AnimalControls.SetBestFoodSourceOnMap_maxNutrition));
            FieldInfo LTrainAnimalNutritionLimit = AccessTools.Field(typeof(AnimalControls), nameof(AnimalControls.TrainAnimalNutritionLimit));
            //
            CodeInstruction oldi = null;
            bool b1 = false;
            bool b2 = false;
            //yield return new CodeInstruction(OpCodes.Ldc_R4, AnimalControls.TrainAnimalNutritionLimit);
            yield return new CodeInstruction(OpCodes.Ldsfld, LTrainAnimalNutritionLimit);
            oldi = new CodeInstruction(OpCodes.Call, LsetMaxNutrition);
            //Log.Message($"m1 = {LbestFoodSourceOnMap}");
            //Log.Message($"m2 = {LbestFoodSourceOnMap_NewTemp}");
            foreach (var i in instrs)
            {
                //Log.Message($"{i.opcode},{i.operand}");
                if (oldi != null)
                {
                    yield return oldi;
                    if (i.opcode == OpCodes.Stloc_1 
                    && oldi.opcode == OpCodes.Call 
                    && (oldi.operand == (object)LbestFoodSourceOnMap || oldi.operand == (object)LbestFoodSourceOnMap_NewTemp))
                    {
                        yield return i;

                        yield return new CodeInstruction(OpCodes.Ldc_R4, float.MaxValue);
                        oldi = new CodeInstruction(OpCodes.Call, LsetMaxNutrition);
                        b1 = true;
                        continue;
                    }
                }
                //
                if (i.opcode == OpCodes.Ldc_I4_5)
                {
                    i.opcode = OpCodes.Ldc_I4_S;
                    i.operand = (int)FoodPreferability.MealLavish;
                    b2 = true;
                }
                //
                oldi = i;
            }
            //
            yield return oldi;
            if (!b1) Log.Warning("[Animal Controls] WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob patch 1 didn't work");
            if (!b2) Log.Warning("[Animal Controls] WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob patch 2 didn't work");
        }
    }
}