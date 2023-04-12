using BattleTech.UI;
using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper
{

    // Helper that coordinates state changes on allies
    public static class SelectedActorHelper
    {

        public static CombatGameState Combat = null;

        // Records actor selected events to allow id of the last player unit a player selected
        public static void OnActorSelectedMessage(MessageCenterMessage message)
        {
            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
            AbstractActor actor = Combat.FindActorByGUID(actorSelectedMessage.affectedObjectGuid);
            if (actor.team.IsLocalPlayer)
            {
                Mod.Log.Info?.Write($"Updating last activated actor to: ({CombatantUtils.Label(actor)})");
                ModState.LastPlayerActorActivated = actor;

                EWState actorState = new EWState(actor);
                if (actorState.HasNightVision() && ModState.GetMapConfig().isDark)
                {
                    Mod.Log.Info?.Write($"Enabling night vision mode.");
                    VfxHelper.EnableNightVisionEffect(actor);
                }
                else
                {
                    if (ModState.IsNightVisionMode)
                    {
                        VfxHelper.DisableNightVisionEffect();
                    }
                }

                // Refresh the unit's vision
                VfxHelper.RedrawFogOfWar(actor);
                actor.VisibilityCache.RebuildCache(actor.Combat.GetAllImporantCombatants());
                CombatHUDHelper.ForceNameRefresh(actor.Combat);

                // Hack - turn on Vision indicator?
                VisRangeIndicator visRangeIndicator = VisRangeIndicator.Instance;
                visRangeIndicator.SetState(VisRangeIndicator.VisRangeIndicatorState.On);

                // Refresh any CombatHUDMarkDisplays
                foreach (CombatHUDMarkDisplay chudMD in ModState.MarkContainerRefs.Keys) chudMD.RefreshInfo();
            }
        }

        public static void OnAuraAddedMessage(MessageCenterMessage message)
        {
            Mod.Log.Debug?.Write("SAH == ON AURA ADDED");
            AuraAddedMessage auraAddedMessage = message as AuraAddedMessage;
            AbstractActor target = Combat.FindActorByGUID(auraAddedMessage.targetID);
            AbstractActor creator = Combat.FindActorByGUID(auraAddedMessage.creatorID);
            Mod.Log.Debug?.Write($"ON AURA ADDED: {CombatantUtils.Label(target)} from {CombatantUtils.Label(creator)}");
        }

        public static void OnAuraRemovedMessage(MessageCenterMessage message)
        {
            Mod.Log.Debug?.Write("SAH == ON AURA REMOVED");
            AuraRemovedMessage auraRemoveMessage = message as AuraRemovedMessage;
            AbstractActor target = Combat.FindActorByGUID(auraRemoveMessage.targetID);
            AbstractActor creator = Combat.FindActorByGUID(auraRemoveMessage.creatorID);
            Mod.Log.Debug?.Write($"ON AURA ADDED: {CombatantUtils.Label(target)} from {CombatantUtils.Label(creator)}");
        }
    }
}
