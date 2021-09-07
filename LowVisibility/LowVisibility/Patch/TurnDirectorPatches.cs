using BattleTech;
using CustomUnits;
using Harmony;
using IRBTModUtils.Extension;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Reflection;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin
    {

        public static bool IsFromSave = false;

        public static void Prefix(TurnDirector __instance)
        {
            Mod.Log.Trace?.Write("TD:OEB:pre entered.");

            // Initialize the probabilities
            ModState.InitializeCheckResults();
            ModState.InitMapConfig();
            ModState.TurnDirectorStarted = true;

            // Do a pre-encounter populate 
            if (__instance != null && __instance.Combat != null && __instance.Combat.AllActors != null)
            {
                // If we are coming from a save, don't recalculate everything - just roll with what we already have
                if (!IsFromSave)
                {
                    AbstractActor randomPlayerActor = null;
                    foreach (AbstractActor actor in __instance.Combat.AllActors)
                    {
                        if (actor != null)
                        {
                            // Make a pre-encounter detectCheck for them
                            ActorHelper.UpdateSensorCheck(actor, false);

                            bool isPlayer = actor.TeamId == __instance.Combat.LocalPlayerTeamGuid;
                            if (isPlayer && randomPlayerActor == null)
                            {
                                randomPlayerActor = actor;
                            }

                        }
                        else
                        {
                            Mod.Log.Debug?.Write($"  Actor:{CombatantUtils.Label(actor)} was NULL!");
                        }
                    }

                    if (randomPlayerActor != null)
                    {
                        Mod.Log.Debug?.Write($"Assigning actor: {CombatantUtils.Label(randomPlayerActor)} as lastActive.");
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

    [HarmonyPatch(typeof(TurnDirector), "StartFirstRound")]
    static class TurnDirector_StartFirstRound
    {
        static void Prefix()
        {
            Mod.Log.Info?.Write($"=== TurnDirectory is starting first round!");
            // Check fog state
            if (LanceSpawnerGameLogic_OnEnterActive.isObjectivesReady(ModState.Combat.ActiveContract))
            {
                if (ModState.GetMoodController() == null)
                    Mod.Log.Error?.Write("Mood controller was null when attempting to update after manual deploy!");

                Mod.Log.Info?.Write("Applying MoodController logic now that manual deploy is done.");
                Traverse applyMoodSettingsT = Traverse.Create(ModState.GetMoodController())
                    .Method("ApplyMoodSettings", new object[] { true, false });
                applyMoodSettingsT.GetValue();
            }
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "BeginNewRound")]
    public static class TurnDirector_BeginNewRound
    {

        public static void Prefix(TurnDirector __instance, int round)
        {
            Mod.Log.Trace?.Write($"TD:BNR entered");
            Mod.ActorStateLog.Info?.Write($"=== Turn Director is beginning round: {round}");

            // Update the current vision for all allied and friendly units
            foreach (AbstractActor actor in __instance.Combat.AllActors)
            {
                Mod.ActorStateLog.Info?.Write($" -- Updating actor: {actor.DistinctId()}");

                // If our sensors are offline, re-enable them
                if (actor.StatCollection.ContainsStatistic(ModStats.DisableSensors))
                {

                    if (round >= actor.StatCollection.GetValue<int>(ModStats.DisableSensors))
                    {
                        Mod.ActorStateLog.Info?.Write($"Re-enabling sensors for {CombatantUtils.Label(actor)}");
                        actor.StatCollection.RemoveStatistic(ModStats.DisableSensors);
                    }
                    else
                    {
                        Mod.ActorStateLog.Info?.Write($"Actor: {CombatantUtils.Label(actor)} sensors are offline until: {actor.StatCollection.GetValue<int>(ModStats.DisableSensors)}");
                    }
                }

                // Update our sensors check
                ActorHelper.UpdateSensorCheck(actor, true);

                // Print the current state of the actor
                EWState actorState = new EWState(actor);
                Mod.ActorStateLog.Info?.Write(actorState.ToString());

            }

            // Now that all sensor checks are updated, refresh visiblity for all actors
            foreach (AbstractActor actor in __instance.Combat.AllActors)
            {

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
    public static class TurnDirector_OnCombatGameDestroyed
    {
        public static void Prefix(TurnDirector __instance)
        {
            Mod.Log.Debug?.Write($"TD:OCGD entered");
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
    public static class EncounterLayerParent_InitFromSavePassTwo
    {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(EncounterLayerParent), "InitFromSavePassTwo", new Type[] { typeof(CombatGameState) });
        }

        public static void Postfix(EncounterLayerParent __instance, CombatGameState combat)
        {
            Mod.Log.Trace?.Write($"TD:IFSPT entered");

            TurnDirector_OnEncounterBegin.IsFromSave = true;
        }
    }

}
