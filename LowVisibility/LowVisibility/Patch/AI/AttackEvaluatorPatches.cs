using LowVisibility.Helper;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{
    [HarmonyPatch(typeof(AttackEvaluator), "MakeCalledShotOrder")]
    public static class AttackEvaluator_MakeCalledShotOrder
    {

        public static void Postfix(ref CalledShotAttackOrderInfo __result, AbstractActor attackingUnit, int enemyUnitIndex)
        {
            Mod.Log.Trace?.Write("AE:CCSLTC entered");

            ICombatant combatant = attackingUnit.BehaviorTree.enemyUnits[enemyUnitIndex];
            if (combatant is AbstractActor targetActor)
            {
                // Prevents blips from being the targets of called shots
                VisibilityLevel targetVisibility = attackingUnit.VisibilityToTargetUnit(targetActor);
                if (targetVisibility < VisibilityLevel.LOSFull)
                {
                    Mod.Log.Info?.Write($"Target {CombatantUtils.Label(combatant)} is a blip, cannot be targeted by AI called shot");
                    __result = null;
                    return;
                }

                float distance = Vector3.Distance(attackingUnit.CurrentPosition, targetActor.CurrentPosition);
                bool hasVisualScan = VisualLockHelper.GetVisualScanRange(attackingUnit) >= distance;
                SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, attackingUnit);
                if (sensorScan < SensorScanType.ArmorAndWeaponType && !hasVisualScan)
                {
                    Mod.Log.Info?.Write($"Target {CombatantUtils.Label(targetActor)} sensor info {sensorScan} is less than SurfaceScan and outside visualID, cannot be targeted by AI called shot");
                    __result = null;
                    return;
                }

            }

        }
    }
}
