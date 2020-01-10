using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.PostProcessing;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static bool IsFromSave = false;

        public static void Prefix(TurnDirector __instance) {
            Mod.Log.Trace("TD:OEB:pre entered.");

            // Initialize the probabilities
            ModState.InitializeCheckResults();
            ModState.InitMapConfig();
            ModState.TurnDirectorStarted = true;

            // Do a pre-encounter populate 
            if (__instance != null && __instance.Combat != null && __instance.Combat.AllActors != null) {
                // If we are coming from a save, don't recalculate everything - just roll with what we already have
                if (!IsFromSave) {
                    AbstractActor randomPlayerActor = null;
                    foreach (AbstractActor actor in __instance.Combat.AllActors) {
                        if (actor != null) {
                            // Make a pre-encounter detectCheck for them
                            ActorHelper.UpdateSensorCheck(actor);

                            bool isPlayer = actor.TeamId == __instance.Combat.LocalPlayerTeamGuid;
                            if (isPlayer && randomPlayerActor == null) {
                                randomPlayerActor = actor;
                            }

                        } else {
                            Mod.Log.Debug($"  Actor:{CombatantUtils.Label(actor)} was NULL!");
                        }
                    }
                }

            }

            // Initialize the VFX materials
            // TODO: Do a pooled instantiate here?
            VfxHelper.Initialize(__instance.Combat);

            // Attach to the message bus so we get updates on selected actor
            SelectedActorHelper.Combat = __instance.Combat;
            __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorSelectedMessage, 
                new ReceiveMessageCenterMessage(SelectedActorHelper.OnActorSelectedMessage), true);
        }

    }


    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Prefix(TurnDirector __instance, int round) {
            Mod.Log.Trace($"TD:BNR entered");
            Mod.Log.Debug($"=== TurnDirector - Beginning round:{round}");

            // Update the current vision for all allied and friendly units
            foreach (AbstractActor actor in __instance.Combat.AllActors) {
                ActorHelper.UpdateSensorCheck(actor);
            }

        }
    }

    // This must be called as a prefix, as TurnDirector deallocates this.Combat in 1.8
    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Prefix(TurnDirector __instance) {
            Mod.Log.Debug($"TD:OCGD entered");
            // Remove all combat state
            CombatHUD_SubscribeToMessages.OnCombatGameDestroyed(__instance.Combat);

            // Unsubscribe from actor selected messages
            SelectedActorHelper.Combat = null;
            __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorSelectedMessage, 
                new ReceiveMessageCenterMessage(SelectedActorHelper.OnActorSelectedMessage), false);

            // Reset state
            ModState.Reset();
        }
    }

    [HarmonyPatch()]
    public static class EncounterLayerParent_InitFromSavePassTwo {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(EncounterLayerParent), "InitFromSavePassTwo", new Type[] { typeof(CombatGameState) });
        }

        public static void Postfix(EncounterLayerParent __instance, CombatGameState combat) {
            Mod.Log.Trace($"TD:IFSPT entered");

            TurnDirector_OnEncounterBegin.IsFromSave = true;
        }
    }

    // Helper that coordinates state changes to allow id of the last player unit a player selected
    public static class SelectedActorHelper {

        public static CombatGameState Combat = null;

        public static void OnActorSelectedMessage(MessageCenterMessage message) {
            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
            AbstractActor actor = Combat.FindActorByGUID(actorSelectedMessage.affectedObjectGuid);
            if (actor.team.IsLocalPlayer) {
                Mod.Log.Info($"Updating last activated actor to: ({CombatantUtils.Label(actor)})");
                ModState.LastPlayerActorActivated = actor;

                EWState actorState = new EWState(actor);
                if (actorState.HasNightVision() && ModState.GetMapConfig().isDark) {
                    Mod.Log.Info($"Enabling night vision mode.");
                    VfxHelper.EnableNightVisionEffect(actor);
                } else {
                    if (ModState.IsNightVisionMode) {
                        VfxHelper.DisableNightVisionEffect();
                    }
                }

                // Refresh the unit's vision
                VfxHelper.RedrawFogOfWar(actor);
                
            }
        }
    }

}
