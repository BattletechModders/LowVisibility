using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {
    [HarmonyPatch(typeof(AttackEvaluator), "MakeCalledShotOrder")]
    public static class AttackEvaluator_MakeCalledShotOrder {

        public static void Postfix(ref CalledShotAttackOrderInfo __result, AbstractActor attackingUnit, int enemyUnitIndex) {
            Mod.Log.Trace("AE:CCSLTC entered");

            ICombatant combatant = attackingUnit.BehaviorTree.enemyUnits[enemyUnitIndex];
            if (combatant is AbstractActor targetActor) {
                EWState attackerState = new EWState(attackingUnit);
                EWState targetState = new EWState(targetActor);

                // Prevents blips from being the targets of called shots
                VisibilityLevel targetVisibility = attackingUnit.VisibilityToTargetUnit(targetActor);
                if (targetVisibility < VisibilityLevel.LOSFull) {
                    Mod.Log.Info($"Target {CombatantUtils.Label(combatant)} is a blip, cannot be targeted by AI called shot");
                    __result = null;
                    return;
                }

                SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, attackingUnit);
                if (sensorScan < SensorScanType.SurfaceScan) {
                    Mod.Log.Info($"Target {CombatantUtils.Label(targetActor)} sensor info {sensorScan} is less than SurfaceScan, cannot be targeted by AI called shot");
                    __result = null;
                    return;
                }

            }
            
        }
    }
}
