using BattleTech.UI;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{

    // Initializes any custom status effects. Without this, they wont' be read.
    [HarmonyPatch(typeof(AbstractActor), "InitEffectStats")]
    static class AbstractActor_InitEffectStats
    {
        static void Postfix(AbstractActor __instance)
        {
            Mod.Log.Trace?.Write("AA:IES entered");

            __instance.StatCollection.AddStatistic<int>(ModStats.TacticsMod, 0);

            __instance.StatCollection.AddStatistic<int>(ModStats.CurrentRoundEWCheck, 0);

            // ECM
            __instance.StatCollection.AddStatistic<int>(ModStats.ECMShield, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.ECMJamming, 0);

            // Sensors
            __instance.StatCollection.AddStatistic<int>(ModStats.AdvancedSensors, 0);

            // Probe
            __instance.StatCollection.AddStatistic<int>(ModStats.ProbeCarrier, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.PingedByProbe, 0);

            // Sensor Stealth
            __instance.StatCollection.AddStatistic<string>(ModStats.StealthEffect, "");

            // Visual Stealth
            __instance.StatCollection.AddStatistic<string>(ModStats.MimeticEffect, "");
            __instance.StatCollection.AddStatistic<int>(ModStats.MimeticCurrentSteps, 0);

            // Vision
            __instance.StatCollection.AddStatistic<string>(ModStats.HeatVision, "");
            __instance.StatCollection.AddStatistic<string>(ModStats.ZoomAttack, "");
            __instance.StatCollection.AddStatistic<int>(ModStats.ZoomVision, 0);

            // Narc 
            __instance.StatCollection.AddStatistic<string>(ModStats.NarcEffect, "");

            // Tag
            __instance.StatCollection.AddStatistic<string>(ModStats.TagEffect, "");

            // Vision sharing
            __instance.StatCollection.AddStatistic<bool>(ModStats.SharesVision, false);

            // Night vision
            __instance.StatCollection.AddStatistic<bool>(ModStats.NightVision, false);

            // Disabled sensors flag
            __instance.StatCollection.AddStatistic<int>(ModStats.DisableSensors, 2);
            if (Mod.Config.Sensors.SensorsOfflineAtSpawn)
            {

                if (__instance.Combat != null && __instance.Combat.TurnDirector != null && __instance.Combat.TurnDirector.GameHasBegun && __instance.Combat.TurnDirector.CurrentRound >= 2)
                {
                    __instance.StatCollection.Set<int>(ModStats.DisableSensors, __instance.Combat.TurnDirector.CurrentRound + 1); // initialize to start on the next round
                }
                Mod.Log.Info?.Write($"Disabling sensors on actor: {CombatantUtils.Label(__instance)} until: {__instance.StatCollection.GetValue<int>(ModStats.DisableSensors)}.");
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    static class AbstractActor_OnActivationBegin
    {

        static void Prefix(ref bool __runOriginal, AbstractActor __instance, int stackItemID)
        {
            if (!__runOriginal) return;

            if (stackItemID == -1 || __instance == null || __instance.HasBegunActivation)
            {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DOES NOTHING. GAH!
                return;
            }

            // Draw stealth if applicable
            EWState actorState = new EWState(__instance);
            if (actorState.HasStealth())
            {
                Mod.Log.Debug?.Write($"-- Sending message to update stealth");
                StealthChangedMessage message = new StealthChangedMessage(__instance.GUID);
                __instance.Combat.MessageCenter.PublishMessage(message);
            }

            // If friendly, reset the map visibility 
            if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid &&
                __instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.TeamId))
            {
                Mod.Log.Info?.Write($"{CombatantUtils.Label(__instance)} IS FRIENDLY, REBUILDING FOG OF WAR");

                if (actorState.HasNightVision() && ModState.GetMapConfig().isDark)
                {
                    Mod.Log.Info?.Write($"Enabling night vision mode.");
                    VfxHelper.EnableNightVisionEffect(__instance);
                }
                else
                {
                    // TODO: This is likely never triggered due to the patch below... remove?
                    if (ModState.IsNightVisionMode)
                    {
                        VfxHelper.DisableNightVisionEffect();
                    }
                }

                VfxHelper.RedrawFogOfWar(__instance);
            }
        }
    }

    // Disable the night vision effect when activation is complete
    [HarmonyPatch(typeof(AbstractActor), "OnActivationEnd")]
    public static class AbstractActor_OnActivationEnd
    {

        public static void Prefix(ref bool __runOriginal, AbstractActor __instance)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Write("AA:OnAEnd - entered.");

            if (__instance != null)
            {
                // Disable night vision 
                if (ModState.IsNightVisionMode) VfxHelper.DisableNightVisionEffect();

            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "HasLOSToTargetUnit")]
    public static class AbstractActor_HasLOSToTargetUnit
    {
        public static void Postfix(AbstractActor __instance, ref bool __result, ICombatant targetUnit)
        {
            //LowVisibility.Logger.Debug("AbstractActor:HasLOSToTargetUnit:post - entered.");

            // Forces you to be able to see targets that are only blips
            __result = __instance.VisibilityToTargetUnit(targetUnit) >= VisibilityLevel.Blip0Minimum;
            //Mod.Log.Trace?.Write($"Actor{CombatantUtils.Label(__instance)} has LOSToTargetUnit? {__result} " +
            //    $"to target:{CombatantUtils.Label(targetUnit as AbstractActor)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CreateEffect")]
    [HarmonyPatch(new Type[] { typeof(EffectData), typeof(Ability), typeof(string), typeof(int), typeof(AbstractActor), typeof(bool) })]
    public static class AbstractActor_CreateEffect_AbstractActor
    {
        public static void Postfix(AbstractActor __instance, EffectData effect, AbstractActor creator)
        {
            Mod.Log.Debug?.Write("AA:CreateEffect entered");

            Mod.Log.Debug?.Write($" Creating effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from creator: {CombatantUtils.Label(creator)}");

            if (effect.effectType == EffectType.StatisticEffect)
            {
                if (ModStats.IsStealthStat(effect.statisticData.statName))
                {
                    Mod.Log.Debug?.Write("  - Stealth effect found, rebuilding visibility.");
                    List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                    __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
                }
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CreateEffect")]
    [HarmonyPatch(new Type[] { typeof(EffectData), typeof(Ability), typeof(string), typeof(int), typeof(Team), typeof(bool) })]
    public static class AbstractActor_CreateEffect_Team
    {
        public static void Postfix(AbstractActor __instance, EffectData effect, Team creator)
        {
            Mod.Log.Debug?.Write("AA:CreateEffect entered");

            Mod.Log.Debug?.Write($" Creating team effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from team: {creator.GUID}");

            if (effect.effectType == EffectType.StatisticEffect)
            {
                if (ModStats.IsStealthStat(effect.statisticData.statName))
                {
                    Mod.Log.Debug?.Write("  - Stealth effect found, rebuilding visibility.");
                    List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                    __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
                }

            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CancelEffect")]
    public static class AbstractActor_CancelEffect
    {
        public static void Postfix(AbstractActor __instance, Effect effect)
        {
            Mod.Log.Trace?.Write("AA:CancelEffect entered");

            if (effect.EffectData.effectType == EffectType.StatisticEffect)
            {
                Mod.Log.Debug?.Write($" Cancelling effectId: '{effect.EffectData.Description.Id}'  effectName: '{effect.EffectData.Description.Name}'  " +
                    $"on actor: '{CombatantUtils.Label(__instance)}'  from creator: {effect.creatorID}");

                if (effect.EffectData.effectType == EffectType.StatisticEffect && ModStats.IsStealthStat(effect.EffectData.statisticData.statName))
                {
                    Mod.Log.Debug?.Write("  - Stealth effect found, rebuilding visibility.");
                    List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                    __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);

                    // TODO: Set current stealth pips?
                }

                if (ModStats.IsStealthStat(effect.EffectData.statisticData.statName))
                {
                    Mod.Log.Debug?.Write("  - Stealth effect found, rebuilding visibility.");
                    List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                    __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);

                    // TODO: Set current stealth pips?
                }
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnAuraAdded")]
    public static class AbstractActor_OnAuraAdded
    {
        public static void Postfix(AbstractActor __instance, MessageCenterMessage message)
        {
            //Mod.Log.Debug?.Write("AA:OAA entered");

            AuraAddedMessage auraAddedMessage = message as AuraAddedMessage;
            Mod.Log.Trace?.Write($" Adding aura: {auraAddedMessage.effectData.Description.Id} to target: {auraAddedMessage.targetID} from creator: {auraAddedMessage.creatorID}");
            if (auraAddedMessage.targetID == __instance.GUID && __instance.Combat.TurnDirector.IsInterleaved)
            {

                if (auraAddedMessage.effectData.statisticData.statName == ModStats.ECMShield)
                {
                    string localText = new Text(Mod.LocalizedText.Floaties[ModText.LT_FLOATIE_ECM_JAMMED]).ToString();
                    __instance.Combat.MessageCenter.PublishMessage(
                           new FloatieMessage(auraAddedMessage.creatorID, auraAddedMessage.targetID, localText, FloatieMessage.MessageNature.Buff));
                }

                if (auraAddedMessage.effectData.statisticData.statName == ModStats.ECMJamming)
                {
                    string localText = new Text(Mod.LocalizedText.Floaties[ModText.LT_FLOATIE_ECM_JAMMED]).ToString();
                    __instance.Combat.MessageCenter.PublishMessage(
                           new FloatieMessage(auraAddedMessage.creatorID, auraAddedMessage.targetID, localText, FloatieMessage.MessageNature.Debuff));
                }

            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnMoveComplete")]
    public static class AbstractActor_OnMoveComplete
    {

        static void Prefix(ref bool __runOriginal, AbstractActor __instance)
        {

            if (!__runOriginal) return;

            if (__instance.TeamId == __instance.Combat.LocalPlayerTeamGuid)
            {
                CombatHUDHelper.ForceNameRefresh(__instance.Combat);
            }

            // Refresh any CombatHUDMarkDisplays
            foreach (CombatHUDMarkDisplay chudMD in ModState.MarkContainerRefs.Keys) chudMD.RefreshInfo();

            if (Mod.Config.Toggles.LogEffectsOnMove)
            {
                Mod.Log.Debug?.Write($" OnMoveComplete: Effects targeting actor: {CombatantUtils.Label(__instance)}");
                List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance);
                foreach (Effect effect in list)
                {
                    if (effect.EffectData.statisticData != null)
                    {
                        Mod.Log.Debug?.Write($"   -- EffectID: '{effect.EffectData.Description.Id}'  Name: '{effect.EffectData.Description.Name}'  " +
                            $"StatName:'{effect.EffectData.statisticData.statName}'  StatValue:{effect.EffectData.statisticData.modValue}");
                    }
                    else
                    {
                        Mod.Log.Debug?.Write($"   -- EffectID: {effect.EffectData.Description.Id}  Name: {effect.EffectData.Description.Name}");
                    }
                }

                foreach (AbstractActor unit in __instance.team.units)
                {
                    if (unit.GUID != __instance.GUID)
                    {
                        Mod.Log.Debug?.Write($" friendly actor effects: {CombatantUtils.Label(unit)}");
                        List<Effect> list2 = __instance.Combat.EffectManager.GetAllEffectsTargeting(unit);
                        foreach (Effect effect in list2)
                        {
                            if (effect.EffectData.statisticData != null)
                            {
                                Mod.Log.Debug?.Write($"   -- EffectID: '{effect.EffectData.Description.Id}'  Name: '{effect.EffectData.Description.Name}'  " +
                                    $"StatName:'{effect.EffectData.statisticData.statName}'  StatValue:{effect.EffectData.statisticData.modValue}");
                            }
                            else
                            {
                                Mod.Log.Debug?.Write($"   -- EffectID: {effect.EffectData.Description.Id}  Name: {effect.EffectData.Description.Name}");
                            }
                        }
                    }
                }
            }

        }
    }

}

