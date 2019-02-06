using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System.Collections.Generic;

namespace LowVisibility.Patch {
    [HarmonyPatch(typeof(Team), "AddUnit")]
    public static class Team_AddUnit {
        public static void Prefix(Team __instance, AbstractActor unit) {
            
            if (__instance.Combat.TurnDirector.CurrentRound > 1) {
                // We are spawning reinforcements. Do the work ahead of the main call to prevent it from failing in the visibility lookups
                if (__instance.units == null) {
                    __instance.units = new List<AbstractActor>();
                }
                if (__instance.units.Contains(unit)) {
                    return;
                }
                __instance.Combat.combatantAdded = true;
                __instance.units.Add(unit);
                unit.AddToTeam(__instance);

                // Before recalculating visibility, add the dynamic and static states for this actor
                EWState actorEWConfig = new EWState(unit);
                State.EWState[unit.GUID] = actorEWConfig;

                // Build their sourceLocks
                //VisibilityHelper.UpdateDetectionForActor(unit);                

                //HostilityMatrix hostilityMatrix = (HostilityMatrix)Traverse.Create(__instance).Property("HostilityMatrix").GetValue();                
                //if (hostilityMatrix.IsLocalPlayerFriendly(__instance.GUID)) {
                //    unit.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
                //}
                //if (__instance.Combat.TurnDirector.CurrentRound > 0) {
                //    unit.UpdateVisibilityCache(__instance.Combat.GetAllCombatants());
                //}
            }
        }
    }
}
