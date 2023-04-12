using BattleTech.UI;
using IRBTModUtils;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;

namespace LowVisibility.Patch
{

    // --- HIDE UNIT NAME PATCHES ---
    [HarmonyPatch(typeof(Mech), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Mech_GetActorInfoFromVisLevel
    {

        public static void Postfix(Mech __instance, ref Text __result, VisibilityLevel visLevel)
        {
            if (__instance == null) { return; }

            /*
                Mech.UnitName = MechDef.Chassis.Description.Name -> Shadow Hawk / Atlas / Marauder
                Mech.Nickname = Mech.Description.Name -> Shadow Hawk SHD-2D / Atlas AS7-D / Marauder ANU-O
                Mech.Description.UIName -> Shadow Hawk SHD-2D / Atlas AS7-D Danielle / Anand ANU-O
            */
            string fullName = __instance.Description.UIName;
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID))
            {
                string chassisName = __instance.UnitName;
                string partialName = __instance.Nickname;
                string typeName = (__instance is ICustomMech custMech) ? custMech.UnitTypeName : string.Empty;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                if (scanType < SensorScanType.ArmorAndWeaponType)
                {
                    bool hasVisualScan = VisualLockHelper.CanSpotTargetUsingCurrentPositions(ModState.LastPlayerActorActivated, __instance);
                    if (hasVisualScan) scanType = SensorScanType.ArmorAndWeaponType;
                }

                string enemyMechName = UnitDetectionNameHelper.GetEnemyMechName(visLevel, scanType, typeName, fullName, partialName, chassisName);
                __result = new Text($"{enemyMechName}");
            }
            else
            {
                string displayName = __instance.DisplayName;

                string nonHostileMechName = UnitDetectionNameHelper.GetNonHostileMechName(fullName, displayName);
                __result = new Text(nonHostileMechName);
            }
        }
    }

    [HarmonyPatch(typeof(Turret), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Turret_GetActorInfoFromVisLevel
    {
        public static void Postfix(Turret __instance, ref Text __result, VisibilityLevel visLevel)
        {
            if (__instance == null) { return; }

            /*
                Turret.UnitName = return (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Chassis.Description.Name ->
                Turret.NickName = (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Description.Name ->
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID))
            {
                string chassisName = __instance.UnitName;
                string fullName = __instance.Nickname;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                if (scanType < SensorScanType.ArmorAndWeaponType)
                {
                    bool hasVisualScan = VisualLockHelper.CanSpotTargetUsingCurrentPositions(ModState.LastPlayerActorActivated, __instance);
                    if (hasVisualScan) scanType = SensorScanType.ArmorAndWeaponType;
                }

                string name = UnitDetectionNameHelper.GetTurretName(visLevel, scanType, fullName, chassisName);
                __result = new Text(name);
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Vehicle_GetActorInfoFromVisLevel
    {
        public static void Postfix(Vehicle __instance, ref Text __result, VisibilityLevel visLevel)
        {
            if (__instance == null) { return; };

            /*
                Vehicle.UnitName = VehicleDef.Chassis.Description.Name -> 
                    Alacorn Mk.VI-P / vehicledef_ARES_CLAN / Demolisher II / Galleon GAL-102
                Vehicle.NickName = VehicleDef.Description.Name -> 
                    Pirate Alacorn Gauss Carrier / Ares / Demolisher II`
                    VehicleDef.Description.Id ->
                        / / vehicledef_DEMOLISHER-II / vehicledef_GALLEON_GAL102
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID))
            {
                string chassisName = __instance.UnitName;
                string fullName = __instance.Nickname;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                if (scanType < SensorScanType.ArmorAndWeaponType)
                {
                    bool hasVisualScan = VisualLockHelper.CanSpotTargetUsingCurrentPositions(ModState.LastPlayerActorActivated, __instance);
                    if (hasVisualScan) scanType = SensorScanType.ArmorAndWeaponType;
                }

                string name = UnitDetectionNameHelper.GetVehicleName(visLevel, scanType, fullName, chassisName);
                __result = new Text(name);
            }
        }
    }

    // Hide the pilot name unless you have all info
    [HarmonyPatch(typeof(CombatHUDActorNameDisplay), "RefreshInfo")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class CombatHUDActorNameDisplay_RefreshInfo
    {

        public static void Postfix(CombatHUDActorNameDisplay __instance, VisibilityLevel visLevel, AbstractActor ___displayedActor)
        {
            if (___displayedActor != null && ModState.LastPlayerActorActivated != null && ModState.TurnDirectorStarted
                && !___displayedActor.Combat.HostilityMatrix.IsLocalPlayerFriendly(___displayedActor.TeamId))
            {

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(___displayedActor, ModState.LastPlayerActorActivated);

                string pilotName = PilotNameHelper.GetEnemyPilotName(visLevel, scanType, ___displayedActor);
                __instance.PilotNameText.SetText(pilotName);

            }
        }
    }


}
