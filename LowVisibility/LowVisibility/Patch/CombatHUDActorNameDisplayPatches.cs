using BattleTech;
using BattleTech.UI;
using Harmony;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    static class CombatNameHelper {

        /*
            chassisName -> Mech.UnitName = MechDef.Chassis.Description.Name -> Atlas / Trebuchet
            variantName -> Mech.VariantName = MechDef.Chassis.VariantName -> AS7-D / TBT-5N
            fullname -> Mech.NickName = MechDef.Description.Name -> Atlas II AS7-D-HT or Atlas AS7-D / Trebuchet
        */
        public static Text GetDetectionLabel(VisibilityLevel visLevel, LockState lockState, VisibilityLevel blipLevel,
            string fullName, string variantName, string chassisName, string type, float tonnage) {

            Text response = new Text("?");

            if (visLevel == VisibilityLevel.LOSFull) {
                // HBS: Full details
                if (lockState.sensorLockLevel >= DetectionLevel.StructureAnalysis) {
                    response = new Text($"{fullName}");
                } else if (lockState.sensorLockLevel >= DetectionLevel.WeaponAnalysis) {
                    response = new Text($"{chassisName} {variantName}");
                } else if (lockState.sensorLockLevel >= DetectionLevel.Silhouette) {
                    response = new Text($"{chassisName} {tonnage}");
                } else {
                    response = new Text($"{chassisName}");
                }
            } else if (visLevel >= VisibilityLevel.Blip4Maximum) {
                // HBS: Type only
                // US: DetectionLevel.Silhouette
                response = new Text($"{chassisName} - {tonnage}t");
            } else if (visLevel >= VisibilityLevel.Blip1Type) {
                // US: DetectionLevel.Type
                response = new Text($"{type} - {tonnage}t");
            } else if (visLevel >= VisibilityLevel.Blip0Minimum) {
                // US: DetectionLevel.Location
                response = new Text($"???");
            } else {
                // HBS: ? only
                // US: Nothing
                response = new Text($"?");                
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
            if (__instance == null || State.DynamicEWState.Count == 0) { return; }

            /*
                Mech.UnitName = MechDef.Chassis.Description.Name -> Atlas / Trebuchet
                Mech.VariantName = MechDef.Chassis.VariantName -> AS7-D / TBT-5N
                Mech.NickName = MechDef.Description.Name -> Atlas II AS7-D-HT or Atlas AS7-D / Trebuchet
            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                LockState lockState = GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(__instance.Combat), __instance);

                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.MechDef.Chassis.Tonnage;

                VisibilityLevel blipLevel = ActorHelper.VisibilityLevelByTactics(__instance.GetPilot().Tactics);
                Text response = CombatNameHelper.GetDetectionLabel(visLevel, lockState, blipLevel, fullName, variantName, chassisName, "MECH", tonnage);
                LowVisibility.Logger.LogIfDebug($"Mech:GetActorInfoFromVisLevel:post - response:({response}) for " +
                    $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName}) " +
                    $"for visLevel:{visLevel} and lockState:{lockState}");
                __result = response;
            }
        }
    }

    [HarmonyPatch(typeof(Turret), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Turret_GetActorInfoFromVisLevel {
        public static void Postfix(Turret __instance, ref Text __result, VisibilityLevel visLevel) {
            //KnowYourFoe.Logger.Log("Turret:GetActorInfoFromVisLevel:post - entered.");
            if (__instance == null || State.DynamicEWState.Count == 0) { return; }

            /*
                Turret.UnitName = return (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Chassis.Description.Name ->

                Turret.VariantName = string.Empty -> ""
                Turret.NickName = (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Description.Name ->

            */
            if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
                LockState lockState = GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(__instance.Combat), __instance);

                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.TurretDef.Chassis.Tonnage;

                VisibilityLevel blipLevel = ActorHelper.VisibilityLevelByTactics(__instance.GetPilot().Tactics);
                Text response = CombatNameHelper.GetDetectionLabel(visLevel, lockState, blipLevel, fullName, variantName, chassisName, "TURRET", tonnage);
                LowVisibility.Logger.Log($"Turret:GetActorInfoFromVisLevel:post - response:({response}) for " +
                    $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName}) " +
                    $"for visLevel:{visLevel} and lockState:{lockState}");
                __result = response;
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "GetActorInfoFromVisLevel")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class Vehicle_GetActorInfoFromVisLevel {
        public static void Postfix(Vehicle __instance, ref Text __result, VisibilityLevel visLevel) {
            //KnowYourFoe.Logger.Log("Vehicle:GetActorInfoFromVisLevel:post - entered.");
            if (__instance == null || State.DynamicEWState.Count == 0) { return; };

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
                LockState lockState = GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(__instance.Combat), __instance);

                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.VehicleDef.Chassis.Tonnage;

                VisibilityLevel blipLevel = ActorHelper.VisibilityLevelByTactics(__instance.GetPilot().Tactics);
                Text response = CombatNameHelper.GetDetectionLabel(visLevel, lockState, blipLevel, fullName, variantName, chassisName, "VEHICLE", tonnage);
                LowVisibility.Logger.Log($"Vehicle:GetActorInfoFromVisLevel:post - response:({response}) for " +
                    $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName}) " +
                    $"for visLevel:{visLevel} and lockState:{lockState}");
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
                    LockState lockState = GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(target.Combat), target);
                    if (lockState.sensorLockLevel < DetectionLevel.DeepScan) {
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
                    LockState lockState = GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(target.Combat), target);
                    if (lockState.sensorLockLevel < DetectionLevel.DeepScan) {
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
            if (___displayedActor != null && 
                (HostilityHelper.IsLocalPlayerEnemy(___displayedActor) || HostilityHelper.IsLocalPlayerNeutral(___displayedActor))) {
                LockState lockState = State.GetLockStateForLastActivatedAgainstTarget(___displayedActor);
                if (lockState != null && lockState.sensorLockLevel < DetectionLevel.DentalRecords) {
                    __instance.PilotNameText.SetText("Unidentified Pilot");
                }
            }
        }
    }
}
