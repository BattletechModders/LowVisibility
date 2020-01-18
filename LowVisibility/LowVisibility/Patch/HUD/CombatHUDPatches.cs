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

            AbstractActor attacker = __instance.DisplayedWeapon.parent;
            Traverse AddToolTipDetailMethod = Traverse.Create(__instance).Method("AddToolTipDetail", new Type[] { typeof(string), typeof(int) });

            if (target is AbstractActor targetActor && __instance.DisplayedWeapon != null) {
                float magnitude = (attacker.CurrentPosition - target.CurrentPosition).magnitude;
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);

                // If we can't see the target, apply the No Visuals penalty
                bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);
                int mimeticMod = targetState.MimeticAttackMod(attackerState);
                int eyeballAttackMod = canSpotTarget ? mimeticMod : Mod.Config.Attack.NoVisualsPenalty;

                // Zoom applies independently of visibility (request from Harkonnen)
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(__instance.DisplayedWeapon, magnitude);
                int zoomAttackMod = attackerState.HasZoomVisionToTarget(__instance.DisplayedWeapon, magnitude) ? zoomVisionMod - mimeticMod : Mod.Config.Attack.NoVisualsPenalty;

                bool hasVisualAttack = (eyeballAttackMod < Mod.Config.Attack.NoVisualsPenalty || zoomAttackMod < Mod.Config.Attack.NoVisualsPenalty);

                // Sensor attack bucket.  Sensors always fallback, so roll everything up and cap
                int narcAttackMod = targetState.NarcAttackMod(attackerState);
                int tagAttackMod = targetState.TagAttackMod(attackerState);
                int ecmShieldAttackMod = targetState.ECMAttackMod(attackerState);
                int stealthAttackMod = targetState.StealthAttackMod(attackerState, __instance.DisplayedWeapon, magnitude);

                bool hasSensorAttack = SensorLockHelper.CalculateSharedLock(targetActor, attacker) <= SensorScanType.NoInfo;
                int sensorsAttackMod = Mod.Config.Attack.NoSensorsPenalty;
                if (hasSensorAttack) {
                    sensorsAttackMod = 0;
                    sensorsAttackMod -= narcAttackMod;
                    sensorsAttackMod -= tagAttackMod;
                    sensorsAttackMod += ecmShieldAttackMod;
                    sensorsAttackMod += stealthAttackMod;
                }
                if (sensorsAttackMod > Mod.Config.Attack.NoSensorsPenalty) {
                    sensorsAttackMod = Mod.Config.Attack.NoSensorsPenalty;
                    hasSensorAttack = false;
                }

                // Check firing blind
                if (!hasVisualAttack && !hasSensorAttack) {
                    string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_FIRING_BLIND]).ToString();
                    AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.FiringBlindPenalty });
                } else {
                    // Visual attacks
                    if (!hasVisualAttack) {
                        string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NO_VISUALS]).ToString();
                        AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.NoVisualsPenalty });
                    } else {
                        // If the zoom + mimetic is better than eyeball, use that. Otherwise, we're using the good ol mk.1 eyeball
                        if (zoomAttackMod > eyeballAttackMod) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_ZOOM_VISION]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, zoomVisionMod });
                        }

                        if (mimeticMod != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_MIMETIC]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, mimeticMod });
                        }
                    }

                    if (attackerState.HasHeatVisionToTarget(__instance.DisplayedWeapon, magnitude)) {
                        int heatAttackMod = attackerState.GetHeatVisionAttackMod(targetActor, magnitude, __instance.DisplayedWeapon);
                        string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_HEAT_VISION]).ToString();
                        AddToolTipDetailMethod.GetValue(new object[] { localText, mimeticMod });
                    }

                    if (!hasSensorAttack) {
                        string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NO_SENSORS]).ToString();
                        AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.NoSensorsPenalty });
                    } else {

                        if (targetState.TagAttackMod(attackerState) != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_ECM_SHEILD]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, ecmShieldAttackMod * -1 });
                        }
                        if (targetState.ECMAttackMod(attackerState) != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_STEALTH]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, stealthAttackMod * -1 });
                        }
                        if (targetState.NarcAttackMod(attackerState) != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NARCED ]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, narcAttackMod });
                        }
                        if (targetState.StealthAttackMod(attackerState, __instance.DisplayedWeapon, magnitude) != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_TAGGED]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, tagAttackMod });
                        }
                    }
                }
            }
        }
    }
}
