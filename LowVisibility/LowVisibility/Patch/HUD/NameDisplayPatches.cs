using BattleTech;
using BattleTech.UI;
using Harmony;
using IRBTModUtils;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;

namespace LowVisibility.Patch {

    static class CombatNameHelper {

        /*
           Helper method used to find the label text for any enemy vehicles and turrets based on the visiblity and senors levels.
          
            chassisName -> ICombatant.UnitName = (Vehicle/Turret)Def.Chassis.Description.Name -> Carrier / Vargr APC / ArrowIV Chassis
            fullname -> ICombatant.NickName = (Vehicle/Turret)Def.Description.Name -> AC/2 Carrier / Vargr APC / Arrow IV Turret
        */
        public static Text GetTurretOrVehicleDetectionLabel(VisibilityLevel visLevel, SensorScanType sensorScanType,
            string fullName, string chassisName, bool isVehicle=true) {

            Text label = new Text("?");
            
            if (visLevel >= VisibilityLevel.Blip0Minimum)
            {
                string labelKey = isVehicle ? ModText.LT_UNIT_TYPE_VEHICLE : ModText.LT_UNIT_TYPE_TURRET;
                string typeS = new Text(Mod.LocalizedText.StatusPanel[labelKey]).ToString();

                if (sensorScanType == SensorScanType.NoInfo) label = new Text("?");
                else if (sensorScanType == SensorScanType.LocationAndType) label = new Text(typeS);
                else if (sensorScanType == SensorScanType.ArmorAndWeaponType) label = new Text($"{chassisName}");
                else if (sensorScanType == SensorScanType.StructAndWeaponID) label = new Text($"{fullName}");
                else if (sensorScanType == SensorScanType.AllInformation) label = new Text($"{fullName}");
            }

            return label;
        }


        /*
           Helper method used to find the label text for any enemy mechs based on the visiblity and senors levels.
         
           Parameters:
            chassisName -> Mech.UnitName = MechDef.Chassis.Description.Name -> Shadow Hawk / Atlas / Marauder                 
                    (The name of the base chassis, even if customized chassis (such as RogueOmnis))

            partialName -> Mech.NickName = MechDef.Description.Name -> Shadow Hawk SHD-2D / Atlas AS7-D / Marauder ANU-O      
                    (Partial name, most cases chassis and variant name combined, but for some elite mechs can be "less precise" to trick the player which mech it is)

            fullname -> Mech.NickName = MechDef.Description.UIName -> Shadow Hawk SHD-2D / Atlas AS7-D Danielle / Anand ANU-O
                    (Full name, will almost always display the full actual name, and if a hero/elite mech the chassis name is replaced by its custom name. ONly exception is LA's hidden nasty surprises, such as Nuke mechs)
        */
        public static Text GetEnemyMechDetectionLabel(VisibilityLevel visLevel, SensorScanType sensorScanType,
            string typeName,string fullName, string partialName, string chassisName)
        {

            Text label = new Text("?");

            if (visLevel >= VisibilityLevel.Blip0Minimum)
            {
                string typeS = new Text(string.IsNullOrEmpty(typeName)?Mod.LocalizedText.StatusPanel[ModText.LT_UNIT_TYPE_MECH]:typeName).ToString();

                if (sensorScanType == SensorScanType.NoInfo) label = new Text("?");
                else if (sensorScanType == SensorScanType.LocationAndType) label = new Text(typeS);
                else if (sensorScanType == SensorScanType.ArmorAndWeaponType) label = new Text($"{chassisName}");
                else if (sensorScanType == SensorScanType.StructAndWeaponID) label = new Text($"{partialName}");
                else if (sensorScanType == SensorScanType.AllInformation) label = new Text($"{fullName}");
            }

            return label;
        }

        /*
           Helper method used to find the label text for any non-hostile mechs. 
           If a custom name has been set by player, the fullName will be null and displayName used instead.

            displayName -> Mech.DisplayName -> Custom name of the mech
          
            fullName -> Mech.NickName = MechDef.Description.UIName -> Shadow Hawk SHD-2D / Atlas AS7-D Danielle / Anand ANU-O
                    (Full name, will almost always display the full actual name, and if a hero/elite mech the chassis name is replaced by its custom name. ONly exception is LA's hidden nasty surprises, such as Nuke mechs)
        */
        public static Text GetNonHostileMechDetectionLabel(ICombatant target, string fullName, string displayName)
        {
            Text label;
            if (string.IsNullOrEmpty(fullName))
            {
                label = new Text($"{displayName}");
            }
            else
            {
                label = new Text($"{fullName}");
            }
            Mod.Log.Debug?.Write($"GetNonHostileMechDetectionLabel - label:({label}) fullName:({fullName})");
            return label;
        }

        private enum CombatHUDLabelLevel
        {
            Type,          // MECH
            Chassis,       // Atlas
            Partial,       // Atlas AS7-D
            Full           // Atlas AS7-D Danielle
        }
    }

    // --- HIDE UNIT NAME PATCHES ---
    [HarmonyPatch(typeof(Mech), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Mech_GetActorInfoFromVisLevel {

        public static void Postfix(Mech __instance, ref Text __result, VisibilityLevel visLevel) {
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

                __result = CombatNameHelper.GetEnemyMechDetectionLabel(visLevel, scanType, typeName, fullName, partialName, chassisName);
            }
            else
            {
                string displayName = __instance.DisplayName;

               __result = CombatNameHelper.GetNonHostileMechDetectionLabel(__instance, fullName, displayName);
            }
        }
    }

    [HarmonyPatch(typeof(Turret), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Turret_GetActorInfoFromVisLevel {
        public static void Postfix(Turret __instance, ref Text __result, VisibilityLevel visLevel) {
            if (__instance == null) { return; }

            /*
                Turret.UnitName = return (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Chassis.Description.Name ->
                Turret.NickName = (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Description.Name ->
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                string chassisName = __instance.UnitName;
                string fullName = __instance.Nickname;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                if (scanType < SensorScanType.ArmorAndWeaponType)
                {
                    bool hasVisualScan = VisualLockHelper.CanSpotTargetUsingCurrentPositions(ModState.LastPlayerActorActivated, __instance);
                    if (hasVisualScan) scanType = SensorScanType.ArmorAndWeaponType;
                }

                Text response = CombatNameHelper.GetTurretOrVehicleDetectionLabel(visLevel, scanType, fullName, chassisName, false);
                __result = response;
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Vehicle_GetActorInfoFromVisLevel {
        public static void Postfix(Vehicle __instance, ref Text __result, VisibilityLevel visLevel) {
            if (__instance == null) { return; };

            /*
                Vehicle.UnitName = VehicleDef.Chassis.Description.Name -> 
                    Alacorn Mk.VI-P / vehicledef_ARES_CLAN / Demolisher II / Galleon GAL-102
                Vehicle.NickName = VehicleDef.Description.Name -> 
                    Pirate Alacorn Gauss Carrier / Ares / Demolisher II`
                    VehicleDef.Description.Id ->
                        / / vehicledef_DEMOLISHER-II / vehicledef_GALLEON_GAL102
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                string chassisName = __instance.UnitName;
                string fullName = __instance.Nickname;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                if (scanType < SensorScanType.ArmorAndWeaponType)
                {
                    bool hasVisualScan = VisualLockHelper.CanSpotTargetUsingCurrentPositions(ModState.LastPlayerActorActivated, __instance);
                    if (hasVisualScan) scanType = SensorScanType.ArmorAndWeaponType;
                }

                Text response = CombatNameHelper.GetTurretOrVehicleDetectionLabel(visLevel, scanType, fullName, chassisName, true);
                __result = response;
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

                // TODO: Needs to be hidden or localized
                string nameText = scanType >= SensorScanType.AllInformation ? ___displayedActor.GetPilot().Name : "";
                __instance.PilotNameText.SetText(nameText);
                
            }
        }
    }


}
