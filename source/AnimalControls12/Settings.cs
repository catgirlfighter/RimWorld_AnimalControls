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
            listing_Standard.CheckboxLabeled("allow_feeding_with_plants_label".Translate(), ref allow_feeding_with_plants, "allow_feeding_with_plants_note".Translate());
            listing_Standard.CheckboxLabeled("animals_pay_attention_label".Translate(), ref animals_pay_attention, "animals_pay_attention_note".Translate());
            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allow_feeding_with_plants, "allow_feeding_with_plants", true, false);
            Scribe_Values.Look(ref animals_pay_attention, "animals_pay_attention", true, false);
        }
    }
}
