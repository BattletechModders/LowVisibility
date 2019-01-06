using BattleTech;
using BattleTech.UI;
using Harmony;
using Localize;
using System;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    static class CombatNameHelper {

        /*
            Mech.UnitName = Atlas
            Mech.VariantName = AS7-D
            Mech.NickName = Atlas II AS7-D-HT or Atlas AS7-D
        */
        public static Text GetDetectionLabel(VisibilityLevel visLevel, LockState lockState,
            string fullName, string variantName, string chassisName, string type, float tonnage) {

            Text response = new Text("?");

            // TODO: Refine VisualID, Silhouette values here
            if (visLevel == VisibilityLevel.LOSFull) {
                // HBS: Full details
                if (lockState.sensorType == SensorLockType.ProbeID) {
                    response = new Text($"{chassisName} {variantName}");
                } else if (lockState.sensorType == SensorLockType.SensorID) {
                    response = new Text($"{chassisName} {tonnage}t");
                } else if (lockState.visionType >= VisionLockType.Silhouette) {
                    response = new Text($"{chassisName}");
                } else {
                    response = new Text($"{type}");
                }

            } else if (visLevel >= VisibilityLevel.Blip0Minimum) {
                // HBS: Type only
                if (lockState.sensorType == SensorLockType.ProbeID) {
                    response = new Text($"{chassisName} {variantName}");
                } else if (lockState.sensorType >= SensorLockType.SensorID) {
                    response = new Text($"{chassisName} {tonnage}t");
                } else if (lockState.visionType >= VisionLockType.Silhouette) {
                    response = new Text($"{chassisName}");
                } else {
                    response = new Text($"{type}");
                }
            } else {
                // HBS: ? only
                if (lockState.sensorType == SensorLockType.ProbeID) {
                    response = new Text($"{chassisName} {variantName}");
                } else if (lockState.sensorType >= SensorLockType.SensorID) {
                    response = new Text($"{chassisName} {tonnage}t");
                } else if (lockState.visionType >= VisionLockType.Silhouette) {
                    response = new Text($"{chassisName}");
                } else {
                    response = new Text($"?");
                }
            }

            return response;
        }
    }

    // --- HIDE UNIT NAME PATCHES ---
    [HarmonyPatch(typeof(Mech), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Mech_GetActorInfoFromVisLevel {

        public static void Postfix(Mech __instance, ref Text __result, VisibilityLevel visLevel) {
            //KnowYourFoe.Logger.Log("Mech:GetActorInfoFromVisLevel:post - entered.");
            if (__instance == null || State.roundDetectResults.Count == 0) { return; }

            /*
                Mech.UnitName = MechDef.Chassis.Description.Name -> Atlas / Trebuchet
                Mech.VariantName = MechDef.Chassis.VariantName -> AS7-D / TBT-5N
                Mech.NickName = MechDef.Description.Name -> Atlas II AS7-D-HT or Atlas AS7-D / Trebuchet
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastActiveActor(__instance.Combat), __instance);

                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.MechDef.Chassis.Tonnage;

                Text response = CombatNameHelper.GetDetectionLabel(visLevel, lockState, fullName, variantName, chassisName, "MECH", tonnage);
                LowVisibility.Logger.LogIfDebug($"Mech:GetActorInfoFromVisLevel:post - response:({response}) for " +
                    $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName})");
                __result = response;
            }
        }
    }

    [HarmonyPatch(typeof(Turret), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Turret_GetActorInfoFromVisLevel {
        public static void Postfix(Turret __instance, ref Text __result, VisibilityLevel visLevel) {
            //KnowYourFoe.Logger.Log("Turret:GetActorInfoFromVisLevel:post - entered.");
            if (__instance == null || State.roundDetectResults.Count == 0) { return; }

            /*
                Turret.UnitName = return (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Chassis.Description.Name ->

                Turret.VariantName = string.Empty -> ""
                Turret.NickName = (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Description.Name ->

            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastActiveActor(__instance.Combat), __instance);

                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.TurretDef.Chassis.Tonnage;

                Text response = CombatNameHelper.GetDetectionLabel(visLevel, lockState, fullName, variantName, chassisName, "TURRET", tonnage);
                LowVisibility.Logger.Log($"Turret:GetActorInfoFromVisLevel:post - response:({response}) for " +
                    $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName})");
                __result = response;
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Vehicle_GetActorInfoFromVisLevel {
        public static void Postfix(Vehicle __instance, ref Text __result, VisibilityLevel visLevel) {
            //KnowYourFoe.Logger.Log("Vehicle:GetActorInfoFromVisLevel:post - entered.");
            if (__instance == null || State.roundDetectResults.Count == 0) { return; };

            /*
                Vehicle.UnitName = VehicleDef.Chassis.Description.Name -> 
                    Alacorn Mk.VI-P / vehicledef_ARES_CLAN / Demolisher II / Galleon GAL-102
                Vehicle.VariantName = string.Empty -> ""
                Vehicle.NickName = VehicleDef.Description.Name -> 
                    Pirate Alacorn Gauss Carrier / Ares / Demolisher II
                    VehicleDef.Description.Id ->
                        / / vehicledef_DEMOLISHER-II / vehicledef_GALLEON_GAL102
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastActiveActor(__instance.Combat), __instance);

                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.VehicleDef.Chassis.Tonnage;

                Text response = CombatNameHelper.GetDetectionLabel(visLevel, lockState, fullName, variantName, chassisName, "VEHICLE", tonnage);
                LowVisibility.Logger.Log($"Vehicle:GetActorInfoFromVisLevel:post - response:({response}) for " +
                    $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName})");
                __result = response;
            }
        }
    }


    // --- HIDE COMPONENT PATCHES ---
    [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover), "OnPointerEnter")]
    public static class CombatHUDMechTrayArmorHover_OnPointerEnter {
        public static void Postfix(CombatHUDMechTrayArmorHover __instance) {
            HUDMechArmorReadout ___Readout = (HUDMechArmorReadout)Traverse.Create(__instance).Property("Readout").GetValue();
            CombatHUDTooltipHoverElement ___ToolTip = (CombatHUDTooltipHoverElement)Traverse.Create(__instance).Property("ToolTip").GetValue();

            //KnowYourFoe.Logger.Log($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - entered.");
            if (___Readout != null && ___Readout.DisplayedMech != null) {
                Mech target = ___Readout.DisplayedMech;
                bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
                if (!isPlayer) {
                    LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastActiveActor(target.Combat), target);
                    if (lockState.sensorType < SensorLockType.ProbeID) {
                        ___ToolTip.BuffStrings.Clear();
                    } else {
                        //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be shown for actor:{target.DisplayName}_{target.GetPilot().Name}");
                    }
                }

            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDVehicleArmorHover), "OnPointerEnter")]
    public static class CombatHUDVehicleArmorHover_OnPointerEnter {
        public static void Postfix(CombatHUDVehicleArmorHover __instance) {
            HUDVehicleArmorReadout ___Readout = (HUDVehicleArmorReadout)Traverse.Create(__instance).Property("Readout").GetValue();
            CombatHUDTooltipHoverElement ___ToolTip = (CombatHUDTooltipHoverElement)Traverse.Create(__instance).Property("ToolTip").GetValue();

            //KnowYourFoe.Logger.Log($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - entered.");
            if (___Readout != null && ___Readout.DisplayedVehicle != null) {
                Vehicle target = ___Readout.DisplayedVehicle;
                bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
                if (!isPlayer) {
                    LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastActiveActor(target.Combat), target);
                    if (lockState.sensorType < SensorLockType.ProbeID) {
                        //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be hidden for actor:{target.DisplayName}_{target.GetPilot().Name}");
                        ___ToolTip.BuffStrings.Clear();
                    } else {
                        //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be shown for actor:{target.DisplayName}_{target.GetPilot().Name}");
                    }
                }

            }
        }
    }
}
