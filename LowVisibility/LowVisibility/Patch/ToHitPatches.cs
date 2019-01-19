using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using UnityEngine;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
    public static class ToHit_GetAllModifiers {
        private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            LowVisibility.Logger.LogIfTrace($"Getting modifiers for attacker:{CombatantHelper.Label(attacker)} " +
                $"using weapon:{weapon.Name} vs target:{CombatantHelper.Label(target)}");

            AbstractActor targetActor = target as AbstractActor;
            if (__instance != null && attacker != null && targetActor != null) {
                LockState lockState = GetUnifiedLockStateForTarget(attacker, targetActor);
                float distance = Vector3.Distance(attackPosition, targetPosition);
                StaticEWState attackerEWConfig = State.GetStaticState(attacker);
                StaticEWState targetEWConfig = State.GetStaticState(targetActor);

                if (lockState.sensorLockLevel == DetectionLevel.NoInfo) {
                    //LowVisibility.Logger.LogIfDebug($"Attacker:{CombatantHelper.Label(attacker)} has no sensor lock to target:{CombatantHelper.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    __result = __result + attackerEWConfig.CalculateZoomVisionMod(distance);
                }

                if (lockState.visionLockLevel == VisionLockType.None) {
                    //LowVisibility.Logger.LogIfDebug($"Attacker:{CombatantHelper.Label(attacker)} has no visual lock to target:{CombatantHelper.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    __result = __result + (float)LowVisibility.Config.SensorsOnlyPenalty;
                }
                
                if (targetEWConfig.HasStealthRangeMod()) {
                    //LowVisibility.Logger.LogIfDebug($"target:{CombatantHelper.Label(target as AbstractActor)} has StealthRangeMod with values: " +
                    //    $"short:{targetEWConfig.stealthRangeMod[0]} medium:{targetEWConfig.stealthRangeMod[1]} long:{targetEWConfig.stealthRangeMod[2]} ");
                    int weaponStealthMod = targetEWConfig.CalculateStealthRangeMod(weapon, distance);
                    if (weaponStealthMod != 0) {
                        __result = __result + (float)weaponStealthMod;
                    }
                }

                if (targetEWConfig.HasStealthMoveMod()) {
                    int stealthMoveMod = targetEWConfig.CalculateStealthMoveMod(targetActor);
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

            AbstractActor targetActor = target as AbstractActor;
            if (__instance != null && attacker != null && target != null && weapon != null && targetActor != null) {
                LockState lockState = GetUnifiedLockStateForTarget(attacker, targetActor);
                float distance = Vector3.Distance(attackPosition, targetPosition);
                StaticEWState attackerEWConfig = State.GetStaticState(attacker);

                if (lockState.sensorLockLevel == DetectionLevel.NoInfo) {
                    __result = string.Format("{0}NO SENSOR LOCK {1:+#;-#}; ", __result, attackerEWConfig.CalculateZoomVisionMod(distance));
                }

                if (lockState.visionLockLevel == VisionLockType.None) {
                    __result = string.Format("{0}NO VISUAL LOCK {1:+#;-#}; ", __result, LowVisibility.Config.SensorsOnlyPenalty);
                }

                StaticEWState targetEWConfig = State.GetStaticState(targetActor);
                if (targetEWConfig.HasStealthRangeMod()) {
                    int weaponStealthMod = targetEWConfig.CalculateStealthRangeMod(weapon, distance);
                    if (weaponStealthMod != 0) {
                        __result = string.Format("{0}STEALTH - RANGE {1:+#;-#}; ", __result, weaponStealthMod);
                    }
                }

                if (targetEWConfig.HasStealthMoveMod()) {
                    int stealthMoveMod = targetEWConfig.CalculateStealthMoveMod(targetActor);
                    if (stealthMoveMod != 0) {
                        __result = string.Format("{0}STEALTH - MOVEMENT {1:+#;-#}; ", __result, stealthMoveMod);
                    }
                }
            }
        }
    }
}
