﻿using BattleTech;
using Harmony;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
    public static class ToHit_GetAllModifiers {

        [HarmonyBefore(new string[] { "Sheepy.BattleTechMod.AttackImprovementMod" })]
        private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            Mod.Log.Trace($"Getting modifiers for attacker:{CombatantUtils.Label(attacker)} " +
                $"using weapon:{weapon.Name} vs target:{CombatantUtils.Label(target)}");

            AbstractActor targetActor = target as AbstractActor;
            if (__instance != null && attacker != null && targetActor != null) {
                Locks locks = State.LocksForTarget(attacker, targetActor);
                float distance = Vector3.Distance(attackPosition, targetPosition);
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);

                Mod.Log.Debug($"TARGET:{CombatantUtils.Label(targetActor)} has ewState:{targetState.Details()}");

                if (locks.sensorLock == SensorScanType.NoInfo) {
                    //LowVisibility.Logger.Debug($"Attacker:{CombatantUtils.Label(attacker)} has no sensor lock to target:{CombatantUtils.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    __result = __result + (float)Mod.Config.VisionOnlyPenalty;
                }

                if (locks.visualLock == VisualScanType.None) {
                    //LowVisibility.Logger.Debug($"Attacker:{CombatantUtils.Label(attacker)} has no visual lock to target:{CombatantUtils.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    __result = __result + (float)Mod.Config.SensorsOnlyPenalty;
                }

                if (targetState.GetECMShieldAttackModifier() != 0) {
                    //LowVisibility.Logger.Debug($"Attacker:{CombatantUtils.Label(attacker)} has no visual lock to target:{CombatantUtils.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    Mod.Log.Debug($" Target:{CombatantUtils.Label(target)} has ECM_SHIELD, applying modifier: {targetState.GetECMShieldAttackModifier()}");
                    __result = __result + (float)targetState.GetECMShieldAttackModifier();
                }


                VisionModeModifer vismodeMod = attackerState.CalculateVisionModeModifier(target, distance, weapon);
                if (vismodeMod.modifier != 0) {
                    Mod.Log.Trace($" VisionMode modifier vs target:{CombatantUtils.Label(target)} => result:{__result} + {vismodeMod.modifier}");
                    __result = __result + (float)vismodeMod.modifier;
                }

                if (targetState.HasStealthMoveMod()) {
                    int stealthMoveMod = targetState.CalculateStealthMoveMod(targetActor);
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

        //[HarmonyBefore(new string[] { "Sheepy.BattleTechMod.AttackImprovementMod" })]
        private static void Postfix(ToHit __instance, ref string __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            Mod.Log.Debug($"Getting modifier descriptions for attacker:{CombatantUtils.Label(attacker)} " +
                $"using weapon:{weapon.Name} vs target:{CombatantUtils.Label(target)}");

            AbstractActor targetActor = target as AbstractActor;
            if (__instance != null && attacker != null && weapon != null && target != null && targetActor != null) {
                Locks lockState = State.LocksForTarget(attacker, targetActor);
                float distance = Vector3.Distance(attackPosition, targetPosition);
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);
                
                if (lockState.sensorLock == SensorScanType.NoInfo) {
                    __result = string.Format("{0}NO SENSOR LOCK {1:+#;-#}; ", __result, Mod.Config.VisionOnlyPenalty);
                }

                if (lockState.visualLock == VisualScanType.None) {
                    __result = string.Format("{0}NO VISUAL LOCK {1:+#;-#}; ", __result, Mod.Config.SensorsOnlyPenalty);
                }

                if (targetState.GetECMShieldAttackModifier() != 0) {
                    //LowVisibility.Logger.Debug($"Attacker:{CombatantUtils.Label(attacker)} has no visual lock to target:{CombatantUtils.Label(target as AbstractActor)} " +
                    //    $" applying modifier:{LowVisibility.Config.NoSensorLockAttackPenalty}");
                    Mod.Log.Debug($" Target:{CombatantUtils.Label(target)} has ECM_SHIELD, applying modifier: {targetState.GetECMShieldAttackModifier()} to label ECM_JAMING");
                    __result = string.Format("{0}ECM JAMMING {1:+#;-#}; ", __result, targetState.GetECMShieldAttackModifier());
                }

                VisionModeModifer vismodeMod = attackerState.CalculateVisionModeModifier(target, distance, weapon);
                if (vismodeMod.modifier != 0) {
                    Mod.Log.Trace($" VisionMode modifier vs target:{CombatantUtils.Label(target)} => {vismodeMod.ToString()}");
                    __result = string.Format("{0}{1} {2:+#;-#}; ", __result, vismodeMod.label, vismodeMod.modifier);
                }

                EWState targetEWConfig = new EWState(targetActor);
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
