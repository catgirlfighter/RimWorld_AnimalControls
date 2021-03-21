using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using RimWorld;

namespace AnimalControls
{
    //xml-defined defs for new categories where plants, crop and trees will be stated
    [DefOf]
    public static class AnimalControlsDefOf
    {
        public static ThingCategoryDef Plants;
        public static ThingCategoryDef Crops;
        public static ThingCategoryDef Trees;
        public static ThingCategoryDef OtherEdible;

        public static JobDef AnimalControls_Wait_PayAttention;
    }

    //mod basics
    [StaticConstructorOnStartup]
    public class AnimalControls : Mod
    {
        public const  float TrainAnimalNutritionLimit = 0.1f;
        public static float BestFoodSourceOnMap_maxNutrition = float.MaxValue;

        public static void SetBestFoodSourceOnMap_maxNutrition(float val)
        {
            BestFoodSourceOnMap_maxNutrition = val;
        }

        public static bool lookingAtDefaults = false;
        public AnimalControls(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("net.avilmask.rimworld.mod.AnimalControls");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
           GetSettings<Settings>();
        }

        public void Save()
        {
            LoadedModManager.GetMod<AnimalControls>().GetSettings<Settings>().Write();
        }

        public override string SettingsCategory()
        {
            return "AnimalControls";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}
