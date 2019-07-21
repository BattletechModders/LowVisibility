using BattleTech;
using Harmony;
using LowVisibility.Helper;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {
    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnMech")]
    public static class UnitSpawnPointGameLogic_SpawnMech {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Mech __result) {

            Mod.Log.Debug($"=== SpawnMech entered for {CombatantUtils.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
        }
    }

    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnVehicle")]
    public static class UnitSpawnPointGameLogic_SpawnVehicle {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Vehicle __result) {

            Mod.Log.Debug($"=== SpawnVehicle entered for {CombatantUtils.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
        }
    }

    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnTurret")]
    public static class UnitSpawnPointGameLogic_SpawnTurret {

        // Perform visibility updates at this point, after the unit has spawned and has been added to a team.
        public static void Postfix(UnitSpawnPointGameLogic __instance, Turret __result) {

            Mod.Log.Debug($"=== SpawnTurret entered for {CombatantUtils.Label(__result)}.");
            ECMHelper.UpdateECMState(__result);
        }
    }
}
