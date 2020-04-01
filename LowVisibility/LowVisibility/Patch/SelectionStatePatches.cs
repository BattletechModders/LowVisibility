using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(SelectionStateFire), "ProcessClickedCombatant")]
    public static class SelectionStateFire_ProcessClickedCombatant {

        public static bool Prefix(SelectionStateFire __instance, ref bool __result, ICombatant combatant) {
            Mod.Log.Trace("SSF:PCC:PRE entered");

            if (__instance != null && combatant != null && combatant is AbstractActor targetActor && __instance.SelectedActor != null) {

                CombatGameState Combat = __instance.SelectedActor.Combat;
                bool targetIsFriendly = Combat.HostilityMatrix.IsFriendly(combatant.team.GUID, Combat.LocalPlayerTeamGuid);
                if (targetIsFriendly) {
                    Mod.Log.Trace("Friendly target, skipping check");
                    return true;
                }

                EWState targetState = new EWState(targetActor);
                EWState attackerState = new EWState(__instance.SelectedActor);

                if (__instance.SelectionType == SelectionType.FireMorale) {
                    // Prevents blips from being the targets of called shots
                    VisibilityLevel targetVisibility = __instance.SelectedActor.VisibilityToTargetUnit(targetActor);
                    if (targetVisibility < VisibilityLevel.LOSFull) {
                        Mod.Log.Info($"Target {CombatantUtils.Label(combatant)} is a blip, cannot be targeted by called shot");
                        __result = false;
                        return false;
                    }

                    float distance = Vector3.Distance(__instance.SelectedActor.CurrentPosition, targetActor.CurrentPosition);
                    bool hasVisualScan = VisualLockHelper.GetVisualScanRange(__instance.SelectedActor) >= distance;
                    SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, __instance.SelectedActor);
                    if (sensorScan < SensorScanType.ArmorAndWeaponType && !hasVisualScan) {
                        Mod.Log.Info($"Target {CombatantUtils.Label(targetActor)} sensor info {sensorScan} is less than SurfaceScan and range:{distance} outside visualScan range, cannot be targeted by called shot");
                        __result = false;
                        return false;
                    }
                } else if (__instance.SelectionType == SelectionType.FireMulti) {
                    if (targetState.HasStealth() || targetState.HasMimetic()) {
                        Mod.Log.Info($"Target {CombatantUtils.Label(targetActor)} has stealth, cannot be multi-targeted!");
                        __result = false;
                        return false;
                    }
                }
            }

            __result = false;
            return true;
        }
    }

}
