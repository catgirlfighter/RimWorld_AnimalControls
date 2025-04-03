
using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AnimalControls.Patch
{
    //normally this one is possible only for humanlikes, but we need it done for animals too
    [HarmonyPatch(typeof(Pawn_FoodRestrictionTracker), "Configurable", MethodType.Getter)]
    static class Pawn_FoodRestrictionTracker_Configurable_AnimalControlsPatch
    {
        public static bool Prefix(ref Pawn_FoodRestrictionTracker __instance, ref bool __result)
        {
            __result = (!__instance.pawn.Destroyed && (__instance.pawn.Faction == Faction.OfPlayer || __instance.pawn.HostFaction == Faction.OfPlayer));
            return false;
        }
    }

    //change restriction to "handler" if handler offers food to the animal
    [HarmonyPatch(typeof(Pawn_FoodRestrictionTracker), "GetCurrentRespectedRestriction")]
    static class Pawn_FoodRestrictionTracker_GetCurrentRespectedRestriction_AnimalControlsPatch
    {
        public static void Postfix(Pawn_FoodRestrictionTracker __instance, ref FoodPolicy __result, Pawn getter)
        {
            if (__result == null || __instance?.pawn == null || __instance.pawn == getter) return;

            if (__instance.pawn?.RaceProps?.Animal == true && getter != null && getter.Faction == Find.FactionManager.OfPlayer)
            {
                var comp = Current.Game?.GetComponent<AnimalControlsRestrictions>();
                if (comp?.HandlerRestriction == null) return;
                __result = comp.HandlerRestriction;
            }
        }
    }

    //automatically change food restriction to "default animals" for animals
    [HarmonyPatch(typeof(Pawn_FoodRestrictionTracker), "CurrentFoodPolicy", MethodType.Getter)]
    static class Pawn_FoodRestrictionTracker_CurrentFoodRestriction_Getter_AnimalControlsPatch
    {
        private static FoodPolicy GetDefaultFoodRestriction(FoodRestrictionDatabase database, Pawn_FoodRestrictionTracker tracker)
        {
            if (tracker?.pawn?.RaceProps?.Animal == true)
            {
                var comp = Current.Game?.GetComponent<AnimalControlsRestrictions>();
                if (comp?.DefaultRestriction == null) return database?.DefaultFoodRestriction();
                return comp.DefaultRestriction;
            }
            else
                return database?.DefaultFoodRestriction();
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            bool b0 = false;
            MethodBase Ldfr = AccessTools.Method(typeof(FoodRestrictionDatabase), nameof(FoodRestrictionDatabase.DefaultFoodRestriction));
            MethodBase Lget = AccessTools.Method(typeof(Pawn_FoodRestrictionTracker_CurrentFoodRestriction_Getter_AnimalControlsPatch), nameof(Pawn_FoodRestrictionTracker_CurrentFoodRestriction_Getter_AnimalControlsPatch.GetDefaultFoodRestriction));
            foreach (var i in instrs)
            {
                if (i.opcode == OpCodes.Callvirt && i.operand == (object)Ldfr)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    i.operand = Lget;
                    b0 = true;
                }
                yield return i;
            }
            if (!b0) Log.Warning("[Animal Controls] CurrentFoodRestriction patch 0 didn't work");
        }
    }
}

