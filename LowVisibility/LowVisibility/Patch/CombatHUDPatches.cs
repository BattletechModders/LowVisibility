using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
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
            Traverse AddToolTipDetailMethod = Traverse.Create(__instance).Method("AddToolTipDetail", new Type[] { typeof(string), typeof(int) });

            if (target is AbstractActor targetActor && __instance.DisplayedWeapon != null) {
                float magnitude = (actor.CurrentPosition - target.CurrentPosition).magnitude;
                EWState attackerState = new EWState(actor);
                EWState targetState = new EWState(targetActor);

                // Vision modifiers
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(__instance.DisplayedWeapon, magnitude);
                int heatVisionMod = attackerState.GetHeatVisionAttackMod(targetActor, __instance.DisplayedWeapon);
                int mimeticMod = targetState.MimeticAttackMod(attackerState, __instance.DisplayedWeapon, magnitude);
                bool hasLineOfSight = VisualLockHelper.CalculateVisualLock(actor, actor.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, actor.Combat.LOS);
                if (!hasLineOfSight) {
                    AddToolTipDetailMethod.GetValue(new object[] { "NO LINE OF SIGHT", Mod.Config.NoLineOfSightPenalty });
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

                // Sensor modifiers
                int ecmShieldMod = targetState.GetECMShieldAttackModifier(attackerState);
                int stealthMod = targetState.StealthAttackMod(attackerState, __instance.DisplayedWeapon, magnitude);
                SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, actor);
                if (sensorScan == SensorScanType.NoInfo) {
                    AddToolTipDetailMethod.GetValue(new object[] { "NO SENSOR LOCK", Mod.Config.NoSensorLockPenalty });
                } else {
                    if (ecmShieldMod != 0) {
                        Mod.Log.Debug($" CHUDWS:SHC Target:{CombatantUtils.Label(target)} has ECM_SHIELD, applying modifier: {targetState.GetECMShieldAttackModifier(attackerState)}");
                        AddToolTipDetailMethod.GetValue(new object[] { "ECM SHIELD", targetState.GetECMShieldAttackModifier(attackerState) });
                    }
                    if (stealthMod != 0) {
                        Mod.Log.Debug($" CHUDWS:SHC Target:{CombatantUtils.Label(target)} has STEALTH, applying modifier: {targetState.GetECMShieldAttackModifier(attackerState)}");
                        AddToolTipDetailMethod.GetValue(new object[] { "SENSOR STEALTH", stealthMod });
                    }
                }

            }
        }
    }
}
