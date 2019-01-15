using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using UnityEngine;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
    public static class ToHit_GetAllModifiers {
        private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            LowVisibility.Logger.LogIfTrace($"Getting modifiers for attacker:{CombatantHelper.Label(attacker)} " +
                $"using weapon:{weapon.Name} vs target:{CombatantHelper.Label(target)}");

            if (__instance != null && attacker != null && target != null) {
                LockState lockState = GetUnifiedLockStateForTarget(attacker, target as AbstractActor);
                if (lockState == null) { LowVisibility.Logger.Log($"WARNING: LOCKSTATE NULL FOR TARGET:{CombatantHelper.Label(target)}"); }
                if (lockState.sensorLockLevel == DetectionLevel.NoInfo) {
                    //LowVisibility.Logger.LogIfDebug($"Attacker:{CombatantHelper.Label(attacker)} has no sensor lock to target:{CombatantHelper.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    __result = __result + (float)LowVisibility.Config.NoSensorLockRangePenaltyMulti;
                }
                if (lockState.visionLockLevel == VisionLockType.None) {
                    //LowVisibility.Logger.LogIfDebug($"Attacker:{CombatantHelper.Label(attacker)} has no visual lock to target:{CombatantHelper.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    __result = __result + (float)LowVisibility.Config.NoVisualLockRangePenaltyMulti;
                }

                StaticEWState targetEWConfig = State.GetStaticState(target as AbstractActor);
                if (targetEWConfig == null) { LowVisibility.Logger.Log($"WARNING: targetEWConfig NULL FOR TARGET:{CombatantHelper.Label(target)}"); }
                if (targetEWConfig.HasStealthRangeMod()) {
                    //LowVisibility.Logger.LogIfDebug($"target:{CombatantHelper.Label(target as AbstractActor)} has StealthRangeMod with values: " +
                    //    $"short:{targetEWConfig.stealthRangeMod[0]} medium:{targetEWConfig.stealthRangeMod[1]} long:{targetEWConfig.stealthRangeMod[2]} ");

                    float distance = Vector3.Distance(attackPosition, targetPosition);
                    //LowVisibility.Logger.LogIfDebug($"  distance is:{distance} vs weapon min:{weapon.MinRange} short:{weapon.ShortRange} " +
                    //    $"medium:{weapon.MediumRange} long:{weapon.LongRange} max:{weapon.MaxRange}");

                    int weaponStealthMod = targetEWConfig.StealthRangeModAtDistance(weapon, distance);
                    if (weaponStealthMod != 0) {
                        __result = __result + (float)weaponStealthMod;
                    }
                }

                if (targetEWConfig.HasStealthMoveMod()) {
                    int stealthMoveMod = targetEWConfig.StealthMoveModForActor(target as AbstractActor);
                    if (stealthMoveMod != 0) {
                        __result = __result + (float)stealthMoveMod;
                    }
                }
            }            
        }
    }

    //public string GetAllModifiersDescription(AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
    [HarmonyPatch(typeof(ToHit), "GetAllModifiersDescription")]
    public static class ToHit_GetAllModifiersDescription {
        private static void Postfix(ToHit __instance, ref string __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            LowVisibility.Logger.LogIfTrace($"Getting modifier descriptions for attacker:{CombatantHelper.Label(attacker)} " +
                $"using weapon:{weapon.Name} vs target:{CombatantHelper.Label(target)}");

            if (__instance != null && attacker != null && target != null && weapon != null) {
                LockState lockState = GetUnifiedLockStateForTarget(attacker, target as AbstractActor);
                if (lockState == null) { LowVisibility.Logger.Log($"WARNING: LOCKSTATE NULL FOR TARGET:{CombatantHelper.Label(target)}"); }
                if (lockState.sensorLockLevel == DetectionLevel.NoInfo) {
                    __result = string.Format("{0}NO SENSOR LOCK {1:+#;-#}; ", __result, LowVisibility.Config.NoSensorLockRangePenaltyMulti);
                }
                if (lockState.visionLockLevel == VisionLockType.None) {
                    __result = string.Format("{0}NO VISUAL LOCK {1:+#;-#}; ", __result, LowVisibility.Config.NoVisualLockRangePenaltyMulti);
                }

                StaticEWState targetEWConfig = State.GetStaticState(target as AbstractActor);
                if (targetEWConfig == null) { LowVisibility.Logger.Log($"WARNING: targetEWConfig NULL FOR TARGET:{CombatantHelper.Label(target)}"); }
                if (targetEWConfig.HasStealthRangeMod()) {
                    float distance = Vector3.Distance(attackPosition, targetPosition);
                    int weaponStealthMod = targetEWConfig.StealthRangeModAtDistance(weapon, distance);
                    if (weaponStealthMod != 0) {
                        __result = string.Format("{0}STEALTH - RANGE {1:+#;-#}; ", __result, weaponStealthMod);
                    }
                }

                if (targetEWConfig.HasStealthMoveMod()) {
                    int stealthMoveMod = targetEWConfig.StealthMoveModForActor(target as AbstractActor);
                    if (stealthMoveMod != 0) {
                        __result = string.Format("{0}STEALTH - MOVEMENT {1:+#;-#}; ", __result, stealthMoveMod);
                    }
                }
            }
        }
    }
}
