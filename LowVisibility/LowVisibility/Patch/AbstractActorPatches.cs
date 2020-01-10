﻿using BattleTech;
using BattleTech.Rendering.Mood;
using FogOfWar;
using Harmony;
using HBS;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Initializes any custom status effects. Without this, they wont' be read.
    [HarmonyPatch(typeof(AbstractActor), "InitEffectStats")]
    public static class AbstractActor_InitEffectStats {
        private static void Postfix(AbstractActor __instance) {
            Mod.Log.Trace("AA:IES entered");

            __instance.StatCollection.AddStatistic<int>(ModStats.TacticsMod, 0);

            __instance.StatCollection.AddStatistic<int>(ModStats.CurrentRoundEWCheck, 0);

            // ECM
            __instance.StatCollection.AddStatistic<int>(ModStats.ECMCarrier, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.ShieldedByECM, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.JammedByECM, 0);

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
            __instance.StatCollection.AddStatistic<string>(ModStats.ZoomVision, "");

            // Narc 
            __instance.StatCollection.AddStatistic<string>(ModStats.NarcEffect, "");

            // Tag
            __instance.StatCollection.AddStatistic<string>(ModStats.TagEffect, "");

            // Vision sharing
            __instance.StatCollection.AddStatistic<bool>(ModStats.SharesVision, false);

            // Night vision
            __instance.StatCollection.AddStatistic<bool>(ModStats.NightVision, false);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {

        public static void Prefix(AbstractActor __instance, int stackItemID) {
            if (stackItemID == -1 || __instance == null || __instance.HasBegunActivation) {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DOES NOTHING. GAH!
                return;
            }

            // Draw stealth if applicable
            EWState actorState = new EWState(__instance);
            if (actorState.HasStealth()) {
                Mod.Log.Debug($"-- Sending message to update stealth");
                StealthChangedMessage message = new StealthChangedMessage(__instance.GUID);
                __instance.Combat.MessageCenter.PublishMessage(message);
            }

            // If friendly, reset the map visibility 
            if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid && 
                __instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.TeamId)) {
                Mod.Log.Info($"{CombatantUtils.Label(__instance)} IS FRIENDLY, REBUILDING FOG OF WAR");

                if (actorState.HasNightVision() && ModState.GetMapConfig().isDark) {
                    Mod.Log.Info($"Enabling night vision mode.");
                    VfxHelper.EnableNightVisionEffect(__instance);
                } else {
                    // TODO: This is likely never triggered due to the patch below... remove?
                    if (ModState.IsNightVisionMode) {
                        VfxHelper.DisableNightVisionEffect();
                    }
                }
                
                VfxHelper.RedrawFogOfWar(__instance);
            }
        }
    }

    // Disable the night vision effect when activation is complete
    [HarmonyPatch(typeof(AbstractActor), "OnActivationEnd")]
    public static class AbstractActor_OnActivationEnd {

        public static void Prefix(AbstractActor __instance) {
            Mod.Log.Trace("AA:OnAEnd - entered.");

            if (__instance != null && ModState.IsNightVisionMode) {
                VfxHelper.DisableNightVisionEffect();
            }

        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnPositionUpdate")]
    public static class AbstractActor_OnPositionUpdate  {

        public static void Prefix(AbstractActor __instance, Vector3 position) {
            Mod.Log.Trace($"AA:OPU entered");
             
        }

    }

    [HarmonyPatch(typeof(Mech), "InitGameRep")]
    public static class Mech_InitGameRep {
        public static void Postfix(Mech __instance, Transform parentTransform) {

        }
    }

    [HarmonyPatch(typeof(Vehicle), "InitGameRep")]
    public static class Vehicle_InitGameRep {
        public static void Postfix(Vehicle __instance, Transform parentTransform) {

        }
    }

    [HarmonyPatch(typeof(Turret), "InitGameRep")]
    public static class Turret_InitGameRep {
        public static void Postfix(Turret __instance, Transform parentTransform) {

        }
    }

    [HarmonyPatch(typeof(AbstractActor), "UpdateLOSPositions")]
    public static class AbstractActor_UpdateLOSPositions {
        public static void Prefix(AbstractActor __instance) {
            // Check for teamID; if it's not present, unit hasn't spawned yet. Defer to UnitSpawnPointGameLogic::SpawnUnit for these updates
            if (ModState.TurnDirectorStarted && __instance.TeamId != null) {
                Mod.Log.Trace($"AbstractActor_UpdateLOSPositions:pre - entered for {CombatantUtils.Label(__instance)}.");
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "HasLOSToTargetUnit")]
    public static class AbstractActor_HasLOSToTargetUnit {
        public static void Postfix(AbstractActor __instance, ref bool __result, ICombatant targetUnit) {
            //LowVisibility.Logger.Debug("AbstractActor:HasLOSToTargetUnit:post - entered.");

            // Forces you to be able to see targets that are only blips
            __result = __instance.VisibilityToTargetUnit(targetUnit) >= VisibilityLevel.Blip0Minimum;
            Mod.Log.Trace($"Actor{CombatantUtils.Label(__instance)} has LOSToTargetUnit? {__result} " +
                $"to target:{CombatantUtils.Label(targetUnit as AbstractActor)}");
            //LowVisibility.Logger.Trace($"Called from:{new StackTrace(true)}");
        }
    }


    [HarmonyPatch(typeof(AbstractActor), "CanDetectPositionNonCached")]
    public static class AbstractActor_CanDetectPositionNonCached {
        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CDPNC: source{CombatantUtils.Label(__instance)} checking detection " +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CanSeeTargetAtPositionNonCached")]
    public static class AbstractActor_CanSeeTargetAtPositionNonCached {
        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CSTAPNC: source{__instance} checking vision" +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CreateEffect")]
    [HarmonyPatch(new Type[] { typeof(EffectData), typeof(Ability), typeof(string), typeof(int), typeof(AbstractActor), typeof(bool) })]
    public static class AbstractActor_CreateEffect_AbstractActor {
        public static void Postfix(AbstractActor __instance, EffectData effect, Ability fromAbility, string effectId, int stackItemUID, AbstractActor creator, bool skipLogging) {
            Mod.Log.Trace("AA:CreateEffect entered");

            Mod.Log.Trace($" Creating effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from creator: {CombatantUtils.Label(creator)}");

            if (effect.effectType == EffectType.StatisticEffect) {
                if (ModStats.IsStealthStat(effect.statisticData.statName)) {
                    Mod.Log.Debug("  - Stealth effect found, rebuilding visibility.");
                    List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                    __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
                }

                if (ModStats.ShieldedByECM.Equals(effect.statisticData.statName) && creator.GUID == __instance.GUID) {
                    EWState sourceState = new EWState(__instance);
                    if (sourceState.IsECMCarrier()) {
                        Mod.Log.Debug("  - ECM carrier found, starting effect");
                        VfxHelper.EnableECMCarrierVfx(__instance, effect);
                    }
                }
            }

        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CreateEffect")]
    [HarmonyPatch(new Type[] { typeof(EffectData), typeof(Ability), typeof(string), typeof(int), typeof(Team), typeof(bool) })]
    public static class AbstractActor_CreateEffect_Team {
        public static void Postfix(AbstractActor __instance, EffectData effect, Ability fromAbility, string sourceID, int stackItemUID, Team creator, bool skipLogging) {
            Mod.Log.Trace("AA:CreateEffect entered");

            Mod.Log.Trace($" Creating effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from team: {creator.GUID}");

            if (effect.effectType == EffectType.StatisticEffect) {
                if (ModStats.IsStealthStat(effect.statisticData.statName)) {
                    Mod.Log.Debug("  - Stealth effect found, rebuilding visibility.");
                    List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                    __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
                }

                if (ModStats.ShieldedByECM.Equals(effect.statisticData.statName) && creator.GUID == __instance.GUID) {
                    EWState sourceState = new EWState(__instance);
                    if (sourceState.IsECMCarrier()) {
                        Mod.Log.Debug("  - ECM carrier found, starting effect");
                        VfxHelper.EnableECMCarrierVfx(__instance, effect);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CancelEffect")]
        public static class AbstractActor_CancelEffect {
        public static void Postfix(AbstractActor __instance, EffectData effect) {
            Mod.Log.Trace("AA:CancelEffect entered");

            Mod.Log.Debug($" Cancelling effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from creator: ");

            if (effect.effectType == EffectType.StatisticEffect) {
                if (effect.effectType == EffectType.StatisticEffect && ModStats.IsStealthStat(effect.statisticData.statName)) {
                    Mod.Log.Debug("  - Stealth effect found, rebuilding visibility.");
                    List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                    __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);

                    // TODO: Set current stealth pips?
                }

                if (ModStats.ShieldedByECM.Equals(effect.statisticData.statName)) {
                    EWState sourceState = new EWState(__instance);
                    if (!sourceState.IsECMCarrier()) {
                        Mod.Log.Debug("  - ECM carrier NOT found, disabling effect");
                        VfxHelper.DisableECMCarrierVfx(__instance);
                    }
                }
            }

            if (effect.effectType == EffectType.StatisticEffect && ModStats.IsStealthStat(effect.statisticData.statName)) {
                Mod.Log.Debug("  - Stealth effect found, rebuilding visibility.");
                List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);

                // TODO: Set current stealth pips?
            }

        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnAuraAdded")]
    public static class AbstractActor_OnAuraAdded {
        public static void Postfix(AbstractActor __instance, MessageCenterMessage message) {
            //Mod.Log.Debug("AA:OAA entered");

            AuraAddedMessage auraAddedMessage = message as AuraAddedMessage;
            Mod.Log.Debug($" Adding aura: {auraAddedMessage.effectData.Description.Id} to target: {auraAddedMessage.targetID}");
            if (auraAddedMessage.targetID == __instance.GUID) {
                if (auraAddedMessage.effectData.statisticData.statName == ModStats.ShieldedByECM) {
                    if (__instance.Combat.TurnDirector.IsInterleaved) {
                        __instance.Combat.MessageCenter.PublishMessage(
                            new FloatieMessage(auraAddedMessage.creatorID, auraAddedMessage.targetID,
                                new Text("ECM PROTECTED", new object[0]), FloatieMessage.MessageNature.Buff));
                    }

                    EWState actorState = new EWState(__instance);
                    if (actorState.IsECMCarrier()) {
                        VfxHelper.EnableECMCarrierVfx(__instance, auraAddedMessage.effectData);
                    }
                }

            }// TODO: Add else if conditional?
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnAuraRemoved")]
    public static class AbstractActor_OnAuraRemoved {
        public static void Postfix(AbstractActor __instance, MessageCenterMessage message) {
            AuraRemovedMessage auraRemovedMessage = message as AuraRemovedMessage;
            AbstractActor creator = __instance.Combat.FindActorByGUID(auraRemovedMessage.creatorID);
            Mod.Log.Debug($" Removing aura: {auraRemovedMessage.effectData.Description.Id} from target: {CombatantUtils.Label(__instance)} created by: {CombatantUtils.Label(creator)}");
            if (auraRemovedMessage.targetID == __instance.GUID) {
                if (auraRemovedMessage.effectData.statisticData.statName == ModStats.ShieldedByECM) {
                    EWState actorState = new EWState(__instance);
                    if (actorState.IsECMCarrier()) {
                        VfxHelper.DisableECMCarrierVfx(__instance);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnMoveComplete")]
    public static class AbstractActor_OnMoveComplete {

        public static void Prefix(AbstractActor __instance) {

            EWState actorState = new EWState(__instance);
            if (actorState.HasStealth()) {

            }

            Mod.Log.Debug($" OnMoveComplete: Effects targeting actor: {CombatantUtils.Label(__instance)}");
            List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance);
            foreach (Effect effect in list) {
                Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
            }

            foreach(AbstractActor unit in __instance.team.units) {
                if (unit.GUID != __instance.GUID) {
                    Mod.Log.Debug($" friendly actor effects: {CombatantUtils.Label(unit)}");
                    List<Effect> list2 = __instance.Combat.EffectManager.GetAllEffectsTargeting(unit);
                    foreach (Effect effect in list2) {
                        Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
                    }
                }
            }
            
        }
    }

}

