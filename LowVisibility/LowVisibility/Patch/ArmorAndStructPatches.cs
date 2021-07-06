using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Reflection;
using TMPro;
using us.frostraptor.modUtils;

namespace LowVisibility.Patches {

    public static class ArmorAndStructHelper {
        public static void ObfuscateArmorAndStructText(AbstractActor target, TextMeshProUGUI armorHover, TextMeshProUGUI structHover) {
            if (target == null) { Mod.Log.Warn?.Write("Helper::HideArmorAndStructure - target is null!");  }
            if (armorHover == null) { Mod.Log.Warn?.Write("Helper::HideArmorAndStructure - armorHover is null!"); }
            if (structHover == null) { Mod.Log.Warn?.Write("Helper::HideArmorAndStructure - structHover is null!"); }

            try
            {
                SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, ModState.LastPlayerActorActivated);
                bool hasVisualScan = VisualLockHelper.CanSpotTarget(
                    ModState.LastPlayerActorActivated, ModState.LastPlayerActorActivated.CurrentPosition,
                    target, target.CurrentPosition, target.CurrentRotation, target.Combat.LOS);

                string armorText;
                string structText;
                if (scanType >= SensorScanType.StructAndWeaponID)
                {
                    // See all values
                    armorText = armorHover.text;
                    structText = structHover.text;
                }
                else if (scanType >= SensorScanType.ArmorAndWeaponType || hasVisualScan)
                {
                    // See max armor, max struct                
                    string rawArmor = armorHover.text;
                    string maxArmor = rawArmor.Split('/')[1];

                    string rawStruct = structHover.text;
                    string maxStruct = rawStruct.Split('/')[1];

                    armorText = $"? / {maxArmor}";
                    structText = $"? / {maxStruct}";
                }
                else
                {
                    // See ? / ?
                    armorText = "? / ?";
                    structText = "? / ?";
                }

                // TODO: Sensor lock should give you an exact amount at the point you're locked
                armorHover.SetText(armorText);
                structHover.SetText(structText);
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Write(e, $"Failed to obfuscate armor and structure text for unit: {CombatantUtils.Label(target)}" +
                    $" from position of last-activated actor: {CombatantUtils.Label(ModState.LastPlayerActorActivated)}.");
            }


        }
    }

    [HarmonyPatch(typeof(HUDMechArmorReadout), "RefreshHoverInfo")]
    public static class HUDMechArmorReadout_RefreshHoverInfo {

        // Private method can't be patched by annotations, so use MethodInfo
        //public static MethodInfo TargetMethod() {
        //    return AccessTools.Method(typeof(HUDMechArmorReadout), "RefreshHoverInfo", new Type[] { });
        //}

        public static void Postfix(HUDMechArmorReadout __instance) {

            if (__instance != null && __instance.DisplayedMech != null && __instance.HoverInfoTextArmor != null && __instance.HoverInfoTextStructure != null) {
                if (!__instance.DisplayedMech.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.DisplayedMech.TeamId)) {
                    Mod.Log.Trace?.Write($"Hiding armor and structure on target: {CombatantUtils.Label(__instance.DisplayedMech)}");
                    ArmorAndStructHelper.ObfuscateArmorAndStructText(__instance.DisplayedMech, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
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

            if (__instance != null && __instance.DisplayedVehicle != null && __instance.HoverInfoTextArmor != null && __instance.HoverInfoTextStructure != null) {
                if (!__instance.DisplayedVehicle.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.DisplayedVehicle.TeamId)) {
                    ArmorAndStructHelper.ObfuscateArmorAndStructText(__instance.DisplayedVehicle, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
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
                if (!__instance.DisplayedTurret.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.DisplayedTurret.TeamId)) {
                    ArmorAndStructHelper.ObfuscateArmorAndStructText(__instance.DisplayedTurret, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
                }
            }
        }
    }

    public static class HUDTurretArmorReadout_UpdateArmorStructureBars {
        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(HUDTurretArmorReadout), "UpdateArmorStructureBars", new Type[] { });
        }

        public static void Postfix(HUDTurretArmorReadout __instance) {
            if (!__instance.DisplayedTurret.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.DisplayedTurret.TeamId)) {
                ArmorAndStructHelper.ObfuscateArmorAndStructText(__instance.DisplayedTurret, __instance.HoverInfoTextArmor, __instance.HoverInfoTextStructure);
            }
        }
    }


    // --- BUILDINGS ---
    //[HarmonyPatch()]
    //public static class HUDBuildingStructureReadout_ResetArmorStructureBars {
    //    // Private method can't be patched by annotations, so use MethodInfo
    //    public static MethodInfo TargetMethod() {
    //        return AccessTools.Method(typeof(HUDBuildingStructureReadout), "ResetArmorStructureBars", new Type[] { });
    //    }

    //    public static void Postfix(HUDBuildingStructureReadout __instance) {
    //        if (__instance.DisplayedBuilding != null && __instance.HoverInfoTextStructure != null) {
    //            // TODO: Handle sensor lock
    //            int maxStructure = HUDMechArmorReadout.FormatForSummary(__instance.DisplayedBuilding.SummaryStructureMax);
    //            __instance.HoverInfoTextStructure.SetText($"? / {maxStructure}");
    //        }
    //    }
    //}

    //[HarmonyPatch()]
    //public static class HUDBuildingStructureReadout_UpdateArmorStructureBars {
    //    // Private method can't be patched by annotations, so use MethodInfo
    //    public static MethodInfo TargetMethod() {
    //        return AccessTools.Method(typeof(HUDBuildingStructureReadout), "UpdateArmorStructureBars", new Type[] { });
    //    }

    //    public static void Postfix(HUDBuildingStructureReadout __instance) {
    //        if (__instance.DisplayedBuilding != null && __instance.HoverInfoTextStructure != null) {
    //            // TODO: Handle sensor lock
    //            int maxStructure = HUDMechArmorReadout.FormatForSummary(__instance.DisplayedBuilding.SummaryStructureMax);
    //            __instance.HoverInfoTextStructure.SetText($"? / {maxStructure}");
    //        }
    //    }
    //}

}
