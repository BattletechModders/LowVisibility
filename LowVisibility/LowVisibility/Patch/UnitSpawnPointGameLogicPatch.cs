using BattleTech;
using Harmony;
using LowVisibility.Helper;

namespace LowVisibility.Patch {
    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnMech")]
    public static class UnitSpawnPointGameLogic_SpawnMech {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Mech __result) {

            LowVisibility.Logger.LogIfDebug($"SpawnMech:post - entered for {CombatantHelper.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
            //VisibilityHelper.UpdateDetectionForAllActors(__result.Combat);
            //VisibilityHelper.UpdateVisibilityForAllTeams(__result.Combat);
        }
    }

    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnVehicle")]
    public static class UnitSpawnPointGameLogic_SpawnVehicle {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Vehicle __result) {

            LowVisibility.Logger.LogIfDebug($"SpawnMech:post - entered for {CombatantHelper.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
            //VisibilityHelper.UpdateDetectionForAllActors(__result.Combat);
            //VisibilityHelper.UpdateVisibilityForAllTeams(__result.Combat);
        }
    }

    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnTurret")]
    public static class UnitSpawnPointGameLogic_SpawnTurret {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Turret __result) {

            LowVisibility.Logger.LogIfDebug($"SpawnMech:post - entered for {CombatantHelper.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
            //VisibilityHelper.UpdateDetectionForAllActors(__result.Combat);
            //VisibilityHelper.UpdateVisibilityForAllTeams(__result.Combat);
        }
    }
}
