using BattleTech;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Reflection;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static bool IsFromSave = false;

        public static void Prefix(TurnDirector __instance) {
            Mod.Log.Trace()?.Invoke("TD:OEB:pre entered.");

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
                            ActorHelper.UpdateSensorCheck(actor, false);

                            bool isPlayer = actor.TeamId == __instance.Combat.LocalPlayerTeamGuid;
                            if (isPlayer && randomPlayerActor == null) {
                                randomPlayerActor = actor;
                            }

                        } else {
                            Mod.Log.Debug()?.Invoke($"  Actor:{CombatantUtils.Label(actor)} was NULL!");
                        }
                    }
                    
                    if (randomPlayerActor != null)
                    {
                        Mod.Log.Debug()?.Invoke($"Assigning actor: {CombatantUtils.Label(randomPlayerActor)} as lastActive.");
                        ModState.LastPlayerActorActivated = randomPlayerActor;
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
            __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.OnAuraAdded,
                new ReceiveMessageCenterMessage(SelectedActorHelper.OnAuraAddedMessage), true);
            __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.OnAuraRemoved,
                new ReceiveMessageCenterMessage(SelectedActorHelper.OnAuraRemovedMessage), true);
        }

    }


    [HarmonyPatch(typeof(TurnDirector), "BeginNewRound")]
    public static class TurnDirector_BeginNewRound {

        public static void Prefix(TurnDirector __instance, int round) {
            Mod.Log.Trace()?.Invoke($"TD:BNR entered");
            Mod.Log.Info($"=== Turn Director is beginning round: {round}");

            // Update the current vision for all allied and friendly units
            foreach (AbstractActor actor in __instance.Combat.AllActors) {
                // If our sensors are offline, re-enable them
                if (actor.StatCollection.ContainsStatistic(ModStats.DisableSensors))
                {
                    Mod.Log.Info($"Actor: {CombatantUtils.Label(actor)} sensors are offline until: {actor.StatCollection.GetValue<int>(ModStats.DisableSensors)}");

                    if (round >= actor.StatCollection.GetValue<int>(ModStats.DisableSensors))
                    {
                        Mod.Log.Info($"Re-enabling sensors for {CombatantUtils.Label(actor)}");
                        actor.StatCollection.RemoveStatistic(ModStats.DisableSensors);
                    }
                }
                    
                // Update our sensors check
                ActorHelper.UpdateSensorCheck(actor, true);
                if (actor.TeamId == __instance.Combat.LocalPlayerTeamGuid)
                {
                    actor.VisibilityCache.RebuildCache(actor.Combat.GetAllImporantCombatants());
                    CombatHUDHelper.ForceNameRefresh(actor.Combat);
                }
            }

        }
    }

    // This must be called as a prefix, as TurnDirector deallocates this.Combat in 1.8
    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Prefix(TurnDirector __instance) {
            Mod.Log.Debug()?.Invoke($"TD:OCGD entered");
            // Remove all combat state
            CombatHUD_SubscribeToMessages.OnCombatGameDestroyed(__instance.Combat);

            // Unsubscribe from actor selected messages
            __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorSelectedMessage, 
                new ReceiveMessageCenterMessage(SelectedActorHelper.OnActorSelectedMessage), false);
            __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.OnAuraAdded,
                new ReceiveMessageCenterMessage(SelectedActorHelper.OnAuraAddedMessage), false);
            __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.OnAuraRemoved,
                new ReceiveMessageCenterMessage(SelectedActorHelper.OnAuraRemovedMessage), false);
            SelectedActorHelper.Combat = null;

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
            Mod.Log.Trace()?.Invoke($"TD:IFSPT entered");

            TurnDirector_OnEncounterBegin.IsFromSave = true;
        }
    }

}
