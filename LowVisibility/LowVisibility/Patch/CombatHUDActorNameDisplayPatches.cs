using BattleTech;
using BattleTech.UI;
using Harmony;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    static class CombatNameHelper {

        /*
            chassisName -> Mech.UnitName = MechDef.Chassis.Description.Name -> Atlas / Trebuchet
            variantName -> Mech.VariantName = MechDef.Chassis.VariantName -> AS7-D / TBT-5N
            fullname -> Mech.NickName = MechDef.Description.Name -> Atlas II AS7-D-HT or Atlas AS7-D / Trebuchet
        */
        public static Text GetDetectionLabel(ICombatant target, VisibilityLevel visLevel, 
            string fullName, string variantName, string chassisName, string type, float tonnage) {

            Text label = new Text("?");

            if (visLevel == VisibilityLevel.LOSFull) {
                List<Locks> allLocks = State.TeamLocksForTarget(target);
                AggregateLocks locks = AggregateLocks.Aggregate(allLocks);
                if (locks.sensorLock >= SensorScanType.DeepScan) {
                    label = new Text($"{fullName}");
                } else if (locks.sensorLock >= SensorScanType.SurfaceAnalysis|| locks.visualLock >= VisualScanType.VisualID) {
                    label = new Text($"{chassisName} {variantName} ({tonnage}t)");
                } else {
                    // Silhouette or better
                    label = new Text($"{chassisName} ?");
                }
            } else if (visLevel == VisibilityLevel.Blip4Maximum) {
                label = new Text($"{fullName}");
            } else if (visLevel == VisibilityLevel.Blip1Type) {
                label = new Text($"{chassisName} {variantName} ({tonnage}t)");
            } else if (visLevel == VisibilityLevel.Blip0Minimum) {
                label = new Text($"{chassisName}");
            } else if (visLevel == VisibilityLevel.BlobSmall) {
                label = new Text($"{type}");
            } else {
                label = new Text($"?");
            }

            LowVisibility.Logger.LogIfDebug($"GetDetectionLabel - label:({label}) for visLevel:{visLevel} " +
                $"chassisName:({chassisName}) variantName:({variantName}) fullName:({fullName}) type:({type}) tonnage:{tonnage}t");
            return label;
        }
    }

    // --- HIDE UNIT NAME PATCHES ---
    [HarmonyPatch(typeof(Mech), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Mech_GetActorInfoFromVisLevel {

        public static void Postfix(Mech __instance, ref Text __result, VisibilityLevel visLevel) {
            if (__instance == null || State.EWState.Count == 0) { return; }

            /*
                Mech.UnitName = MechDef.Chassis.Description.Name -> Atlas / Trebuchet
                Mech.VariantName = MechDef.Chassis.VariantName -> AS7-D / TBT-5N
                Mech.NickName = MechDef.Description.Name -> Atlas II AS7-D-HT or Atlas AS7-D / Trebuchet
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.MechDef.Chassis.Tonnage;

                Text response = CombatNameHelper.GetDetectionLabel(__instance, visLevel,fullName, variantName, chassisName, "MECH", tonnage);
                __result = response;
            }
        }
    }

    [HarmonyPatch(typeof(Turret), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Turret_GetActorInfoFromVisLevel {
        public static void Postfix(Turret __instance, ref Text __result, VisibilityLevel visLevel) {
            if (__instance == null || State.EWState.Count == 0) { return; }

            /*
                Turret.UnitName = return (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Chassis.Description.Name ->
                Turret.VariantName = string.Empty -> ""
                Turret.NickName = (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Description.Name ->
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.TurretDef.Chassis.Tonnage;

                Text response = CombatNameHelper.GetDetectionLabel(__instance, visLevel, fullName, variantName, chassisName, "TURRET", tonnage);
                __result = response;
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Vehicle_GetActorInfoFromVisLevel {
        public static void Postfix(Vehicle __instance, ref Text __result, VisibilityLevel visLevel) {
            if (__instance == null || State.EWState.Count == 0) { return; };

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
                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.VehicleDef.Chassis.Tonnage;

                Text response = CombatNameHelper.GetDetectionLabel(__instance, visLevel, fullName, variantName, chassisName, "VEHICLE", tonnage);
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
            if (___Readout != null && ___Readout.DisplayedMech != null && ___Readout.DisplayedMech.Combat != null && ___ToolTip != null) {
                Mech target = ___Readout.DisplayedMech;
                bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
                if (!isPlayer) {
                    Locks lockState = State.LastActivatedLocksForTarget(target);
                    if (lockState.sensorLock < SensorScanType.DeepScan) {
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
            if (___Readout != null && ___Readout.DisplayedVehicle != null && ___Readout.DisplayedVehicle.Combat != null && ___ToolTip != null) {
                Vehicle target = ___Readout.DisplayedVehicle;
                bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
                if (!isPlayer) {
                    Locks lockState = State.LastActivatedLocksForTarget(target);
                    if (lockState.sensorLock < SensorScanType.DeepScan) {
                        //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be hidden for actor:{target.DisplayName}_{target.GetPilot().Name}");
                        ___ToolTip.BuffStrings.Clear();
                    } else {
                        //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be shown for actor:{target.DisplayName}_{target.GetPilot().Name}");
                    }
                }
            }
        }
    }

    // -- HIDE PILOT NAME PATCHES --
    [HarmonyPatch(typeof(CombatHUDActorNameDisplay), "RefreshInfo")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) } )]
    public static class CombatHUDActorNameDisplay_RefreshInfo {

        public static void Postfix(CombatHUDActorNameDisplay __instance, VisibilityLevel visLevel, AbstractActor ___displayedActor) {
            if (___displayedActor != null && State.LastPlayerActor != null && State.TurnDirectorStarted &&
                (HostilityHelper.IsLocalPlayerEnemy(___displayedActor) || HostilityHelper.IsLocalPlayerNeutral(___displayedActor))) {
                Locks lockState = State.LastActivatedLocksForTarget(___displayedActor);
                if (lockState?.sensorLock < SensorScanType.DentalRecords) {
                    __instance.PilotNameText.SetText("Unidentified Pilot");
                } else if (lockState?.sensorLock >= SensorScanType.DentalRecords) {
                    __instance.PilotNameText.SetText(___displayedActor.GetPilot().Name);
                }
            }
        }
    }
}
