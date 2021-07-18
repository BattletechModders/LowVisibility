using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Patch {

  //[HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
  public enum LowVisModifierType {
    FiringBlind, NoVisuals, zoomVisionMod, mimeticMod, heatAttackMod, NoSensors, ecmJammed, ecmShield, narcAttack, tagAttack, stealthAttack
  }
  public class LowVisToHitState {
    public Dictionary<LowVisModifierType, int> modifiers = new Dictionary<LowVisModifierType, int>();
    public float get(LowVisModifierType type) { if (modifiers.TryGetValue(type, out int result)) { return result; }; return 0f; }
    public void AddModifiers(LowVisModifierType type, int value) { if (modifiers.ContainsKey(type)) { modifiers[type] = value; } else { modifiers.Add(type, value); }; }
  }
  public static class ToHit_GetAllModifiers {
    public static LowVisToHitState prepare(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      LowVisToHitState state = new LowVisToHitState();
      if (target is AbstractActor targetActor && weapon != null) {
        float magnitude = (attacker.CurrentPosition - target.CurrentPosition).magnitude;
        EWState attackerState = new EWState(attacker);
        EWState targetState = new EWState(targetActor);

        // If we can't see the target, apply the No Visuals penalty
        bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);
        int mimeticMod = targetState.MimeticAttackMod(attackerState);
        int eyeballAttackMod = canSpotTarget ? mimeticMod : Mod.Config.Attack.NoVisualsPenalty;

        // Zoom applies independently of visibility (request from Harkonnen)
        if (Vector3.Distance(attacker.CurrentPosition, attackPosition) > 0.1f) {
          Vector3 vector;
          lofLevel = attacker.Combat.LOS.GetLineOfFire(attacker, attackPosition, target, target.CurrentPosition, target.CurrentRotation, out vector);
        } else {
          lofLevel = attacker.VisibilityCache.VisibilityToTarget(target).LineOfFireLevel;
        }

        int zoomVisionMod = attackerState.GetZoomVisionAttackMod(weapon, magnitude);
        int zoomAttackMod = attackerState.HasZoomVisionToTarget(weapon, magnitude, lofLevel) ? zoomVisionMod - mimeticMod : Mod.Config.Attack.NoVisualsPenalty;
        Mod.Log.Debug?.Write($"  Visual attack == eyeball: {eyeballAttackMod} mimetic: {mimeticMod} zoomAtack: {zoomAttackMod}");

        bool hasVisualAttack = (eyeballAttackMod < Mod.Config.Attack.NoVisualsPenalty || zoomAttackMod < Mod.Config.Attack.NoVisualsPenalty);

        // Sensor attack bucket.  Sensors always fallback, so roll everything up and cap
        int narcAttackMod = targetState.NarcAttackMod(attackerState);
        int tagAttackMod = targetState.TagAttackMod(attackerState);

        int ecmJammedAttackMod = attackerState.ECMJammedAttackMod();
        int ecmShieldAttackMod = targetState.ECMAttackMod(attackerState);
        int stealthAttackMod = targetState.StealthAttackMod(attackerState, weapon, magnitude);
        Mod.Log.Debug?.Write($"  Sensor attack penalties == narc: {narcAttackMod}  tag: {tagAttackMod}  ecmShield: {ecmShieldAttackMod}  stealth: {stealthAttackMod}");

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
          Mod.Log.Debug?.Write($"  Rollup of penalties {sensorsAttackMod} is > than NoSensors, defaulting to {Mod.Config.Attack.NoSensorsPenalty} ");
          hasSensorAttack = false;
        }

        // Check firing blind
        if (!hasVisualAttack && !hasSensorAttack) {
          //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_FIRING_BLIND]).ToString();
          //AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.FiringBlindPenalty });
          state.AddModifiers(LowVisModifierType.FiringBlind, Mod.Config.Attack.FiringBlindPenalty);
        } else {
          // Visual attacks
          if (!hasVisualAttack) {
            //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NO_VISUALS]).ToString();
            //AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.NoVisualsPenalty });
            state.AddModifiers(LowVisModifierType.NoVisuals, Mod.Config.Attack.NoVisualsPenalty);
          } else {
            // If the zoom + mimetic is better than eyeball, use that. Otherwise, we're using the good ol mk.1 eyeball
            if (zoomAttackMod < eyeballAttackMod) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ZOOM_VISION]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, zoomVisionMod });
              state.AddModifiers(LowVisModifierType.zoomVisionMod, zoomVisionMod);
            }

            if (mimeticMod != 0) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_MIMETIC]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, mimeticMod });
              state.AddModifiers(LowVisModifierType.mimeticMod, mimeticMod);
            }
          }

          if (attackerState.HasHeatVisionToTarget(weapon, magnitude)) {
            int heatAttackMod = attackerState.GetHeatVisionAttackMod(targetActor, magnitude, weapon);
            if (heatAttackMod != 0) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_HEAT_VISION]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, heatAttackMod });
              state.AddModifiers(LowVisModifierType.heatAttackMod, heatAttackMod);
            }
          }

          if (!hasSensorAttack) {
            //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NO_SENSORS]).ToString();
            //AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.NoSensorsPenalty });
            state.AddModifiers(LowVisModifierType.NoSensors, Mod.Config.Attack.NoSensorsPenalty);
          } else {

            if (ecmJammedAttackMod != 0) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ECM_JAMMED]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, ecmJammedAttackMod });
              state.AddModifiers(LowVisModifierType.ecmJammed, ecmJammedAttackMod);
            }
            if (ecmShieldAttackMod != 0) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ECM_SHIELD]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, ecmShieldAttackMod });
              state.AddModifiers(LowVisModifierType.ecmShield, ecmShieldAttackMod);
            }
            if (narcAttackMod != 0) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NARCED]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, narcAttackMod });
              state.AddModifiers(LowVisModifierType.narcAttack, narcAttackMod);
            }
            if (tagAttackMod != 0) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_TAGGED]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, tagAttackMod });
              state.AddModifiers(LowVisModifierType.tagAttack, tagAttackMod);
            }
            if (stealthAttackMod != 0) {
              //string localText = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_STEALTH]).ToString();
              //AddToolTipDetailMethod.GetValue(new object[] { localText, stealthAttackMod });
              state.AddModifiers(LowVisModifierType.stealthAttack, stealthAttackMod);
            }
          }
        }
      }
      return state;
    }
    public static float get_FiringBlindMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.FiringBlind);
    }
    public static float get_NoVisualsMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.NoVisuals);
    }
    public static float get_zoomVisionMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.zoomVisionMod);
    }
    public static float get_mimeticMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.mimeticMod);
    }
    public static float get_heatAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.heatAttackMod);
    }
    public static float get_NoSensors(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.NoSensors);
    }
    public static float get_ecmJammedMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.ecmJammed);
    }
    public static float get_ecmShieldMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.ecmShield);
    }
    public static float get_narcAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.narcAttack);
    }
    public static float get_tagAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.tagAttack);
    }
    public static float get_stealthAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return ((LowVisToHitState)state).get(LowVisModifierType.stealthAttack);
    }
    public static void registerCACToHitModifiers() {
      CustAmmoCategories.ToHitModifiersHelper.registerNode("LOWVIS", ToHit_GetAllModifiers.prepare);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_FIRING_BLIND, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_FIRING_BLIND]).ToString(), true, false, get_FiringBlindMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_NO_VISUALS, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NO_VISUALS]).ToString(), true, false, get_NoVisualsMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_ZOOM_VISION, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ZOOM_VISION]).ToString(), true, false, get_zoomVisionMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_MIMETIC, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_MIMETIC]).ToString(), true, false, get_mimeticMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_HEAT_VISION, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_HEAT_VISION]).ToString(), true, false, get_heatAttackMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_NO_SENSORS, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NO_SENSORS]).ToString(), true, false, get_NoSensors, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_ECM_JAMMED, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ECM_JAMMED]).ToString(), true, false, get_ecmJammedMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_ECM_SHIELD, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ECM_SHIELD]).ToString(), true, false, get_ecmShieldMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_NARCED, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NARCED]).ToString(), true, false, get_narcAttackMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_TAGGED, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_TAGGED]).ToString(), true, false, get_tagAttackMod, null);
      CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier("LOWVIS", ModText.LT_ATTACK_STEALTH, new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ECM_SHIELD]).ToString(), true, false, get_stealthAttackMod, null);
    }
    //[HarmonyBefore(new string[] { "Sheepy.BattleTechMod.AttackImprovementMod" })]
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
