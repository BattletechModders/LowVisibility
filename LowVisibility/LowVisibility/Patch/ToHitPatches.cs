using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
    public static class ToHit_GetAllModifiers {

        [HarmonyBefore(new string[] { "Sheepy.BattleTechMod.AttackImprovementMod" })]
        private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            Mod.Log.Debug($"Getting modifiers for attacker:{CombatantUtils.Label(attacker)} " +
                $"using weapon:{weapon.Name} vs target:{CombatantUtils.Label(target)} with initial result:{__result}");

            AbstractActor targetActor = target as AbstractActor;
            if (__instance != null && attacker != null && targetActor != null) {
                float distance = Vector3.Distance(attackPosition, targetPosition);
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);

                // Vision modifiers
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(weapon, distance);
                int heatVisionMod = attackerState.GetHeatVisionAttackMod(targetActor, weapon);
                int mimeticMod = targetState.MimeticAttackMod(attackerState);
                bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);

                // Sensor modifiers
                int ecmShieldMod = targetState.ECMAttackMod(attackerState);
                int stealthMod = targetState.StealthAttackMod(attackerState, weapon, distance);
                int narcMod = targetState.NarcAttackMod(attackerState);
                int tagMod = targetState.TagAttackMod(attackerState);
                SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, attacker);

                if (sensorScan == SensorScanType.NoInfo && !canSpotTarget) {
                    __result = __result + (float)Mod.Config.BlindFirePenalty;
                } else {
                    if (!canSpotTarget) {
                        __result = __result + (float)Mod.Config.NoVisualsPenalty;
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

                    if (sensorScan == SensorScanType.NoInfo) {
                        __result = __result + (float)Mod.Config.NoSensorInfoPenalty;
                    } else {
                        if (ecmShieldMod != 0) {
                            __result = __result + (float)ecmShieldMod;
                        }
                        if (stealthMod != 0) {
                            __result = __result + (float)stealthMod;
                        }
                        if (narcMod != 0) {
                            __result = __result + (float)narcMod;
                        }
                        if (tagMod != 0) {
                            __result = __result + (float)tagMod;
                        }
                    }
                }

                Mod.Log.Debug($" -- Final result:{__result}");

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
                float distance = Vector3.Distance(attackPosition, targetPosition);
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);

                // Vision modifiers
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(weapon, distance);
                int heatVisionMod = attackerState.GetHeatVisionAttackMod(targetActor, weapon);
                int mimeticMod = targetState.MimeticAttackMod(attackerState);
                bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);

                // Sensor modifiers
                int ecmShieldMod = targetState.ECMAttackMod(attackerState);
                int stealthMod = targetState.StealthAttackMod(attackerState, weapon, distance);
                int narcMod = targetState.NarcAttackMod(attackerState);
                int tagMod = targetState.TagAttackMod(attackerState);
                SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, attacker);

                if (sensorScan == SensorScanType.NoInfo && !canSpotTarget) {
                    __result = string.Format("{0}FIRING BLIND {1:+#;-#}; ", __result, Mod.Config.BlindFirePenalty);
                } else {
                    if (!canSpotTarget) {
                        __result = string.Format("{0}NO VISUALS {1:+#;-#}; ", __result, Mod.Config.NoVisualsPenalty);
                    } else {
                        if (zoomVisionMod != 0) {
                            __result = string.Format("{0}ZOOM VISION {1:+#;-#}; ", __result, zoomVisionMod);
                        }
                        if (heatVisionMod != 0) {
                            __result = string.Format("{0}HEAT VISION {1:+#;-#}; ", __result, heatVisionMod);
                        }
                        if (mimeticMod != 0) {
                            __result = string.Format("{0}MIMETIC ARMOR {1:+#;-#}; ", __result, mimeticMod);
                        }
                    }

                    if (sensorScan == SensorScanType.NoInfo) {
                        __result = string.Format("{0}NO SENSOR INFO {1:+#;-#}; ", __result, Mod.Config.NoSensorInfoPenalty);
                    } else {
                        if (ecmShieldMod != 0) {
                            __result = string.Format("{0}ECM SHIELD {1:+#;-#}; ", __result, ecmShieldMod);
                        }
                        if (stealthMod != 0) {
                            __result = string.Format("{0}STEALTH {1:+#;-#}; ", __result, stealthMod);
                        }
                        if (ecmShieldMod != 0) {
                            __result = string.Format("{0}NARCED {1:+#;-#}; ", __result, narcMod);
                        }
                        if (stealthMod != 0) {
                            __result = string.Format("{0}TAGGED {1:+#;-#}; ", __result, tagMod);
                        }
                    }
                }
            }
        }
    }
}
