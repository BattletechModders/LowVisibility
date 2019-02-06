using BattleTech;
using Harmony;
using LowVisibility.Helper;

namespace LowVisibility.Patch {
    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnMech")]
    public static class UnitSpawnPointGameLogic_SpawnMech {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Mech __result) {

            LowVisibility.Logger.LogIfDebug($"=== SpawnMech entered for {CombatantHelper.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
        }
    }

    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnVehicle")]
    public static class UnitSpawnPointGameLogic_SpawnVehicle {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Vehicle __result) {

            LowVisibility.Logger.LogIfDebug($"=== SpawnVehicle entered for {CombatantHelper.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
        }
    }

    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnTurret")]
    public static class UnitSpawnPointGameLogic_SpawnTurret {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Turret __result) {

            LowVisibility.Logger.LogIfDebug($"=== SpawnTurret entered for {CombatantHelper.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
        }
    }
}
