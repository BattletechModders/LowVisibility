using System.Collections.Generic;

namespace LowVisibility.Patch
{
    [HarmonyPatch(typeof(Team), "AddUnit")]
    public static class Team_AddUnit
    {
        public static void Prefix(ref bool __runOriginal, Team __instance, AbstractActor unit)
        {
            if (!__runOriginal) return;

            if (__instance.Combat.TurnDirector.CurrentRound > 1)
            {
                // We are spawning reinforcements. Do the work ahead of the main call to prevent it from failing in the visibility lookups
                if (__instance.units == null)
                {
                    __instance.units = new List<AbstractActor>();
                }
                if (__instance.units.Contains(unit))
                {
                    return;
                }
                __instance.Combat.combatantAdded = true;
                __instance.units.Add(unit);
                unit.AddToTeam(__instance);
            }
        }
    }
}
