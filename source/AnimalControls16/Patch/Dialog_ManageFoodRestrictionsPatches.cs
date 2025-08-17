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
    [HarmonyPatch(typeof(Dialog_ManageFoodPolicies), MethodType.Constructor, new Type[] { typeof(FoodPolicy) })]
    static class Dialog_ManageFoodPolicies_Constructor_AnimalControlsPatch
    {
        static PropertyInfo PFoodGlobalFilter = null;
        internal static bool Prepare()
        {

            PFoodGlobalFilter = AccessTools.Property(typeof(Dialog_ManageFoodPolicies), "FoodGlobalFilter");
            if (PFoodGlobalFilter == null)
                throw new Exception("Can't get field Dialog_ManageFoodPolicies.FoodGlobalFilter");
            return true;
        }

        internal static void Postfix(ref Dialog_ManageFoodPolicies __instance)
        {
            ThingFilter f = (ThingFilter)PFoodGlobalFilter.GetValue(__instance);
            f.SetAllow(AnimalControlsDefOf.Plants, true, null, null);
        }
    }

    [HarmonyPatch(typeof(Dialog_ManagePolicies<FoodPolicy>), "DoWindowContents")]
    public static class Dialog_ManageFoodRestrictions_DoWindowContents_AnimalControlsPatch
    {
        public static TaggedString asText(this FoodPolicy restriction)
        {
            return restriction == null ? "AnimalControlsDefault".Translate() : (TaggedString)restriction.label;
        }

        public static bool DefaultsMenu(Rect inRect, bool ForceShow = false)
        {
            if (!ForceShow && !AnimalControls.lookingAtDefaults) return false;

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
                foreach (FoodPolicy fr in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
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
                foreach (FoodPolicy fr in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
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

        internal static void Postfix(Rect inRect)
        {
            Rect rect;
            float top = inRect.yMax - Window.CloseButSize.y + 2f;
            float left = inRect.xMax - Window.CloseButSize.x;
            rect = new Rect(left, top, Window.CloseButSize.x, Window.CloseButSize.y);
            if (Widgets.ButtonText(rect, AnimalControls.lookingAtDefaults ? "Back".Translate() : "AnimalControlsDefaultsLabel".Translate()))
            {
                AnimalControls.lookingAtDefaults = !AnimalControls.lookingAtDefaults;
            }
        }

        internal static bool Prefix(Dialog_ManageFoodPolicies __instance, Rect inRect)
        {
            if (!AnimalControls.lookingAtDefaults)
                return true;
            Rect rect = new Rect(0f, -10f, inRect.width, inRect.height - 40f - Window.CloseButSize.y).ContractedBy(10f);
            DefaultsMenu(rect);

            return false;
        }
    }

    [HarmonyPatch(typeof(Dialog_ManagePolicies<FoodPolicy>), "PreClose")]
    static class Dialog_ManageFoodRestrictions_PreClose_AnimalControlsPatch
    {
        internal static void Prefix()
        {
            AnimalControls.lookingAtDefaults = false;
        }
    }
}
