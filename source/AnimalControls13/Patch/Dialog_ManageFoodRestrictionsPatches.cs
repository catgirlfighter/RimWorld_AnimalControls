using System;
using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using UnityEngine;

namespace AnimalControls.Patch
{
    //adding a tab with global restrictions to the food restriction editor
    [HarmonyPatch(typeof(Dialog_ManageFoodRestrictions), MethodType.Constructor, new Type[] { typeof(FoodRestriction) })]
    static class Dialog_ManageFoodRestrictions_Constructor_AnimalControlsPatch
    {
        static FieldInfo FfoodGlobalFilter = null;
        static bool Prepare()
        {

            FfoodGlobalFilter = AccessTools.Field(typeof(Dialog_ManageFoodRestrictions), "foodGlobalFilter");
            if (FfoodGlobalFilter == null)
                throw new Exception("Can't get field Dialog_ManageFoodRestrictions.foodGlobalFilter");
            return true;
        }

        static void Postfix(ref Dialog_ManageFoodRestrictions __instance)
        {
            ThingFilter f = (ThingFilter)FfoodGlobalFilter.GetValue(__instance);
            f.SetAllow(AnimalControlsDefOf.Plants, true, null, null);
        }
    }

    [HarmonyPatch(typeof(Dialog_ManageFoodRestrictions), "DoWindowContents")]
    static class Dialog_ManageFoodRestrictions_DoWindowContents_AnimalControlsPatch
    {
        static TaggedString asText(this FoodRestriction restriction)
        {
            return restriction == null ? "AnimalControlsDefault".Translate() : (TaggedString)restriction.label;
        }

        static void reset()
        {
            AnimalControls.lookingAtDefaults = false;
        }

        static bool DefaultsMenu(Dialog_ManageFoodRestrictions dialog, Rect inRect)
        {
            if (!AnimalControls.lookingAtDefaults) return false;

            var comp = Current.Game.GetComponent<AnimalControlsRestrictions>();

            GUI.BeginGroup(inRect);

            Widgets.Label(new Rect(5f, 8f, 300f, 30f), "AnimalControlsAnimalRestrictionLabel".Translate());
            Widgets.DrawHighlightIfMouseover(new Rect(0f, 0f, 460f, 35f));
            if (Widgets.ButtonText(new Rect(310f, 0f, 150f, 35f), comp.DefaultRestriction.asText(), true, true, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("AnimalControlsDefault".Translate(), delegate ()
                {
                    comp.DefaultRestriction = null;
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
                foreach (FoodRestriction fr in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
                {
                    list.Add(new FloatMenuOption(fr.label, delegate ()
                    {
                        comp.DefaultRestriction = fr;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Widgets.Label(new Rect(5f, 48f, 300f, 30f), "AnimalControlsHandlerRestrictionLabel".Translate());
            Widgets.DrawHighlightIfMouseover(new Rect(0f, 40f, 460f, 35f));
            if (Widgets.ButtonText(new Rect(310f, 40f, 150f, 35f), comp.HandlerRestriction.asText(), true, true, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("AnimalControlsDefault".Translate(), delegate ()
                {
                    comp.HandlerRestriction = null;
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
                foreach (FoodRestriction fr in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
                {
                    list.Add(new FloatMenuOption(fr.label, delegate ()
                    {
                        comp.HandlerRestriction = fr;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            GUI.EndGroup();

            return true;
        }

        static void Postfix(Rect inRect)
        {
            float num;
            Rect rect;
            if (AnimalControls.saveStorageSettingsModActive && !AnimalControls.lookingAtDefaults)
            {
                num = 10f * 2 + 150f * 2;
                rect = new Rect(num, 50f, 150f, 35f);
            }
            else
            {
                num = 10f * 3 + 150f * 3;
                rect = new Rect(num, 0f, 150f, 35f);
            }
            if (Widgets.ButtonText(rect, AnimalControls.lookingAtDefaults ? "Back".Translate() : "AnimalControlsDefaultsLabel".Translate(), true, true, true))
            {
                AnimalControls.lookingAtDefaults = !AnimalControls.lookingAtDefaults;
            }
        }

        static bool Prefix(Dialog_ManageFoodRestrictions __instance, Rect inRect)
        {
            if (!AnimalControls.lookingAtDefaults)
                return true;
            Rect rect = new Rect(0f, -10f, inRect.width, inRect.height - 40f - Window.CloseButSize.y).ContractedBy(10f);
            DefaultsMenu(__instance, rect);

            return false;
        }
        /*
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs, ILGenerator il)
        {
            MethodInfo Lget_sfr = AccessTools.Method(typeof(Dialog_ManageFoodRestrictions), "get_SelectedFoodRestriction");
            MethodInfo Lset_sfr = AccessTools.Method(typeof(Dialog_ManageFoodRestrictions), "set_SelectedFoodRestriction");
            FieldInfo Llooking = AccessTools.Field(typeof(AnimalControls),nameof(AnimalControls.lookingAtDefaults));
            MethodInfo Ldefaultsmenu = AccessTools.Method(typeof(Dialog_ManageFoodRestrictions_DoWindowContents_AnimalControlsPatch), nameof(Dialog_ManageFoodRestrictions_DoWindowContents_AnimalControlsPatch.DefaultsMenu));
            MethodInfo Lmake = AccessTools.Method(typeof(FoodRestrictionDatabase), nameof(FoodRestrictionDatabase.MakeNewFoodRestriction));
            MethodInfo Lbuttontext = AccessTools.Method(typeof(Widgets), nameof(Widgets.ButtonText), new Type[] { typeof(Rect), typeof(string), typeof(bool), typeof(bool), typeof(bool) });
            MethodInfo Lreset = AccessTools.Method(typeof(Dialog_ManageFoodRestrictions_DoWindowContents_AnimalControlsPatch), nameof(Dialog_ManageFoodRestrictions_DoWindowContents_AnimalControlsPatch.reset));
            CodeInstruction oldi = null;
            foreach (var i in (instrs))
            {
                if (oldi != null)
                {
                    if ( i.opcode == OpCodes.Brtrue_S && oldi.opcode == OpCodes.Call && oldi.operand == (object)Lget_sfr)
                    {
                        Label l = il.DefineLabel();
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        yield return new CodeInstruction(OpCodes.Call, Ldefaultsmenu);
                        yield return new CodeInstruction(OpCodes.Brfalse_S, l);
                        yield return new CodeInstruction(OpCodes.Ret);
                        var ci = new CodeInstruction(OpCodes.Ldarg_0);
                        ci.labels.Add(l);
                        yield return ci;
                    }

                    yield return oldi;

                    if (i.opcode == OpCodes.Call && i.operand == (object)Lset_sfr && oldi.opcode == OpCodes.Callvirt && oldi.operand == (object)Lmake
                        || i.opcode == OpCodes.Brfalse && oldi.opcode == OpCodes.Call && oldi.operand == (object)Lbuttontext
                        || i.opcode == OpCodes.Brfalse_S && oldi.opcode == OpCodes.Call && oldi.operand == (object)Lbuttontext)
                    {
                        oldi = null;
                        yield return i;
                        yield return new CodeInstruction(OpCodes.Call, Lreset);
                        continue;
                    }
                }
                oldi = i;
            }
            if (oldi != null) yield return oldi;
        }
        */
    }

    [HarmonyPatch(typeof(Dialog_ManageFoodRestrictions), "PreClose")]
    static class Dialog_ManageFoodRestrictions_PreClose_AnimalControlsPatch
    {
        static void Prefix()
        {
            AnimalControls.lookingAtDefaults = false;
        }
    }
}
