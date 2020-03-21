using BattleTech;
using Harmony;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;

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

            if (visLevel == VisibilityLevel.LOSFull) {
                
                if (sensorScanType >= SensorScanType.DeepScan) {
                    label = new Text($"{fullName}");
                } else if (sensorScanType >= SensorScanType.SurfaceAnalysis) {
                    label = new Text($"{chassisName} ({tonnage}t)");
                } else {
                    // Silhouette or better
                    label = new Text($"{chassisName}");
                }
            } else if (visLevel == VisibilityLevel.Blip4Maximum) {
                label = new Text($"{fullName}");
            } else if (visLevel == VisibilityLevel.Blip1Type) {
                label = new Text($"{chassisName} ({tonnage}t)");
            } else if (visLevel == VisibilityLevel.Blip0Minimum) {
                label = new Text($"{chassisName}");
            } else if (visLevel == VisibilityLevel.BlobSmall) {
                label = new Text($"{type}");
            } else {
                label = new Text($"?");
            }

            Mod.Log.Debug($"GetTurretOrVehicleDetectionLabel - label:({label}) for visLevel:{visLevel} " +
                $"chassisName:({chassisName}) fullName:({fullName}) type:({type}) tonnage:{tonnage}t");
            return label;
        }


        /*
           Helper method used to find the label text for any enemy mechs based on the visiblity and senors levels.
         
           Parameters:
            chassisName -> Mech.UnitName = MechDef.Chassis.Description.Name -> Shadow Hawk / Atlas / Marauder
               - The name of the base chassis, even if customized chassis (such as RogueOmnis)
            partialName -> Mech.NickName = MechDef.Description.Name -> Shadow Hawk SHD-2D / Atlas AS7-D / Marauder ANU-O
               - Partial name, most cases chassis and variant name combined, but for some elite mechs can be "less precise" to trick the player which mech it is
            fullname -> Mech.NickName = MechDef.Description.UIName -> Shadow Hawk SHD-2D / Atlas AS7-D Danielle / Anand ANU-O
               - Full name, will almost always display the full actual name, and if a hero/elite mech the chassis name is replaced by its custom name. ONly exception is LA's hidden nasty surprises, such as Nuke mechs
        */
        public static Text GetEnemyMechDetectionLabel(ICombatant target, VisibilityLevel visLevel, SensorScanType sensorScanType,
            string fullName, string partialName, string chassisName, float tonnage)
        {

            Text label = new Text("?");

            if (visLevel == VisibilityLevel.LOSFull)
            {

                if (sensorScanType >= SensorScanType.DeepScan)
                {
                    label = new Text($"{fullName}");
                }
                else if (sensorScanType >= SensorScanType.SurfaceAnalysis)
                {
                    label = new Text($"{partialName}");
                }
                else
                {
                    // Silhouette or better
                    label = new Text($"{chassisName} ({tonnage}t)");
                }
            }
            else if (visLevel == VisibilityLevel.Blip4Maximum)
            {
                label = new Text($"{fullName}");
            }
            else if (visLevel == VisibilityLevel.Blip1Type)
            {
                label = new Text($"{partialName}");
            }
            else if (visLevel == VisibilityLevel.Blip0Minimum)
            {
                label = new Text($"{chassisName}");
            }
            else if (visLevel == VisibilityLevel.BlobSmall)
            {
                label = new Text($"MECH");
            }
            else
            {
                label = new Text($"?");
            }

            Mod.Log.Debug($"GetMechDetectionLabel - label:({label}) for visLevel:{visLevel} " +
                $"chassisName:({chassisName}) partialName:({partialName}) fullName:({fullName}) type:(MECH) tonnage:{tonnage}t");
            return label;
        }

        /*
           Helper method used to find the label text for any non-hostile mechs
          
            chassisName -> Mech.UnitName = MechDef.Chassis.Description.Name -> Shadow Hawk / Atlas / Marauder
               - The name of the base chassis, even if customized chassis (such as RogueOmnis)
            partialName -> Mech.NickName = MechDef.Description.Name -> Shadow Hawk SHD-2D / Atlas AS7-D / Marauder ANU-O
               - Partial name, most cases chassis and variant name combined, but for some elite mechs can be "less precise" to trick the player which mech it is
            fullname -> Mech.NickName = MechDef.Description.UIName -> Shadow Hawk SHD-2D / Atlas AS7-D Danielle / Anand ANU-O
               - Full name, will almost always display the full actual name, and if a hero/elite mech the chassis name is replaced by its custom name. ONly exception is LA's hidden nasty surprises, such as Nuke mechs
        */
        public static Text GetNonHostileMechDetectionLabel(ICombatant target, string fullName)
        {

            Text label = new Text($"{fullName}");
            Mod.Log.Debug($"GetNonHostileMechDetectionLabel - label:({label}) fullName:({fullName})");
            return label;
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
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                string chassisName = __instance.UnitName;
                string partialName = __instance.Nickname;
                float tonnage = __instance.MechDef.Chassis.Tonnage;

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                Text response = CombatNameHelper.GetEnemyMechDetectionLabel(__instance, visLevel, scanType, 
                    fullName, partialName, chassisName, tonnage);
                __result = response;
            }
            else
            {
                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(__instance, null);
                Text response = CombatNameHelper.GetNonHostileMechDetectionLabel(__instance, fullName);
                __result = response;
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



}
