using BattleTech;
using Harmony;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
    public static class ToHit_GetAllModifiers {
        private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            LockState lockState = State.GetUnifiedLockStateForTarget(attacker, target as AbstractActor);
            if (lockState.sensorType == SensorLockType.None) {
                LowVisibility.Logger.LogIfDebug($"Attacker:{ActorLabel(attacker)} has no sensor lock to target:{ActorLabel(target as AbstractActor)} " +
                    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                __result = __result + (float)LowVisibility.Config.NoSensorLockAttackPenalty;
            }
            if (lockState.visionType == VisionLockType.None) {
                LowVisibility.Logger.LogIfDebug($"Attacker:{ActorLabel(attacker)} has no visual lock to target:{ActorLabel(target as AbstractActor)} " +
                    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                __result = __result + (float)LowVisibility.Config.NoVisualLockAttackPenalty;
            }

            ActorEWConfig targetEWConfig = State.GetOrCreateActorEWConfig(target as AbstractActor);
            if (targetEWConfig.HasStealthRangeMod()) {
                float distance = Vector3.Distance(attackPosition, targetPosition);
                if (distance <= weapon.MaxRange && distance >= weapon.LongRange && targetEWConfig.stealthRangeMod[2] != 0) {
                    __result = __result + (float)targetEWConfig.stealthRangeMod[2];
                } else if (distance < weapon.LongRange && distance >= weapon.MediumRange && targetEWConfig.stealthRangeMod[1] != 0) {
                    __result = __result + (float)targetEWConfig.stealthRangeMod[1];
                } else if (distance < weapon.MediumRange && distance >= weapon.ShortRange && targetEWConfig.stealthRangeMod[0] != 0) {
                    __result = __result + (float)targetEWConfig.stealthRangeMod[0];
                }
            }
        }
    }

    //public string GetAllModifiersDescription(AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
    [HarmonyPatch(typeof(ToHit), "GetAllModifiersDescription")]
    public static class ToHit_GetAllModifiersDescription {
        private static void Postfix(ToHit __instance, ref string __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            LockState lockState = State.GetUnifiedLockStateForTarget(attacker, target as AbstractActor);
            if (lockState.sensorType == SensorLockType.None) {
                __result = string.Format("{0}NO SENSOR LOCK {1:+#;-#}; ", __result, LowVisibility.Config.NoSensorLockAttackPenalty);
            }
            if (lockState.visionType == VisionLockType.None) {
                __result = string.Format("{0}NO VISUAL LOCK {1:+#;-#}; ", __result, LowVisibility.Config.NoVisualLockAttackPenalty);
            }

            ActorEWConfig targetEWConfig = State.GetOrCreateActorEWConfig(target as AbstractActor);
            if (targetEWConfig.HasStealthRangeMod()) {
                float distance = Vector3.Distance(attackPosition, targetPosition);
                if (distance <= weapon.MaxRange && distance >= weapon.LongRange && targetEWConfig.stealthRangeMod[2] != 0) {
                    __result = string.Format("{0}STEALTH - LONG RANGE{1:+#;-#}; ", __result, targetEWConfig.stealthRangeMod[2]);
                } else if (distance < weapon.LongRange && distance >= weapon.MediumRange && targetEWConfig.stealthRangeMod[1] != 0) {
                    __result = string.Format("{0}STEALTH - MEDIUM RANGE{1:+#;-#}; ", __result, targetEWConfig.stealthRangeMod[1]);
                } else if (distance < weapon.MediumRange && distance >= weapon.ShortRange && targetEWConfig.stealthRangeMod[0] != 0) {
                    __result = string.Format("{0}STEALTH - SHORT RANGE{1:+#;-#}; ", __result, targetEWConfig.stealthRangeMod[0]);
                }
            }
        }
    }
}
