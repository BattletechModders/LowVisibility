using BattleTech;
using FogOfWar;
using Harmony;
using System.Collections.Generic;

namespace LowVisibility.Patch {

    // May fix FoW NRE - see https://github.com/BattletechModders/LowVisibility/issues/22
    [HarmonyPatch(typeof(FogOfWarSystem), "UpdateViewer")]
    public static class FogOfWarSystem_UpdateViewer { 

        public static bool Prefix(FogOfWarSystem __instance, AbstractActor unit, List<AbstractActor> ___viewers) {

            if (__instance == null || unit == null) { return true; }

            if (unit.IsDead || !__instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(unit.team)) {
                // Skip processing if the unit is an enemy or dead
                return false;
            } else if (__instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(unit.team) && !___viewers.Contains(unit)) {
                __instance.AddViewer(unit);
            }

            return true;
        }
    }
}
