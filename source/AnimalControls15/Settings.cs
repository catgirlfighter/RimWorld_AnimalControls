using System;
using UnityEngine;
using Verse;

namespace AnimalControls
{
    public class Settings : ModSettings
    {

        public static bool allow_feeding_with_plants = true;
        public static bool animals_pay_attention = true;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("ac_allow_feeding_with_plants_label".Translate(), ref allow_feeding_with_plants, "ac_allow_feeding_with_plants_note".Translate());
            listing_Standard.CheckboxLabeled("ac_animals_pay_attention_label".Translate(), ref animals_pay_attention, "ac_animals_pay_attention_note".Translate());
            listing_Standard.Label("ac_nutrition_limit_per_piece".Translate(Math.Round(AnimalControls.TrainAnimalNutritionLimit, 2).ToString()));
            AnimalControls.TrainAnimalNutritionLimit = listing_Standard.Slider(AnimalControls.TrainAnimalNutritionLimit, 0f, 10f);
            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allow_feeding_with_plants, "allow_feeding_with_plants", true, false);
            Scribe_Values.Look(ref animals_pay_attention, "animals_pay_attention", true, false);
            Scribe_Values.Look(ref AnimalControls.TrainAnimalNutritionLimit, "nutrition_limit_per_piece", 0.1f, false);
        }
    }
}
