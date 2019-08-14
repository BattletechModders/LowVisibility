using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;

namespace LowVisibility.Patch {

    // Show the targeting computer for blips as well as LOSFull 
    [HarmonyPatch(typeof(CombatHUD), "SubscribeToMessages")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class CombatHUD_SubscribeToMessages {

        private static CombatGameState Combat = null;
        private static CombatHUDTargetingComputer TargetingComputer = null;
        private static Traverse ShowTargetMethod = null;

        public static void Postfix(CombatHUD __instance, bool shouldAdd) {
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
            Traverse AddToolTipDetailMethod = Traverse.Create(__instance).Method("AddToolTipDetail", new Type[] { typeof(string), typeof(int) });

            if (target is AbstractActor targetActor && __instance.DisplayedWeapon != null) {
                float magnitude = (actor.CurrentPosition - target.CurrentPosition).magnitude;
                EWState attackerState = new EWState(actor);
                EWState targetState = new EWState(targetActor);

                // Vision modifiers
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(__instance.DisplayedWeapon, magnitude);
                int heatVisionMod = attackerState.GetHeatVisionAttackMod(targetActor, __instance.DisplayedWeapon);
                int mimeticMod = targetState.MimeticAttackMod(attackerState);
                bool canSpotTarget = VisualLockHelper.CanSpotTarget(actor, actor.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, actor.Combat.LOS);

                // Sensor modifiers
                int ecmShieldMod = targetState.ECMAttackMod(attackerState);
                int stealthMod = targetState.StealthAttackMod(attackerState, __instance.DisplayedWeapon, magnitude);
                int narcMod = targetState.NarcAttackMod(attackerState);
                int tagMod = targetState.TagAttackMod(attackerState);
                SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, actor);
                 
                if (sensorScan == SensorScanType.NoInfo && !canSpotTarget) {
                    AddToolTipDetailMethod.GetValue(new object[] { "FIRING BLIND", Mod.Config.BlindFirePenalty });
                } else {
                    if (!canSpotTarget) {
                        AddToolTipDetailMethod.GetValue(new object[] { "NO VISUALS", Mod.Config.NoVisualsPenalty });
                    } else {
                        if (zoomVisionMod != 0) {
                            AddToolTipDetailMethod.GetValue(new object[] { "ZOOM VISION", zoomVisionMod });
                        }
                        if (heatVisionMod != 0) {
                            AddToolTipDetailMethod.GetValue(new object[] { "HEAT VISION", zoomVisionMod });
                        }
                        if (mimeticMod != 0) {
                            AddToolTipDetailMethod.GetValue(new object[] { "MIMETIC ARMOR", mimeticMod });
                        }
                    }

                    if (sensorScan == SensorScanType.NoInfo) {
                        AddToolTipDetailMethod.GetValue(new object[] { "NO SENSOR INFO", Mod.Config.NoSensorInfoPenalty });
                    } else {
                        if (ecmShieldMod != 0) {
                            AddToolTipDetailMethod.GetValue(new object[] { "ECM SHIELD", targetState.ECMAttackMod(attackerState) });
                        }
                        if (stealthMod != 0) {
                            AddToolTipDetailMethod.GetValue(new object[] { "STEALTH", stealthMod });
                        }
                        if (stealthMod != 0) {
                            AddToolTipDetailMethod.GetValue(new object[] { "TARGET NARCED", narcMod });
                        }
                        if (stealthMod != 0) {
                            AddToolTipDetailMethod.GetValue(new object[] { "TARGET TAGGED", tagMod });
                        }
                    }
                }
            }
        }
    }
}
