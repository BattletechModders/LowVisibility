using BattleTech;
using FogOfWar;
using System.Collections.Generic;

namespace LowVisibility.Patch {
    public static class FogOfWarSystem_UpdateViewer { 

        public static bool Prefix(FogOfWarSystem __instance, AbstractActor unit,
            HostilityMatrix ___HostilityMatrix, List<AbstractActor> ___viewers) {

            if (__instance == null || unit == null) { return true; }

            if (unit.IsDead || !___HostilityMatrix.IsLocalPlayerFriendly(unit.team)) {
                // Skip processing if the unit is an enemy or dead
                return false;
            } else if (___HostilityMatrix.IsLocalPlayerFriendly(unit.team) && !___viewers.Contains(unit)) {
                __instance.AddViewer(unit);
            }

            return true;
        }
    }
}
