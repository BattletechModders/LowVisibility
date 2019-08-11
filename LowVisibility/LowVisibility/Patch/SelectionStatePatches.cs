using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(SelectionStateFireMulti), "ProcessClickedCombatant")]
    public static class SelectionStateFireMulti_ProcessClickedCombatant {
        public static void Postfix(SelectionStateFireMulti __instance, ref bool __value, ICombatant actor) {
            Mod.Log.Trace("SSFM:PCC entered");

            // Prevents units with stealth or mimetic from being multi-targeted
            if (__instance != null && actor != null && actor is AbstractActor aActor) {
                EWState targetState = new EWState(aActor);
                if (targetState.HasStealth() || targetState.HasMimetic()) {
                    Mod.Log.Info($"Target {CombatantUtils.Label(actor)} has stealth, cannot be multi-targeted!");
                    __value = false;
                    __instance.BackOut();
                }
            }
        }
    }
    
    public static class SelectionStateFire_ProcessClickedCombatant {
        public static void Postfix(SelectionStateFireMulti __instance, ref bool __value, ICombatant actor) {
            Mod.Log.Trace("SSF:PCC entered");

            if (__instance != null && actor != null && actor is AbstractActor aActor && 
                __instance.SelectionType == SelectionType.FireMorale) {

                EWState targetState = new EWState(aActor);
                EWState attackerState = new EWState(__instance.SelectedActor);

                // Prevents blips from being the targets of called shots
                VisibilityLevel targetVisibility = __instance.SelectedActor.VisibilityToTargetUnit(actor);
                if (targetVisibility < VisibilityLevel.LOSFull) {
                    Mod.Log.Info($"Target {CombatantUtils.Label(actor)} is a blip, cannot be targeted by called shot");
                    __value = false;
                    __instance.BackOut();
                }

                SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(actor, __instance.SelectedActor);
                if (sensorScan < SensorScanType.SurfaceScan) {
                    Mod.Log.Info($"Target {CombatantUtils.Label(actor)} sensor info {sensorScan} is less than SurfaceScan, cannot be targeted by called shot");
                    __value = false;
                    __instance.BackOut();
                }
            }

        }
    }

}
