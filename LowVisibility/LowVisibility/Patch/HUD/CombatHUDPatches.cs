﻿using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Show the targeting computer for blips as well as LOSFull 
    [HarmonyPatch(typeof(CombatHUD), "SubscribeToMessages")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class CombatHUD_SubscribeToMessages {

        private static CombatGameState Combat = null;
        private static Traverse ShowTargetMethod = null;

        public static void Postfix(CombatHUD __instance, bool shouldAdd) {
            if (shouldAdd) {
                Combat = __instance.Combat;
                ShowTargetMethod = Traverse.Create(__instance).Method("ShowTarget", new Type[] { typeof(ICombatant) });

                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(OnActorTargeted), shouldAdd);
                // Disable the previous registration 
                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(__instance.OnActorTargetedMessage), false);
            } else {
                Combat = null;
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
            Mod.Log.Trace?.Write("CHUD:STM:OAT - entered.");

            ActorTargetedMessage actorTargetedMessage = message as ActorTargetedMessage;
            if (message == null || actorTargetedMessage == null || actorTargetedMessage.affectedObjectGuid == null) return; // Nothing to do, bail

            ICombatant combatant = Combat.FindActorByGUID(actorTargetedMessage.affectedObjectGuid);
            if (combatant == null) { combatant = Combat.FindCombatantByGUID(actorTargetedMessage.affectedObjectGuid); }
            
            try {
                if (Combat.LocalPlayerTeam.VisibilityToTarget(combatant) >= VisibilityLevel.Blip0Minimum) {
                    Mod.Log.Trace?.Write("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility >= Blip0, showing target.");
                    if (ShowTargetMethod != null) {
                        ShowTargetMethod.GetValue(combatant);
                    } else {
                        Mod.Log.Info?.Write("WARNING: CHUD:STM caled with a null traverse!");
                    }
                } else {
                    Mod.Log.Trace?.Write("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility < Blip0, hiding target.");
                }
            } catch (Exception e) {
                Mod.Log.Error?.Write($"Failed to display HUD target: {CombatantUtils.Label(combatant)}!");
                Mod.Log.Error?.Write(e);
            }

            

            
        }
    }
    
   
}
