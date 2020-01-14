using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch.HUD {

    [HarmonyPatch(typeof(CombatHUDWeaponSlot), "SetHitChance")]
    [HarmonyPatch(new Type[] {  typeof(ICombatant) })]
    public static class CombatHUDWeaponSlot_SetHitChance {
        public static void Prefix(CombatHUDWeaponSlot __instance, ICombatant target, CombatHUD ___HUD) {
            Mod.Log.Trace("CHUDWS:SHC - entered.");

            if (___HUD.SelectedActor == null || target == null) { return; }

            EWState attackerState = new EWState(___HUD.SelectedActor);
            Mod.Log.Debug($"Attacker ({CombatantUtils.Label(___HUD.SelectedActor)} => EWState: {attackerState}");
            bool canSpotTarget = VisualLockHelper.CanSpotTarget(___HUD.SelectedActor, ___HUD.SelectedActor.CurrentPosition, 
                target, target.CurrentPosition, target.CurrentRotation, ___HUD.SelectedActor.Combat.LOS);
            SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(target, ___HUD.SelectedActor);
            Mod.Log.Debug($"  canSpotTarget: {canSpotTarget}  sensorScan: {sensorScan}");

            if (target is AbstractActor targetActor) {
                EWState targetState = new EWState(targetActor);
                Mod.Log.Debug($"Target ({CombatantUtils.Label(targetActor)} => EWState: {targetState}");
            }
        }
    }

}
