using BattleTech;
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

                // Vision modifiers
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(weapon, distance);
                int heatVisionMod = attackerState.GetHeatVisionAttackMod(targetActor, weapon);
                int mimeticMod = targetState.MimeticAttackMod(attackerState, weapon, distance);
                if (!locks.hasLineOfSight) {
                    __result = __result + (float)Mod.Config.SensorsOnlyPenalty;
                } else {
                    if (zoomVisionMod != 0) {
                        __result = __result + (float)zoomVisionMod;
                    }
                    if (heatVisionMod != 0) {
                        __result = __result + (float)heatVisionMod;
                    }
                    if (mimeticMod != 0) {
                        __result = __result + (float)mimeticMod;
                    }
                }

                // Sensor modifiers
                int ecmShieldMod = targetState.GetECMShieldAttackModifier(attackerState);
                int stealthMod = targetState.StealthAttackMod(attackerState, weapon, distance);
                if (locks.sensorLock == SensorScanType.NoInfo) {
                    __result = __result + (float)Mod.Config.SensorsOnlyPenalty;
                } else {
                    if (ecmShieldMod != 0) {
                        __result = __result + (float)ecmShieldMod;
                    }
                    if (stealthMod != 0) {
                        __result = __result + (float)stealthMod;
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
                Locks locks = State.LocksForTarget(attacker, targetActor);
                float distance = Vector3.Distance(attackPosition, targetPosition);
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);

                // Vision modifiers
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(weapon, distance);
                int heatVisionMod = attackerState.GetHeatVisionAttackMod(targetActor, weapon);
                int mimeticMod = targetState.MimeticAttackMod(attackerState, weapon, distance);
                if (!locks.hasLineOfSight) {
                    __result = string.Format("{0}NO LINE OF SIGHT {1:+#;-#}; ", __result, Mod.Config.SensorsOnlyPenalty);
                } else {
                    if (zoomVisionMod != 0) {
                        __result = string.Format("{0}ZOOM MODE {1:+#;-#}; ", __result, zoomVisionMod);
                    }
                    if (heatVisionMod != 0) {
                        __result = string.Format("{0}THERMAL MODE {1:+#;-#}; ", __result, heatVisionMod);
                    }
                    if (mimeticMod != 0) {
                        __result = string.Format("{0}MIMETIC ARMOR {1:+#;-#}; ", __result, mimeticMod);
                    }
                }

                // Sensor modifiers
                int ecmShieldMod = targetState.GetECMShieldAttackModifier(attackerState);
                int stealthMod = targetState.StealthAttackMod(attackerState, weapon, distance);
                if (locks.sensorLock == SensorScanType.NoInfo) {
                    __result = string.Format("{0}NO SENSOR LOCK {1:+#;-#}; ", __result, Mod.Config.VisionOnlyPenalty);
                } else {
                    if (ecmShieldMod != 0) {
                        __result = string.Format("{0}ECM SHIELD {1:+#;-#}; ", __result, ecmShieldMod);
                    }
                    if (stealthMod != 0) {
                        __result = string.Format("{0}STEALTH {1:+#;-#}; ", __result, stealthMod);
                    }
                }
            }
        }
    }
}
