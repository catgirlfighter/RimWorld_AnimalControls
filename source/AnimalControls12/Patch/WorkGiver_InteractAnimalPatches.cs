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

    /* also must change JobDriver_IntercalAnimal.StartFeedAnimal and WorkGiver_InteractAnimal.TakeFoodForAnimalInteractJob. Too troublesome for now
     * non-centralized checks are annoying T_T */
    [HarmonyPatch(typeof(WorkGiver_InteractAnimal), "HasFoodToInteractAnimal")]
    static class WorkGiver_InteractAnimal_HasFoodToInteractAnimal_AnimalControlsPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            CodeInstruction oldi = null;
            FieldInfo Lpref = AccessTools.Field(typeof(IngestibleProperties), "preferability");
            MethodInfo Lnutrition = AccessTools.Method(typeof(IngestibleProperties), "get_CachedNutrition");
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
                    }
                    yield return oldi;
                }
                oldi = i;
            }
            yield return oldi;
        }
    }

    //change filter requirement for training and taming food from "rawtasty" to "nutrition <= 0.1" when looking for it
    //we've made a new filter for it, based off of bestFoodSourceOnMap_minNutrition_NewTemp
    [HarmonyPatch(typeof(WorkGiver_InteractAnimal), "TakeFoodForAnimalInteractJob")]
    static class WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob_AnimalControlsPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            FieldInfo LminTotalNutrition = AccessTools.Field(typeof(FoodUtility), "bestFoodSourceOnMap_minNutrition_NewTemp");
            MethodInfo LsetMaxNutrition = AccessTools.Method(typeof(AnimalControls), nameof(AnimalControls.SetBestFoodSourceOnMap_maxNutrition));
            //
            CodeInstruction oldi = null;
            foreach (var i in instrs)
            {
                if (oldi != null)
                {
                    yield return oldi;

                    if (i.opcode == OpCodes.Initobj && oldi.opcode == OpCodes.Ldsflda && oldi.operand == (object)LminTotalNutrition)
                    {
                        yield return i;
                        yield return new CodeInstruction(OpCodes.Ldc_R4, float.MaxValue);
                        oldi = new CodeInstruction(OpCodes.Call, LsetMaxNutrition);
                        continue;
                    }
                }
                //
                if (i.opcode == OpCodes.Stfld && i.operand == (object)LminTotalNutrition)
                {
                    yield return i;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, AnimalControls.TrainAnimalNutritionLimit);
                    oldi = new CodeInstruction(OpCodes.Call, LsetMaxNutrition);
                    continue;
                }
                //
                if (i.opcode == OpCodes.Ldc_I4_5)
                {
                    i.opcode = OpCodes.Ldc_I4_S;
                    i.operand = 9;
                }
                //
                oldi = i;
            }
            //
            yield return oldi;
        }
    }
}