using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Reflection;
using TMPro;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patches {

    public static class Helper {
        public static void HideArmorAndStructure(AbstractActor target, TextMeshProUGUI armorHover, TextMeshProUGUI structHover) {
            
            LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastActiveActor(target.Combat), target);
            string armorText = null;
            string structText = null;
            if (lockState.sensorType == SensorLockType.ProbeID) {
                // See all values
                armorText = armorHover.text;
                structText = structHover.text;
            } else if (lockState.sensorType == SensorLockType.SensorID) {
                // See max armor, max struct                
                string rawArmor = armorHover.text;
                string maxArmor = rawArmor.Split('/')[1];

                string rawStruct = structHover.text;                
                string maxStruct = rawStruct.Split('/')[1];

                armorText = $"? / {maxArmor}";
                structText = $"? / {maxStruct}";
            } else if (lockState.visionType == VisionLockType.VisualID) {
                // See max armor, no struct
                armorText = armorHover.text;                
                string rawArmor = armorHover.text;
                string maxArmor = rawArmor.Split('/')[1];

                armorText = $"? / {maxArmor}";
                structText = $"? / ?";
            } else {
                // See ? / ?
                armorText = "? / ?";
                structText = "? / ?";
            }

            // TODO: Sensor lock should give you an exact amount at the point you're locked
            armorHover.SetText(armorText);
            structHover.SetText(structText);
        }
    }

    [HarmonyPatch()]
    public static class HUDMechArmorReadout_RefreshHoverInfo {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(HUDMechArmorReadout), "RefreshHoverInfo", new Type[] { });
        }

        public static void Postfix(HUDMechArmorReadout __instance) {
            //KnowYourFoe.Logger.Log("HUDMechArmorReadout:RefreshHoverInfo:post - entered.");

            if (__instance != null && __instance.DisplayedMech != null && __instance.HoverInfoTextArmor != null && __instance.HoverInfoTextStructure != null) {
                //KnowYourFoe.Logger.Log($"DisplayedMech:{__instance.DisplayedMech} team:{__instance.DisplayedMech.team} combat:{__instance.DisplayedMech.Combat} localPlayerTeam:{__instance.DisplayedMech.Combat.LocalPlayerTeam}");
                bool isPlayer = __instance?.DisplayedMech?.team == __instance?.DisplayedMech?.Combat?.LocalPlayerTeam;
                //KnowYourFoe.Logger.Log($"Source isPlayer:{isPlayer} for " +
                //$"actor:{__instance.DisplayedMech.DisplayName}_{__instance.DisplayedMech.GetPilot().Name}_{__instance.DisplayedMech.GUID}.");
                if (!isPlayer) {
                    Helper.HideArmorAndStructure(__instance.DisplayedMech, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
                }
            }
        }
    }

    [HarmonyPatch()]
    public static class HUDVehicleArmorReadout_RefreshHoverInfo {
        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(HUDVehicleArmorReadout), "RefreshHoverInfo", new Type[] { });
        }

        public static void Postfix(HUDVehicleArmorReadout __instance) {
            //KnowYourFoe.Logger.Log("HUDVehicleArmorReadout:RefreshHoverInfo:post - entered.");

            if (__instance != null && __instance.DisplayedVehicle != null && __instance.HoverInfoTextArmor != null && __instance.HoverInfoTextStructure != null) {
                //KnowYourFoe.Logger.Log($"DisplayedMech:{__instance.DisplayedMech} team:{__instance.DisplayedMech.team} combat:{__instance.DisplayedMech.Combat} localPlayerTeam:{__instance.DisplayedMech.Combat.LocalPlayerTeam}");
                bool isPlayer = __instance?.DisplayedVehicle?.team == __instance?.DisplayedVehicle?.Combat?.LocalPlayerTeam;
                //KnowYourFoe.Logger.Log($"Source isPlayer:{isPlayer} for " +
                //$"actor:{__instance.DisplayedMech.DisplayName}_{__instance.DisplayedMech.GetPilot().Name}_{__instance.DisplayedMech.GUID}.");
                if (!isPlayer) {
                    Helper.HideArmorAndStructure(__instance.DisplayedVehicle, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
                }
            }
        }

    }

    // --- TURRETS ---
    [HarmonyPatch()]
    public static class HUDTurretArmorReadout_ResetArmorStructureBars {
        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(HUDTurretArmorReadout), "ResetArmorStructureBars", new Type[] { });
        }

        public static void Postfix(HUDTurretArmorReadout __instance) {
            if (__instance.DisplayedTurret != null && __instance.HoverInfoTextArmor != null && __instance.HoverInfoTextStructure != null) {
                Helper.HideArmorAndStructure(__instance.DisplayedTurret, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
            }
        }
    }

    public static class HUDTurretArmorReadout_UpdateArmorStructureBars {
        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(HUDTurretArmorReadout), "UpdateArmorStructureBars", new Type[] { });
        }

        public static void Postfix(HUDTurretArmorReadout __instance) {
            if (__instance.DisplayedTurret != null && __instance.HoverInfoTextArmor != null && __instance.HoverInfoTextStructure != null) {
                Helper.HideArmorAndStructure(__instance.DisplayedTurret, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
            }
        }
    }


    // --- BUILDINGS ---
    [HarmonyPatch()]
    public static class HUDBuildingStructureReadout_ResetArmorStructureBars {
        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(HUDBuildingStructureReadout), "ResetArmorStructureBars", new Type[] { });
        }

        public static void Postfix(HUDBuildingStructureReadout __instance) {
            if (__instance.DisplayedBuilding != null && __instance.HoverInfoTextStructure != null) {
                // TODO: Handle sensor lock
                int maxStructure = HUDMechArmorReadout.FormatForSummary(__instance.DisplayedBuilding.SummaryStructureMax);
                __instance.HoverInfoTextStructure.SetText($"? / {maxStructure}");
            }
        }
    }

    [HarmonyPatch()]
    public static class HUDBuildingStructureReadout_UpdateArmorStructureBars {
        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(HUDBuildingStructureReadout), "UpdateArmorStructureBars", new Type[] { });
        }

        public static void Postfix(HUDBuildingStructureReadout __instance) {
            if (__instance.DisplayedBuilding != null && __instance.HoverInfoTextStructure != null) {
                // TODO: Handle sensor lock
                int maxStructure = HUDMechArmorReadout.FormatForSummary(__instance.DisplayedBuilding.SummaryStructureMax);
                __instance.HoverInfoTextStructure.SetText($"? / {maxStructure}");
            }
        }
    }

}
