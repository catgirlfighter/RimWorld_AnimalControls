using System;
using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace AnimalControls.Patch
{
    //change filter requirement for training and taming food from "rawtasty" to "nutrition <= 0.1" during interaction
    [HarmonyPatch]
    static class JobDriver_InteractAnimal_StartFeedAnimal_Toil_initAction_AnimalControlsPatch
    {

        internal static MethodBase TargetMethod()
        {
            Type dc19_0 = AccessTools.Inner(typeof(JobDriver_InteractAnimal), "<>c__DisplayClass19_0");
            MethodInfo b_0 = AccessTools.Method(dc19_0, "<StartFeedAnimal>b__0");
            return b_0;
        }

        //public static Thing BestFoodInInventory (Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false)
        //public static Thing BestFoodInInventory_NewTemp(Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false, bool allowVenerated = false)
        static Thing BestFoodInInventoryWNutrLimit(Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false, bool allowVenerated = false, float maxIndividualNutrition = 1000f)
        {
            if (holder.inventory == null) return null;
            if (eater == null) eater = holder;
            ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Thing thing = innerContainer[i];
                var nutrition = thing.GetStatValue(StatDefOf.Nutrition, true);
                if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow
                    && eater.WillEat_NewTemp(thing, holder, true, allowVenerated)
                    && thing.def.ingestible.preferability >= minFoodPref
                    && thing.def.ingestible.preferability <= maxFoodPref
                    && (allowDrug || !thing.def.IsDrug)
                    && nutrition * thing.stackCount >= minStackNutrition
                    && nutrition <= maxIndividualNutrition)
                    return thing;
            }
            return null;
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            //MethodInfo LBestFoodInInventory = AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.BestFoodInInventory));
            MethodInfo LBestFoodInInventory = AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.BestFoodInInventory_NewTemp));
            MethodInfo LBestFoodInInventoryWNutrLimit = AccessTools.Method(typeof(JobDriver_InteractAnimal_StartFeedAnimal_Toil_initAction_AnimalControlsPatch), nameof(JobDriver_InteractAnimal_StartFeedAnimal_Toil_initAction_AnimalControlsPatch.BestFoodInInventoryWNutrLimit));
            MethodInfo LForceWait = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.ForceWait));
            MethodInfo LPayAttention = AccessTools.Method(typeof(JobDriver_PayAttention), nameof(JobDriver_PayAttention.ForcePayAttention));

            bool b0 = false;
            bool b1 = false;
            bool b2 = false;

            foreach (var i in instrs)
            {
                //change RawTasty to MealLavish in FoodUtility.BestFoodInInventory
                if (i.opcode == OpCodes.Ldc_I4_5)
                {
                    i.opcode = OpCodes.Ldc_I4_S;
                    i.operand = 9;
                    b0 = true;
                }

                if (i.opcode == OpCodes.Call && i.operand == (object)LBestFoodInInventory)
                {
                    i.operand = LBestFoodInInventoryWNutrLimit;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, AnimalControls.TrainAnimalNutritionLimit);
                    b1 = true;
                }

                if (i.opcode == OpCodes.Call && i.operand == (object)LForceWait)
                {
                    i.operand = LPayAttention;
                    b2 = true;
                }

                yield return i;
            }
            if (!b0) Log.Warning("JobDriver_InteractAnimal_StartFeedAnimal_Patch0 didn't work");
            if (!b1) Log.Warning("JobDriver_InteractAnimal_StartFeedAnimal_Patch1 didn't work");
            if (!b2) Log.Warning("JobDriver_InteractAnimal_StartFeedAnimal_Patch2 didn't work");
        }
    }

    [HarmonyPatch]
    static class JobDriver_InteractAnimal_TalkToAnimal_Toil_initAction_AnimalControlsPatch
    {
        static Type dc19_0;

        internal static MethodBase TargetMethod()
        {
            dc19_0 = AccessTools.Inner(typeof(JobDriver_InteractAnimal), "<>c__DisplayClass18_0");
            MethodInfo b_0 = AccessTools.Method(dc19_0, "<TalkToAnimal>b__0");
            return b_0;
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            MethodInfo LPayAttention = AccessTools.Method(typeof(JobDriver_PayAttention), nameof(JobDriver_PayAttention.ForcePayAttention));
            FieldInfo Ltoil = AccessTools.Field(dc19_0, "toil");
            MethodInfo Lget_CurJob = AccessTools.Method(typeof(Pawn), "get_CurJob");
            FieldInfo LtameeInd = AccessTools.Field(dc19_0, "tameeInd");
            MethodInfo LGetActor = AccessTools.Method(typeof(Toil), nameof(Toil.GetActor));
            MethodInfo LGetTarget = AccessTools.Method(typeof(Job), nameof(Job.GetTarget));
            MethodInfo Lop_Explicit = AccessTools.FirstMethod(typeof(LocalTargetInfo), m => m.Name == "op_Explicit" && m.ReturnType == typeof(Thing));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, Ltoil);
            yield return new CodeInstruction(OpCodes.Callvirt, LGetActor);
            yield return new CodeInstruction(OpCodes.Stloc_0);
            yield return new CodeInstruction(OpCodes.Ldloc_0);
            yield return new CodeInstruction(OpCodes.Callvirt, Lget_CurJob);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, LtameeInd);
            yield return new CodeInstruction(OpCodes.Callvirt, LGetTarget);
            yield return new CodeInstruction(OpCodes.Call, Lop_Explicit);
            yield return new CodeInstruction(OpCodes.Castclass, typeof(Pawn));
            yield return new CodeInstruction(OpCodes.Ldc_I4, 270);
            yield return new CodeInstruction(OpCodes.Ldloc_0);
            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            yield return new CodeInstruction(OpCodes.Call, LPayAttention);

            foreach (var i in instrs)
            {
                yield return i;
            }
        }
    }
}
