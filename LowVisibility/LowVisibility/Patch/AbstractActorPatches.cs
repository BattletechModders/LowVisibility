using BattleTech;
using Harmony;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Initializes any custom status effects. Without this, they wont' be read.
    [HarmonyPatch(typeof(AbstractActor), "InitEffectStats")]
    public static class AbstractActor_InitEffectStats {
        private static void Postfix(AbstractActor __instance) {
            Mod.Log.Trace("AA:IES entered");

            __instance.StatCollection.AddStatistic<int>(ModStats.SensorCheck, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.TacticsMod, 0);

            __instance.StatCollection.AddStatistic<int>(ModStats.ECMCarrier, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.ECMShield, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.ECMJammed, 0);

            __instance.StatCollection.AddStatistic<int>(ModStats.SensorStealth, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.SensorStealthCharge, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.VisionStealth, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.VisionStealthCharge, 0);

            __instance.StatCollection.AddStatistic<int>(ModStats.Jammer, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.Probe, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.Stealth, 0);

            __instance.StatCollection.AddStatistic<bool>(ModStats.SharesSensors, false);

            __instance.StatCollection.AddStatistic<int>(ModStats.StealthMoveMod, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.VismodeZoom, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.VismodeHeat, 0);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {

        public static void Prefix(AbstractActor __instance, int stackItemID) {
            if (stackItemID == -1 || __instance == null || __instance.HasBegunActivation) {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DO ES NOTHING. GAH!
                return;
            }

            Mod.Log.Debug($"-- OnActivationBegin: Effects targeting actor: {CombatantUtils.Label(__instance)}");
            List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance);
            foreach (Effect effect in list) {
                Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
            }

            foreach (AbstractActor unit in __instance.team.units) {
                if (unit.GUID != __instance.GUID) {
                    Mod.Log.Debug($" friendly actor effects: {CombatantUtils.Label(unit)}");
                    List<Effect> list2 = __instance.Combat.EffectManager.GetAllEffectsTargeting(unit);
                    foreach (Effect effect in list2) {
                        Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
                    }
                }
            }

            Mod.Log.Debug($" Updating effects to all actors from actor: {CombatantUtils.Label(__instance)}");

            Mod.Log.Debug($"=== AbstractActor:OnActivationBegin:pre - processing {CombatantUtils.Label(__instance)}");
            if (__instance.team == __instance.Combat.LocalPlayerTeam) {
                State.LastPlayerActor = __instance.GUID;
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationEnd")]
    public static class AbstractActor_OnActivationEnd {

        public static void Prefix(AbstractActor __instance) {
            
            Mod.Log.Debug($"-- OnActivationEnd: Effects targeting actor: {CombatantUtils.Label(__instance)}");
            List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance);
            foreach (Effect effect in list) {
                Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
            }

            foreach (AbstractActor unit in __instance.team.units) {
                if (unit.GUID != __instance.GUID) {
                    Mod.Log.Debug($" friendly actor effects: {CombatantUtils.Label(unit)}");
                    List<Effect> list2 = __instance.Combat.EffectManager.GetAllEffectsTargeting(unit);
                    foreach (Effect effect in list2) {
                        Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
                    }
                }
            }

            Mod.Log.Debug($" Updating effects to all actors from actor: {CombatantUtils.Label(__instance)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnPositionUpdate")]
    public static class AbstractActor_OnPositionUpdate  {

        public static void Prefix(AbstractActor __instance, Vector3 position) {

            //List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance);
            //foreach (Effect effect in list) {
            //    Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
            //}

            //foreach (AbstractActor unit in __instance.team.units) {
            //    if (unit.GUID != __instance.GUID) {
            //        Mod.Log.Debug($" friendly actor effects: {CombatantUtils.Label(unit)}");
            //        List<Effect> list2 = __instance.Combat.EffectManager.GetAllEffectsTargeting(unit);
            //        foreach (Effect effect in list2) {
            //            Mod.Log.Debug($"   -- EffectID: {effect.EffectData.Description.Id}");
            //        }
            //    }
            //}

            Mod.Log.Debug($"-- OnPositionUpdate: Updating effects to all actors from actor: {CombatantUtils.Label(__instance)} at position: {position}");
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
            if (State.TurnDirectorStarted && __instance.TeamId != null) {
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

    //[HarmonyPatch(typeof(AbstractActor), "HasECMAbilityInstalled", MethodType.Getter)]
    //public static class AbstractActor_HasECMAbilityInstalled_Getter {
    //    public static void Postfix(AbstractActor __instance, ref bool __result) {
    //        Mod.Log.Trace("AA:HECMAI:GET entered");

    //        List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance)
    //            .FindAll((Effect x) => x.EffectData.effectType == EffectType.StatisticEffect && 
    //            (x.EffectData.Description.Id == "ECMStealth_GhostEffect" || x.EffectData.statisticData.statName == ModStats.ECMCarrier));

    //        Mod.Log.Debug($"  Found {list.Count} effects for actor: {CombatantUtils.Label(__instance)}");
    //        __result = list.Count > 0;
    //    }
    //}

    // ParentECM Carrier is the unit carrying the ECM... duh
    //[HarmonyPatch(typeof(AbstractActor), "ParentECMCarrier", MethodType.Getter)]
    //public static class AbstractActor_ParentECMCarrier_Getter {
    //    public static void Postfix(AbstractActor __instance, ref bool __result) {
    //        Mod.Log.Trace("AA:PECMC:GET entered");

    //        List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance)
    //            .FindAll((Effect x) => x.EffectData.effectType == EffectType.StatisticEffect &&
    //            (x.EffectData.Description.Id == "ECMStealth_GhostEffect" || x.EffectData.statisticData.statName == ModStats.ECMCarrier));
    //        __result = list.Count > 0;
    //    }
    //}

    [HarmonyPatch(typeof(AbstractActor), "CreateEffect")]
    [HarmonyPatch(new Type[] { typeof(EffectData), typeof(Ability), typeof(string), typeof(int), typeof(AbstractActor), typeof(bool) })]
    public static class AbstractActor_CreateEffect_AbstractActor {
        public static void Postfix(AbstractActor __instance, EffectData effect, Ability fromAbility, string effectId, int stackItemUID, AbstractActor creator, bool skipLogging) {
            Mod.Log.Trace("AA:CreateEffect entered");

            Mod.Log.Debug($" Creating effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from creator: {CombatantUtils.Label(creator)}");
            if (effect.effectType == EffectType.StatisticEffect && 
                (effect.statisticData.statName == ModStats.SensorStealth || effect.statisticData.statName == ModStats.VisionStealth)) {
                Mod.Log.Debug("  - Stealth effect found, rebuilding visibility.");
                List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
            }

        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CreateEffect")]
    [HarmonyPatch(new Type[] { typeof(EffectData), typeof(Ability), typeof(string), typeof(int), typeof(Team), typeof(bool) })]
    public static class AbstractActor_CreateEffect_Team {
        public static void Postfix(AbstractActor __instance, EffectData effect, Ability fromAbility, string sourceID, int stackItemUID, Team creator, bool skipLogging) {
            Mod.Log.Trace("AA:CreateEffect entered");

            Mod.Log.Debug($" Creating effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from team: {creator.GUID}");
            if (effect.effectType == EffectType.StatisticEffect &&
                (effect.statisticData.statName == ModStats.SensorStealth || effect.statisticData.statName == ModStats.VisionStealth)) {
                Mod.Log.Debug("  - Stealth effect found, rebuilding visibility.");
                List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
            }

        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CancelEffect")]
        public static class AbstractActor_CancelEffect {
        public static void Postfix(AbstractActor __instance, EffectData effect) {
            Mod.Log.Trace("AA:CancelEffect entered");

            Mod.Log.Debug($" Cancelling effect on actor:{CombatantUtils.Label(__instance)} effectId:{effect.Description.Id} from creator: ");
            if (effect.effectType == EffectType.StatisticEffect &&
                (effect.statisticData.statName == ModStats.SensorStealth || effect.statisticData.statName == ModStats.VisionStealth)) {
                Mod.Log.Debug("  - Stealth effect found, rebuilding visibility.");
                List<ICombatant> allLivingCombatants = __instance.Combat.GetAllLivingCombatants();
                __instance.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
            }
        }
    }


    //[HarmonyPatch(typeof(AbstractActor), "OnNewRound")]
    //public static class AbstractActor_OnNewRound {
    //    public static void Postfix(AbstractActor __instance, int round) {
    //        Mod.Log.Debug("AA:ONR entered");

    //        List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance);
    //        foreach (Effect effect in list) {
    //            Mod.Log.Debug($" Actor: ({CombatantUtils.Label(__instance)}) has effect: ({effect.EffectData.Description.Id})");
    //        }

    //    }
    //}


    //[HarmonyPatch(typeof(AuraCache), "GetEffectID")]
    //public static class AuraCache_GetEffectID {
    //    public static void Postfix(AuraCache __instance, AbstractActor fromActor, string fromEffectOwnerId, EffectData fromEffect, AbstractActor target) {
    //        Mod.Log.Debug("AC:GEID entered");

    //        Mod.Log.Debug($"  fromActor: {CombatantUtils.Label(fromActor)} has effect: {fromEffect.Description.Id} to target: {CombatantUtils.Label(target)}");
    //    }
    //}


    [HarmonyPatch(typeof(AbstractActor), "OnAuraAdded")]
    public static class AbstractActor_OnAuraAdded {
        public static void Postfix(AbstractActor __instance, MessageCenterMessage message) {
            //Mod.Log.Debug("AA:OAA entered");

            AuraAddedMessage auraAddedMessage = message as AuraAddedMessage;
            //Mod.Log.Debug($" Adding aura: {auraAddedMessage.effectData.Description.Id} to target: {auraAddedMessage.targetID}");
            if (auraAddedMessage.targetID == __instance.GUID) {
                if (auraAddedMessage.effectData.statisticData.statName == ModStats.ECMShield) {
                    if (__instance.Combat.TurnDirector.IsInterleaved) {
                        __instance.Combat.MessageCenter.PublishMessage(
                            new FloatieMessage(auraAddedMessage.creatorID, auraAddedMessage.targetID,
                                new Text("ECM PROTECTED", new object[0]), FloatieMessage.MessageNature.Buff));
                    }

                    if (ActorHelper.IsECMCarrier(__instance)) {
                        VfxHelper.EnableECMCarrierEffect(__instance, auraAddedMessage.effectData);
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
                if (auraRemovedMessage.effectData.statisticData.statName == ModStats.ECMShield) {
                    if (ActorHelper.IsECMCarrier(__instance)) {
                        VfxHelper.DisableECMCarrierEffect(__instance);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnMoveComplete")]
    public static class AbstractActor_OnMoveComplete {

        public static void Prefix(AbstractActor __instance) {
            
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

