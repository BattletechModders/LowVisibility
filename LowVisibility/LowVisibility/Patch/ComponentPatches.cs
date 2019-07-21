using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;

namespace LowVisibility.Patch {
    // --- HIDE COMPONENT PATCHES ---
    [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover), "OnPointerEnter")]
    public static class CombatHUDMechTrayArmorHover_OnPointerEnter {
        public static void Postfix(CombatHUDMechTrayArmorHover __instance) {
            HUDMechArmorReadout ___Readout = (HUDMechArmorReadout)Traverse.Create(__instance).Property("Readout").GetValue();
            CombatHUDTooltipHoverElement ___ToolTip = (CombatHUDTooltipHoverElement)Traverse.Create(__instance).Property("ToolTip").GetValue();

            if (___Readout != null && ___Readout.DisplayedMech != null && ___Readout.DisplayedMech.Combat != null && ___ToolTip != null) {
                Mech target = ___Readout.DisplayedMech;
                if (!target.Combat.HostilityMatrix.IsLocalPlayerFriendly(target.TeamId)) {
                    SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, State.LastPlayerActorActivated);
                    if (scanType < SensorScanType.DeepScan) {
                        ___ToolTip.BuffStrings.Clear();
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

            if (___Readout != null && ___Readout.DisplayedVehicle != null && ___Readout.DisplayedVehicle.Combat != null && ___ToolTip != null) {
                Vehicle target = ___Readout.DisplayedVehicle;
                if (!target.Combat.HostilityMatrix.IsLocalPlayerFriendly(target.TeamId)) {

                    SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, State.LastPlayerActorActivated);
                    if (scanType < SensorScanType.DeepScan) {
                        ___ToolTip.BuffStrings.Clear();
                    }
                }
            }
        }
    }

    // -- HIDE PILOT NAME PATCHES --
    [HarmonyPatch(typeof(CombatHUDActorNameDisplay), "RefreshInfo")]
    [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
    public static class CombatHUDActorNameDisplay_RefreshInfo {

        public static void Postfix(CombatHUDActorNameDisplay __instance, VisibilityLevel visLevel, AbstractActor ___displayedActor) {
            if (___displayedActor != null && State.LastPlayerActorActivated != null && State.TurnDirectorStarted
                && !___displayedActor.Combat.HostilityMatrix.IsLocalPlayerFriendly(___displayedActor.TeamId)) {

                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(___displayedActor, State.LastPlayerActorActivated);

                if (scanType < SensorScanType.DentalRecords) {
                    __instance.PilotNameText.SetText("Unidentified Pilot");
                } else if (scanType >= SensorScanType.DentalRecords) {
                    __instance.PilotNameText.SetText(___displayedActor.GetPilot().Name);
                }
            }
        }
    }
}
