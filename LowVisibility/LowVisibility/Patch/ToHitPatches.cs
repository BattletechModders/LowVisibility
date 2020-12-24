using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using UnityEngine;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
    public static class ToHit_GetAllModifiers {

        [HarmonyBefore(new string[] { "Sheepy.BattleTechMod.AttackImprovementMod" })]
        private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel) {

            //Mod.Log.Debug?.Write($"Getting modifiers for attacker:{CombatantUtils.Label(attacker)} " +
            //    $"using weapon:{weapon.Name} vs target:{CombatantUtils.Label(target)} with initial result:{__result}");

            AbstractActor targetActor = target as AbstractActor;
            if (__instance != null && attacker != null && targetActor != null) {
                float distance = Vector3.Distance(attackPosition, targetPosition);
                
                // Cache these
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);

                // If we can't see the target, apply the No Visuals penalty
                bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);
                int mimeticMod = targetState.MimeticAttackMod(attackerState);
                int eyeballAttackMod = canSpotTarget ? mimeticMod : Mod.Config.Attack.NoVisualsPenalty;

                // Zoom applies independently of visibility (request from Harkonnen)
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(weapon, distance);
                int zoomAttackMod = attackerState.HasZoomVisionToTarget(weapon, distance, lofLevel) ? zoomVisionMod - mimeticMod : Mod.Config.Attack.NoVisualsPenalty;

                bool hasVisualAttack = (eyeballAttackMod < Mod.Config.Attack.NoVisualsPenalty || zoomAttackMod < Mod.Config.Attack.NoVisualsPenalty);

                // Sensor attack bucket.  Sensors always fallback, so roll everything up and cap
                int narcAttackMod = targetState.NarcAttackMod(attackerState);
                int tagAttackMod = targetState.TagAttackMod(attackerState);

                int ecmJammedAttackMod = attackerState.ECMJammedAttackMod();
                int ecmShieldAttackMod = targetState.ECMAttackMod(attackerState);
                int stealthAttackMod = targetState.StealthAttackMod(attackerState, weapon, distance);

                bool hasSensorAttack = SensorLockHelper.CalculateSharedLock(targetActor, attacker) > SensorScanType.NoInfo;
                int sensorsAttackMod = Mod.Config.Attack.NoSensorsPenalty;
                if (hasSensorAttack) {
                    sensorsAttackMod = 0;
                    sensorsAttackMod += narcAttackMod;
                    sensorsAttackMod += tagAttackMod;

                    sensorsAttackMod += ecmJammedAttackMod;
                    sensorsAttackMod += ecmShieldAttackMod;
                    sensorsAttackMod += stealthAttackMod;
                }
                if (sensorsAttackMod > Mod.Config.Attack.NoSensorsPenalty) { 
                    sensorsAttackMod = Mod.Config.Attack.NoSensorsPenalty;
                    hasSensorAttack = false;
                }

                // Check firing blind
                if (!hasVisualAttack && !hasSensorAttack) {
                    __result += Mod.Config.Attack.FiringBlindPenalty;
                } else {

                    __result += (zoomAttackMod < eyeballAttackMod) ? zoomAttackMod : eyeballAttackMod;

                    if (attackerState.HasHeatVisionToTarget(weapon, distance)) { 
                        __result += attackerState.GetHeatVisionAttackMod(targetActor, distance, weapon);  
                    }

                    __result += sensorsAttackMod;
                }

            }
        }
    }

    //public string GetAllModifiersDescription(AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
    //[HarmonyPatch(typeof(ToHit), "GetAllModifiersDescription")]
    public static class ToHit_GetAllModifiersDescription {

        //[HarmonyBefore(new string[] { "Sheepy.BattleTechMod.AttackImprovementMod" })]
        private static void Postfix(ToHit __instance, ref string __result, AbstractActor attacker, Weapon weapon, ICombatant target, 
            Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot) {

            //Mod.Log.Debug?.Write($"Getting modifier descriptions for attacker:{CombatantUtils.Label(attacker)} " +
            //    $"using weapon:{weapon.Name} vs target:{CombatantUtils.Label(target)}");

            //AbstractActor targetActor = target as AbstractActor;
            //if (__instance != null && attacker != null && weapon != null && target != null && targetActor != null) {
            //    float distance = Vector3.Distance(attackPosition, targetPosition);
            //    EWState attackerState = new EWState(attacker);
            //    EWState targetState = new EWState(targetActor);

            //    // Vision modifiers
            //    int zoomVisionMod = attackerState.GetZoomVisionAttackMod(weapon, distance);
            //    int heatVisionMod = attackerState.GetHeatVisionAttackMod(targetActor, distance, weapon);
            //    int mimeticMod = targetState.MimeticAttackMod(attackerState);
            //    bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);

            //    // Sensor modifiers
            //    SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, attacker);
            //    int ecmShieldMod = targetState.ECMAttackMod(attackerState);
            //    int stealthMod = targetState.StealthAttackMod(attackerState, weapon, distance);
            //    int narcMod = targetState.NarcAttackMod(attackerState);
            //    int tagMod = targetState.TagAttackMod(attackerState);
            //    if (Mod.Config.Attack.NoSensorInfoPenalty > (ecmShieldMod + stealthMod + narcMod + tagMod)) { sensorScan = SensorScanType.NoInfo; }

            //    if (sensorScan == SensorScanType.NoInfo && !canSpotTarget) {
            //        string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_FIRING_BLIND]).ToString();
            //        __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, Mod.Config.Attack.BlindFirePenalty);
            //    } else {
            //        if (!canSpotTarget) {
            //            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NO_VISUALS]).ToString();
            //            __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, Mod.Config.Attack.NoVisualsPenalty);
            //        } else {
            //            if (zoomVisionMod != 0) {
            //                string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_ZOOM_VISION]).ToString();
            //                __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, zoomVisionMod);
            //            }
            //            if (heatVisionMod != 0) {
            //                string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_HEAT_VISION]).ToString();
            //                __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, heatVisionMod);
            //            }
            //            if (mimeticMod != 0) {
            //                string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_MIMETIC]).ToString();
            //                __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, mimeticMod);
            //            }
            //        }

            //        if (sensorScan == SensorScanType.NoInfo) {
            //            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NO_SENSORS]).ToString();
            //            __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, Mod.Config.Attack.NoSensorInfoPenalty);
            //        } else {
            //            if (ecmShieldMod != 0) {
            //                string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_ECM_SHEILD]).ToString();
            //                __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, ecmShieldMod);
            //            }
            //            if (stealthMod != 0) {
            //                string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_STEALTH]).ToString();
            //                __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, stealthMod);
            //            }
            //            if (ecmShieldMod != 0) {
            //                string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NARCED]).ToString();
            //                __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, narcMod);
            //            }
            //            if (stealthMod != 0) {
            //                string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_TAGGED]).ToString();
            //                __result = string.Format("{0}{1} {2:+#;-#}; ", __result, localText, tagMod);
            //            }
            //        }
            //    }
            //}
        }
    }
}
