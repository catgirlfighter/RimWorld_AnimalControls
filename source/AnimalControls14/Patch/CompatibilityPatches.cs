using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using System;

namespace AnimalControls.Patch
{
    [HarmonyPatch]
    static class CompatibilityPatches
    {
        static MethodBase target;
        internal static bool Prepare()
        {
            Type targetType = AccessTools.TypeByName("SaveStorageSettings.Patch_Dialog_ManageFoodRestrictions");
            if (targetType == null) return false;
            target = AccessTools.Method(targetType, "Postfix");
            return target != null;
        }

        internal static MethodBase TargetMethod()
        {
            return target;
        }

        internal static bool Prefix()
        {
            return !AnimalControls.lookingAtDefaults;
        }
    }
}
