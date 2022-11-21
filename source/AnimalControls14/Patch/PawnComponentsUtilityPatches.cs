using HarmonyLib;
using RimWorld;
using Verse;


namespace AnimalControls.Patch
{
    //adding food restiriction tracker to player controlled animals
    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    static class PawnComponentsUtility_AddAndRemoveDynamicComponents_AnimalControlsPatch
    {
        internal static void Postfix(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                Faction faction = pawn.Faction;
                if (faction == null || !faction.IsPlayer)
                {
                    Faction hostFaction = pawn.HostFaction;
                    if (hostFaction == null || !hostFaction.IsPlayer)
                    {
                        return;
                    }
                }

                if (pawn.foodRestriction == null) pawn.foodRestriction = new Pawn_FoodRestrictionTracker(pawn);
            }
        }
    }
}
