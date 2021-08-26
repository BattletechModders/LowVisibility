using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch.HUD {

    [HarmonyPatch(typeof(CombatHUDWeaponPanel), "RefreshDisplayedWeapons")]
    public static class CombatHUDWeaponPanel_RefreshDisplayedWeapons {
        public static void Prefix(CombatHUDWeaponPanel __instance, AbstractActor ___displayedActor) {

            if (__instance == null || ___displayedActor == null) { return; }
            Mod.Log.Trace?.Write("CHUDWP:RDW - entered.");

            Traverse targetT = Traverse.Create(__instance).Property("target");
            Traverse hoveredTargetT = Traverse.Create(__instance).Property("hoveredTarget");

            Traverse HUDT = Traverse.Create(__instance).Property("HUD");
            CombatHUD HUD = HUDT.GetValue<CombatHUD>();
            SelectionState activeState = HUD.SelectionHandler.ActiveState;
            ICombatant target;
            if (activeState != null && activeState is SelectionStateMove) {
                target = hoveredTargetT.GetValue<ICombatant>();
                if (target == null) { target = targetT.GetValue<ICombatant>(); }
            } else {
                target = targetT.GetValue<ICombatant>();
                if (target == null) { target = hoveredTargetT.GetValue<ICombatant>(); }
            }

            if (target == null) { return; }

            EWState attackerState = new EWState(___displayedActor);
            Mod.Log.Debug?.Write($"Attacker ({CombatantUtils.Label(___displayedActor)} => EWState: {attackerState}");
            bool canSpotTarget = VisualLockHelper.CanSpotTarget(___displayedActor, ___displayedActor.CurrentPosition,
                target, target.CurrentPosition, target.CurrentRotation, ___displayedActor.Combat.LOS);
            SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(target, ___displayedActor);
            Mod.Log.Debug?.Write($"  canSpotTarget: {canSpotTarget}  sensorScan: {sensorScan}");

            if (target is AbstractActor targetActor) {
                EWState targetState = new EWState(targetActor);
                Mod.Log.Debug?.Write($"Target ({CombatantUtils.Label(targetActor)} => EWState: {targetState}");
            }

        }
    }

}
