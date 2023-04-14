using BattleTech.UI;
using LowVisibility.Helper;
using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch.HUD
{

    [HarmonyPatch(typeof(CombatHUDWeaponPanel), "RefreshDisplayedWeapons")]
    public static class CombatHUDWeaponPanel_RefreshDisplayedWeapons
    {
        public static void Prefix(ref bool __runOriginal, CombatHUDWeaponPanel __instance, AbstractActor ___displayedActor)
        {

            if (!__runOriginal) return;

            if (__instance == null || ___displayedActor == null) { return; }
            Mod.Log.Trace?.Write("CHUDWP:RDW - entered.");


            CombatHUD HUD = __instance.HUD;
            SelectionState activeState = HUD.SelectionHandler.ActiveState;
            ICombatant target;
            if (activeState != null && activeState is SelectionStateMove)
            {
                target = __instance.hoveredTarget;
                if (target == null) { target = __instance.target; }
            }
            else
            {
                target = __instance.target;
                if (target == null) { target = __instance.hoveredTarget; }
            }

            if (target == null) { return; }

            EWState attackerState = new EWState(___displayedActor);
            Mod.Log.Debug?.Write($"Attacker ({CombatantUtils.Label(___displayedActor)} => EWState: {attackerState}");
            bool canSpotTarget = VisualLockHelper.CanSpotTarget(___displayedActor, ___displayedActor.CurrentPosition,
                target, target.CurrentPosition, target.CurrentRotation, ___displayedActor.Combat.LOS);
            SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(target, ___displayedActor);
            Mod.Log.Debug?.Write($"  canSpotTarget: {canSpotTarget}  sensorScan: {sensorScan}");

            if (target is AbstractActor targetActor)
            {
                EWState targetState = new EWState(targetActor);
                Mod.Log.Debug?.Write($"Target ({CombatantUtils.Label(targetActor)} => EWState: {targetState}");
            }

        }
    }

}
