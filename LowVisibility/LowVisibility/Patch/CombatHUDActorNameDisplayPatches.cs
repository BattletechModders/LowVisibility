using BattleTech;
using Harmony;
using Localize;
using System;
using System.Linq;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;
using static LowVisibility.Patch.CombatNameHelper;

namespace LowVisibility.Patch {

    static class CombatNameHelper {

        public enum IDState {
            None,
            Silhouette,
            VisualID,
            SensorID,
            ProbeID
        }

        public const float VisualIDRange = 3.0f * 30;

        /*
            Mech.UnitName = Atlas
            Mech.VariantName = AS7-D
            Mech.NickName = Atlas II AS7-D-HT or Atlas AS7-D
        */
        public static Text GetDetectionLabel(VisibilityLevel visLevel, IDState idState,
            string fullName, string variantName, string chassisName, string type, float tonnage) {

            Text response = new Text("?");

            if (visLevel == VisibilityLevel.LOSFull) {
                // HBS: Full details
                if (idState == IDState.ProbeID) {
                    response = new Text($"{chassisName} {variantName}");
                } else if (idState >= IDState.Silhouette) {
                    response = new Text($"{chassisName} {tonnage}t");
                } else {
                    response = new Text($"{type}");
                }

            } else if (visLevel >= VisibilityLevel.Blip1Type) {
                // HBS: Type only
                if (idState == IDState.ProbeID) {
                    response = new Text($"{chassisName} {variantName}");
                } else if (idState >= IDState.SensorID) {
                    response = new Text($"{chassisName} {tonnage}t");
                } else {
                    response = new Text($"{type}");
                }
            } else {
                // HBS: ? only
                if (idState == IDState.ProbeID) {
                    response = new Text($"{chassisName} {variantName}");
                } else if (idState >= IDState.SensorID) {
                    response = new Text($"{chassisName} {tonnage}t");
                } else {
                    response = new Text($"?");
                }
            }

            return response;
        }
    }

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
                IDState idState = IDState.None;
                foreach (AbstractActor actor in __instance.Combat.LocalPlayerTeam.units) {
                    ActorEWConfig ewConfig = State.GetOrCreateActorEWConfig(actor);
                    RoundDetectRange roundDetect = State.GetOrCreateRoundDetectResults(actor);
                    VisibilityLevelAndAttribution visLevelAndAttrib = actor.VisibilityCache.VisibilityToTarget(__instance);
                    
                    // Check for visibility 
                    if (visLevelAndAttrib.VisibilityLevel == VisibilityLevel.LOSFull) {
                        if (idState < IDState.Silhouette) { idState = IDState.Silhouette;  }
                    }

                    // Check for visual ID
                    float distance = Vector3.Distance(actor.CurrentPosition, __instance.CurrentPosition);
                    LowVisibility.Logger.Log($"actor:{__instance.DisplayName}_{__instance.GetPilot().Name} is distance:{distance} from target:{__instance.DisplayName}_{__instance.GetPilot().Name}");
                    if (distance <= VisualIDRange && idState < IDState.VisualID) { idState = IDState.VisualID;  }

                    // Check for sensors
                    float sensorsRange = ewConfig.sensorsRange * (int)roundDetect;
                    LowVisibility.Logger.Log($"actor:{__instance.DisplayName}_{__instance.GetPilot().Name} has sensorsRange:{sensorsRange} vs distance:{distance}");
                    if (distance <= sensorsRange && idState < IDState.SensorID) { idState = IDState.SensorID; }

                    // Check for probes
                    if (ewConfig.probeTier >= 0) {
                        float probeRange = (ewConfig.sensorsRange + ewConfig.probeRange) * (int)roundDetect;
                        LowVisibility.Logger.Log($"actor:{__instance.DisplayName}_{__instance.GetPilot().Name} has probeRange:{probeRange} vs distance:{distance}");
                        if (distance <= probeRange && idState < IDState.ProbeID) { idState = IDState.ProbeID; }
                    }
                    LowVisibility.Logger.Log($"actor:{__instance.DisplayName}_{__instance.GetPilot().Name} has IDstate:{idState}.");
                }                

                string chassisName = __instance.UnitName;
                string variantName = __instance.VariantName;
                string fullName = __instance.Nickname;
                float tonnage = __instance.MechDef.Chassis.Tonnage;
                Text response = CombatNameHelper.GetDetectionLabel(visLevel, idState, fullName, variantName, chassisName, "MECH", tonnage);
                LowVisibility.Logger.LogIfDebug($"Mech:GetActorInfoFromVisLevel:post - response:({response}) for " +
                    $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName})");
                __result = response;
            }
        }
    }

    //[HarmonyPatch(typeof(Turret), "GetActorInfoFromVisLevel")]
    //[HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    //public static class Turret_GetActorInfoFromVisLevel {
    //    public static void Postfix(Turret __instance, ref Text __result, VisibilityLevel visLevel) {
    //        //KnowYourFoe.Logger.Log("Turret:GetActorInfoFromVisLevel:post - entered.");
    //        if (State.ActorDetectState.Count == 0) { return; }

    //        /*
    //            Turret.UnitName = return (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Chassis.Description.Name ->
                
    //            Turret.VariantName = string.Empty -> ""
    //            Turret.NickName = (this.TurretDef == null) ? "UNDEFINED" : this.TurretDef.Description.Name ->
                    
    //        */
    //        if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
    //            DetectLevel detectState = State.ActorDetectState[__instance.GUID].detectLevel;
    //            LowVisibility.Logger.Log($"actor:{__instance.DisplayName}_{__instance.GetPilot().Name} has detectState:{detectState}.");

    //            string chassisName = __instance.UnitName;
    //            string variantName = __instance.VariantName;
    //            string fullName = __instance.Nickname;
    //            float tonnage = __instance.TurretDef.Chassis.Tonnage;
    //            Text response = CombatNameHelper.GetDetectionLabel(visLevel, detectState, fullName, variantName, chassisName, "TURRET", tonnage);
    //            LowVisibility.Logger.Log($"Turret:GetActorInfoFromVisLevel:post - response:({response}) for " +
    //                $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName})");
    //            __result = response;
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(Vehicle), "GetActorInfoFromVisLevel")]
    //[HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    //public static class Vehicle_GetActorInfoFromVisLevel {
    //    public static void Postfix(Vehicle __instance, ref Text __result, VisibilityLevel visLevel) {
    //        //KnowYourFoe.Logger.Log("Vehicle:GetActorInfoFromVisLevel:post - entered.");
    //        if (State.ActorDetectState.Count == 0) { return; }

    //        /*
    //            Vehicle.UnitName = VehicleDef.Chassis.Description.Name -> 
    //                Alacorn Mk.VI-P / vehicledef_ARES_CLAN / Demolisher II / Galleon GAL-102
    //            Vehicle.VariantName = string.Empty -> ""
    //            Vehicle.NickName = VehicleDef.Description.Name -> 
    //                Pirate Alacorn Gauss Carrier / Ares / Demolisher II
    //                VehicleDef.Description.Id ->
    //                    / / vehicledef_DEMOLISHER-II / vehicledef_GALLEON_GAL102
    //        */
    //        if (__instance.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.team.GUID)) {
    //            DetectLevel detectState = State.ActorDetectState[__instance.GUID].detectLevel;
    //            LowVisibility.Logger.Log($"actor:{__instance.DisplayName}_{__instance.GetPilot().Name} has detectState:{detectState}.");

    //            string chassisName = __instance.UnitName;
    //            string variantName = __instance.VariantName;
    //            string fullName = __instance.Nickname;
    //            float tonnage = __instance.VehicleDef.Chassis.Tonnage;
    //            Text response = CombatNameHelper.GetDetectionLabel(visLevel, detectState, fullName, variantName, chassisName, "VEHICLE", tonnage);
    //            LowVisibility.Logger.Log($"Vehicle:GetActorInfoFromVisLevel:post - response:({response}) for " +
    //                $"fullName:({__instance.Nickname}), variantName:({__instance.VariantName}), unitName:({__instance.UnitName})");
    //            __result = response;
    //        }
    //    }
    //}

    //[HarmonyPatch()]
    //public static class CombatHUDActorNameDisplay_RefreshInfo {
    //    // Private method can't be patched by annotations, so use MethodInfo
    //    public static MethodInfo TargetMethod() {
    //        return AccessTools.Method(typeof(CombatHUDMechCallout), "RefreshInfo", new Type[] { });
    //    }

    //    public static void Postfix(CombatHUDMechCallout __instance) {
    //        KnowYourFoe.Logger.Log($"Got handle to a CombatHUDMechCallout: {__instance?.name} hash:{__instance.GetHashCode()}");
    //    }
    //}

    // Hides components if you shouldn't see them
    // TODO: Vehicles
    //[HarmonyPatch(typeof(CombatHUDMechTrayArmorHover), "OnPointerEnter")]
    //public static class CombatHUDMechTrayArmorHover_OnPointerEnter {
    //    public static void Postfix(CombatHUDMechTrayArmorHover __instance) {
    //        HUDMechArmorReadout ___Readout = (HUDMechArmorReadout)Traverse.Create(__instance).Property("Readout").GetValue();
    //        CombatHUDTooltipHoverElement ___ToolTip = (CombatHUDTooltipHoverElement)Traverse.Create(__instance).Property("ToolTip").GetValue();

    //        //KnowYourFoe.Logger.Log($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - entered.");
    //        if (___Readout != null && ___Readout.DisplayedMech != null) {
    //            Mech target = ___Readout.DisplayedMech;
    //            bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
    //            if (!isPlayer) {
    //                DetectState detectState = State.GetOrCreateDetectState(target);
    //                if (detectState.detectLevel < DetectLevel.Components) {
    //                    //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be hidden for actor:{target.DisplayName}_{target.GetPilot().Name}");
    //                    ___ToolTip.BuffStrings.Clear();
    //                } else {
    //                    //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be shown for actor:{target.DisplayName}_{target.GetPilot().Name}");
    //                }
    //            }

    //        }
    //    }
    //}

    //// TODO: Vehicles
    //[HarmonyPatch(typeof(CombatHUDVehicleArmorHover), "OnPointerEnter")]
    //public static class CombatHUDVehicleArmorHover_OnPointerEnter {
    //    public static void Postfix(CombatHUDVehicleArmorHover __instance) {
    //        HUDVehicleArmorReadout ___Readout = (HUDVehicleArmorReadout)Traverse.Create(__instance).Property("Readout").GetValue();
    //        CombatHUDTooltipHoverElement ___ToolTip = (CombatHUDTooltipHoverElement)Traverse.Create(__instance).Property("ToolTip").GetValue();

    //        //KnowYourFoe.Logger.Log($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - entered.");
    //        if (___Readout != null && ___Readout.DisplayedVehicle != null) {
    //            Vehicle target = ___Readout.DisplayedVehicle;
    //            bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
    //            if (!isPlayer) {
    //                DetectState detectState = State.GetOrCreateDetectState(target);
    //                if (detectState.detectLevel < DetectLevel.Components) {
    //                    //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be hidden for actor:{target.DisplayName}_{target.GetPilot().Name}");
    //                    ___ToolTip.BuffStrings.Clear();
    //                } else {
    //                    //KnowYourFoe.Logger.LogIfDebug($"CombatHUDMechTrayArmorHover:OnPointerEnter:post - components should be shown for actor:{target.DisplayName}_{target.GetPilot().Name}");
    //                }
    //            }

    //        }
    //    }
    //}
}
