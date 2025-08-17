using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using AnimalControls.Patch;

namespace AnimalControls
{
    public class Settings : ModSettings
    {

        public static bool allow_feeding_with_plants = true;
        public static bool animals_prefer_corpses = false;
        public static bool animals_pay_attention = true;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("ac_allow_feeding_with_plants_label".Translate(), ref allow_feeding_with_plants, "ac_allow_feeding_with_plants_note".Translate());
			listing_Standard.CheckboxLabeled("ac_animals_prefer_corpses".Translate(), ref animals_prefer_corpses, "ac_animals_prefer_corpses_note".Translate());
			listing_Standard.CheckboxLabeled("ac_animals_pay_attention_label".Translate(), ref animals_pay_attention, "ac_animals_pay_attention_note".Translate());
            listing_Standard.Label("ac_nutrition_limit_per_piece".Translate(Math.Round(AnimalControls.TrainAnimalNutritionLimit, 2).ToString()));
            AnimalControls.TrainAnimalNutritionLimit = listing_Standard.Slider(AnimalControls.TrainAnimalNutritionLimit, 0f, 10f);
            var comp = Current.Game?.GetComponent<AnimalControlsRestrictions>();
            if (comp != null)
            {
                listing_Standard.Label("AnimalControlsDefaultsLabel".Translate());
                var dialogRect = new Rect(0f, 6 * 26f, inRect.width, inRect.height);
                Dialog_ManageFoodRestrictions_DoWindowContents_AnimalControlsPatch.DefaultsMenu(dialogRect, true);
            }
            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allow_feeding_with_plants, "allow_feeding_with_plants", true, false);
            Scribe_Values.Look(ref animals_pay_attention, "animals_pay_attention", true, false);
			Scribe_Values.Look(ref animals_prefer_corpses, nameof(animals_prefer_corpses), false, false);
			Scribe_Values.Look(ref AnimalControls.TrainAnimalNutritionLimit, "nutrition_limit_per_piece", 0.1f, false);
        }
    }
}