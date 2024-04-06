using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AnimalControls.Patch
{
    //new born animals inherit food restriction from parents
    [HarmonyPatch(typeof(Hediff_Pregnant), "DoBirthSpawn")]
    static class Hediff_Pregnant_DoBirthSpawn_AnimalControlsPatch
    {
        static void SetValues(Pawn mother, Pawn father, Pawn pawn)
        {
            if (pawn.playerSettings == null || (mother == null || mother.playerSettings == null) && (father == null || father.playerSettings == null)) return;
            if (mother == null || mother.playerSettings == null)
                pawn.playerSettings.AreaRestrictionInPawnCurrentMap = father.playerSettings.AreaRestrictionInPawnCurrentMap;

            if (pawn.foodRestriction != null)
                if (mother.foodRestriction != null)
                    pawn.foodRestriction.CurrentFoodPolicy = mother.foodRestriction.CurrentFoodPolicy;
                else if (father.foodRestriction != null)
                    pawn.foodRestriction.CurrentFoodPolicy = father.foodRestriction.CurrentFoodPolicy;
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            MethodBase Lset_ar = AccessTools.Method(typeof(Pawn_PlayerSettings), "set_AreaRestrictionInPawnCurrentMap");
            MethodBase Lset_values = AccessTools.Method(typeof(Hediff_Pregnant_DoBirthSpawn_AnimalControlsPatch), nameof(SetValues));
            bool b0 = false;
            foreach (var i in instrs)
            {
                yield return i;
                if (i.opcode == OpCodes.Callvirt && i.operand == (object)Lset_ar)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, Lset_values);
                    b0 = true;
                }
            }
            if (!b0) Log.Warning("[Animal Controls] set_AreaRestriction patch 0 didn't work");
        }
    }
}
