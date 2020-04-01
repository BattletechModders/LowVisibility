using BattleTech;
using BattleTech.UI;
using Harmony;
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
        public static Text GetTurretOrVehicleDetectionLabel(ICombatant target, VisibilityLevel visLevel, SensorScanType sensorScanType,
            string fullName, string chassisName, string type, float tonnage) {

            Text label = new Text("?");


            int visScore = GetVisibilityLevelScore(visLevel);
            int senScore = GetSensorScanTypeScore(sensorScanType);
            CombatHUDLabelLevel labelLevel = GetCombatHUDLabelLevel(visScore, senScore);

            switch (labelLevel)
            {
                case CombatHUDLabelLevel.Type:
                    label = new Text($"{type}");
                    break;
                case CombatHUDLabelLevel.Chassis:
                    label = new Text($"{chassisName}");
                    break;
                case CombatHUDLabelLevel.Weight:
                    label = new Text($"{chassisName} ({tonnage}t)");
                    break;
                case CombatHUDLabelLevel.Partial:
                case CombatHUDLabelLevel.Full:
                    label = new Text($"{fullName}");
                    break;
            }
            Mod.Log.Debug($"GetMechDetectionLabel - label:({label}) for type:({type}) " +
                $"visLevel/visScore:{visLevel}/{visScore} sensorScanType/secScore:{sensorScanType}/{senScore} MechLabelLevel:{labelLevel}" +
                $"chassisName:({chassisName}) fullName:({fullName}) type:(MECH) tonnage:{tonnage}t");
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
        public static Text GetEnemyMechDetectionLabel(ICombatant target, VisibilityLevel visLevel, SensorScanType sensorScanType,
            string fullName, string partialName, string chassisName, float tonnage)
        {

            Text label = new Text("?");

            int visScore = GetVisibilityLevelScore(visLevel);
            int senScore = GetSensorScanTypeScore(sensorScanType);
            CombatHUDLabelLevel labelLevel = GetCombatHUDLabelLevel(visScore, senScore);

            switch (labelLevel)
            {
                case CombatHUDLabelLevel.Type:
                    label = new Text($"MECH");
                    break;
                case CombatHUDLabelLevel.Chassis:
                    label = new Text($"{chassisName}");
                    break;
                case CombatHUDLabelLevel.Weight:
                    label = new Text($"{chassisName} ({tonnage}t)");
                    break;
                case CombatHUDLabelLevel.Partial:
                    label = new Text($"{partialName}");
                    break;
                case CombatHUDLabelLevel.Full:
                    label = new Text($"{fullName}");
                    break;
            }

            Mod.Log.Debug($"GetMechDetectionLabel - label:({label}) for " +
                $"visLevel/visScore:{visLevel}/{visScore} sensorScanType/secScore:{sensorScanType}/{senScore} MechLabelLevel:{labelLevel}" +
                $"chassisName:({chassisName}) partialName:({partialName}) fullName:({fullName}) type:(MECH) tonnage:{tonnage}t");
            return label;
        }

        /*
            Calculate the MechLabelLevel for the mech based on the VisibilityLevel and SensorScanType, with more weight put on the SensorScanType.

            Formula: ( VisibilityLevelScore + 2 x SensorScanTypeScore )

            Results:
            0-2   - None, no information can be deduced                        = No label
            2-5   - Type, basic shape can be detected                          = Mech
            5-10   - Chassis, chassis particularities noticeable               = Atlas
            10-15  - Weight, rough estimate of total weight can be calculated  = Atlas (100t)
            15-21 - Partial, most structure information detectable             = Atlas AS7-D
            21-   - Full, all information available                            = Atlas AS7-D Danielle

         */
        private static CombatHUDLabelLevel GetCombatHUDLabelLevel(decimal visScore, decimal senScore)
        {
            int labelScore = (int) Math.Round((visScore + senScore) / 2);

            if (labelScore < 2)
            {
                return CombatHUDLabelLevel.None;
            }
            if (labelScore < 5)
            {
                return CombatHUDLabelLevel.Type;
            }
            if (labelScore < 10)
            {
                return CombatHUDLabelLevel.Chassis;
            }
            if (labelScore < 15)
            {
                return CombatHUDLabelLevel.Weight;
            }
            if (labelScore < 21)
            {
                return CombatHUDLabelLevel.Partial;
            }
            return CombatHUDLabelLevel.Full;
        }

        /*
          Find the label score value from the visibility level. Range from 0 to 7, if unknown defaults to 0.

            0 - No visibility
            1 - Minimal visiblity
            2 - Limited visibility
            3 - Some visibility
            5 - Partial visibility
            6 - Full visibility 

        */
        private static int GetVisibilityLevelScore(VisibilityLevel visLevel)
        {
            switch (visLevel)
            {
                case VisibilityLevel.Blip0Minimum:
                    return 1;
                case VisibilityLevel.Blip1Type:
                case VisibilityLevel.BlipGhost:
                    return 3;
                case VisibilityLevel.Blip4Maximum:
                    return 5;
                case VisibilityLevel.LOSFull:
                    return 6;
                default:
                    return 0;
            }
        }       

        /*
          Find the label score value from the sensor scan type. Range from 0 to 30, if unknown defaults 0.

            0  - No sensor information
            2  - Type
            4  - Silhouette
            7  - Vector
            9  - Surface Scan
            12 - Surface Analysis
            14 - Weapon Analysis
            16 - Structure Analysis
            22 - Deep Scan
            30 - Dental Records

        */
        private static int GetSensorScanTypeScore(SensorScanType scanType)
        {
            switch (scanType)
            {
                case SensorScanType.NoInfo:
                    return 1;
                case SensorScanType.LocationAndType:
                    return 2;
                case SensorScanType.ArmorAndWeaponType:
                    return 4;
                case SensorScanType.StructAndWeaponID:
                    return 7;
                case SensorScanType.AllInformation:
                    return 9;
                default:
                    return 0;
            }
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
            Mod.Log.Debug($"GetNonHostileMechDetectionLabel - label:({label}) fullName:({fullName})");
            return label;
        }

        private enum CombatHUDLabelLevel
        {
            None,          // ?
            Type,          // MECH
            Chassis,       // Atlas
            Weight,        // Atlas (100t)
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
                float tonnage = __instance.MechDef.Chassis.Tonnage;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                __result = CombatNameHelper.GetEnemyMechDetectionLabel(__instance, visLevel, scanType,
                    fullName, partialName, chassisName, tonnage);
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
                float tonnage = __instance.TurretDef.Chassis.Tonnage;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                Text response = CombatNameHelper.GetTurretOrVehicleDetectionLabel(__instance, visLevel, scanType, 
                    fullName, chassisName, "TURRET", tonnage);
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
                float tonnage = __instance.VehicleDef.Chassis.Tonnage;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                Text response = CombatNameHelper.GetTurretOrVehicleDetectionLabel(__instance, visLevel, scanType,
                    fullName, chassisName, "VEHICLE", tonnage);
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

                if (scanType < SensorScanType.AllInformation)
                {
                    // TODO: Needs to be hidden or localized
                    __instance.PilotNameText.SetText("");
                }
                else
                {
                    __instance.PilotNameText.SetText(___displayedActor.GetPilot().Name);
                }
            }
        }
    }


}
