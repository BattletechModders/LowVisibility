using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Object;
using System;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Show the targeting computer for blips as well as LOSFull 
    [HarmonyPatch(typeof(CombatHUD), "SubscribeToMessages")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class CombatHUD_SubscribeToMessages {

        private static CombatGameState Combat = null;
        private static CombatHUDTargetingComputer TargetingComputer = null;
        private static Traverse ShowTargetMethod = null;

        public static void Postfix(CombatHUD __instance, bool shouldAdd) {
            //LowVisibility.Logger.Debug("CombatHUD:SubscribeToMessages:post - entered.");
            if (shouldAdd) {
                Combat = __instance.Combat;
                TargetingComputer = __instance.TargetingComputer;
                ShowTargetMethod = Traverse.Create(__instance).Method("ShowTarget", new Type[] { typeof(ICombatant) });

                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(OnActorTargeted), shouldAdd);
                // Disable the previous registration 
                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(__instance.OnActorTargetedMessage), false);
            } else {
                Combat = null;
                TargetingComputer = null;
                ShowTargetMethod = null;

                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(OnActorTargeted), shouldAdd);
            }

        }

        // Cleanup our previous registration
        public static void OnCombatGameDestroyed(CombatGameState Combat) {
            if (Combat != null) {
                Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(OnActorTargeted), false);
            }
        }

        public static void OnActorTargeted(MessageCenterMessage message) {
            //LowVisibility.Logger.Debug("CombatHUD:SubscribeToMessages:OnActorTargeted - entered.");
            ActorTargetedMessage actorTargetedMessage = message as ActorTargetedMessage;
            ICombatant combatant = Combat.FindActorByGUID(actorTargetedMessage.affectedObjectGuid);
            if (combatant == null) { combatant = Combat.FindCombatantByGUID(actorTargetedMessage.affectedObjectGuid); }

            if (Combat.LocalPlayerTeam.VisibilityToTarget(combatant) >= VisibilityLevel.Blip0Minimum) {
                Mod.Log.Trace("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility >= Blip0, showing target.");
                if (ShowTargetMethod != null) {
                    ShowTargetMethod.GetValue(combatant);
                } else {
                    Mod.Log.Info("WARNING: CHUD:STM caled with a null traverse!");
                }
            } else {
                Mod.Log.Trace("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility < Blip0, hiding target.");
            }
        }
    }
    
    [HarmonyPatch(typeof(CombatHUDWeaponSlot), "SetHitChance", new Type[] { typeof(ICombatant) })]
    public static class CombatHUDWeaponSlot_SetHitChance {

        private static void Postfix(CombatHUDWeaponSlot __instance, ICombatant target) {
            if (__instance == null || target == null) { return;  }
            Mod.Log.Trace("CHUDWS:SHC - entered.");

            AbstractActor actor = __instance.DisplayedWeapon.parent;
            AbstractActor targetActor = target as AbstractActor;
            Traverse AddToolTipDetailMethod = Traverse.Create(__instance).Method("AddToolTipDetail", new Type[] { typeof(string), typeof(int) });

            if (targetActor != null && __instance.DisplayedWeapon != null) {
                //LowVisibility.Logger.Debug($"___CombatHUDTargetingComputer - SetHitChance for source:{CombatantUtils.Label(targetActor)} target:{CombatantUtils.Label(targetActor)}");
                Locks lockState = State.LocksForTarget(actor, targetActor);
                float distance = Vector3.Distance(actor.CurrentPosition, targetActor.CurrentPosition);
                EWState attackerState = new EWState(actor);

                // Visual modifiers
                if (!lockState.hasLineOfSight) {
                    AddToolTipDetailMethod.GetValue(new object[] { "NO LOS", Mod.Config.SensorsOnlyPenalty });
                }

                VisionModeModifer vismodeMod = attackerState.CalculateVisionModeModifier(target, distance, __instance.DisplayedWeapon);
                if (vismodeMod.modifier != 0) {
                    AddToolTipDetailMethod.GetValue(new object[] { vismodeMod.label, vismodeMod.modifier });
                }

                if (lockState.sensorLock == SensorScanType.NoInfo) {
                    AddToolTipDetailMethod.GetValue(new object[] { "NO SENSOR LOCK", Mod.Config.VisionOnlyPenalty });
                }

                EWState targetState = new EWState(targetActor);
                if (targetState.GetECMShieldAttackModifier(attackerState) != 0) {
                    Mod.Log.Debug($" CHUDWS:SHC Target:{CombatantUtils.Label(target)} has ECM_SHIELD, applying modifier: {targetState.GetECMShieldAttackModifier(attackerState)}");
                    AddToolTipDetailMethod.GetValue(new object[] { "ECM SHIELD", targetState.GetECMShieldAttackModifier(attackerState) });
                }

                if (targetState.HasStealth()) {
                    // Sensor stealth
                    float magnitude = (actor.CurrentPosition - target.CurrentPosition).magnitude;
                    int sStealthMod = targetState.GetSensorStealthAttackModifier(__instance.DisplayedWeapon, magnitude, attackerState);
                    if (sStealthMod != 0) {
                        AddToolTipDetailMethod.GetValue(new object[] { "SENSOR STEALTH", sStealthMod });
                    }

                    // Visual stealth
                    int vStealthMod = targetState.GetVisionStealthAttackModifier(__instance.DisplayedWeapon, magnitude, attackerState);
                    if (vStealthMod != 0) {
                        AddToolTipDetailMethod.GetValue(new object[] { "VISUAL STEALTH", vStealthMod });
                    }

                }
            }          
        }
    }
}
