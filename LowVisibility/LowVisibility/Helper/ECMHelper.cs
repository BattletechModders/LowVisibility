using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {

    class ECMHelper {

        public static void UpdateECMState(AbstractActor source) {

            //List<AbstractActor> playerActors = HostilityHelper.PlayerActors(source.Combat)
            //    .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();
            //List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(source.Combat)
            //    .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();
            //List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(source.Combat)
            //    .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();
            //List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(source.Combat)
            //    .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();

            //List<AbstractActor> hostiles = new List<AbstractActor>();
            //List<AbstractActor> friendlies = new List<AbstractActor>();
            //if (HostilityHelper.IsLocalPlayerEnemy(source)) {
            //    hostiles = playerActors.Union(alliedActors).ToList();
            //    friendlies = enemyActors;                
            //} else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
            //    hostiles = enemyActors;
            //    friendlies = neutralActors;
            //} else if (HostilityHelper.IsLocalPlayerAlly(source) || HostilityHelper.IsPlayer(source)) {
            //    hostiles = enemyActors;
            //    friendlies = playerActors.Union(alliedActors).ToList();
            //}

            //// Determine ECM jamming
            //int jamming = CalculateECMStrength(source, hostiles);
            //if (jamming > 0) {
            //    Mod.Log.Trace($"  -- target:{CombatantUtils.Label(source)} has ECM jamming:{jamming}");
            //    State.AddECMJamming(source, jamming);
            //} else {
            //    State.RemoveECMJamming(source);
            //}

            //// Determine ECM protection
            //int protection = CalculateECMStrength(source, friendlies);
            //if (protection > 0) {
            //    Mod.Log.Trace($"  -- target:{CombatantUtils.Label(source)} has ECM protection:{protection}");
            //    State.AddECMProtection(source, protection);
            //} else {
            //    State.RemoveECMProtection(source);
            //}


            // TODO: FIX
            //// Check for Narc effects
            //List<Effect> allEffects = source.Combat.EffectManager.GetAllEffectsTargeting(source);
            //List<Effect> narcEffects = allEffects != null
            //    ? allEffects.Where(e => e?.EffectData?.tagData?.tagList != null)
            //        .Where(e => e.EffectData.tagData.tagList.Any(s => s.Contains(TagPrefixNarcEffect)))
            //        .ToList()
            //    : new List<Effect>();
            //Mod.Log.Trace($"  -- target:{CombatantUtils.Label(source)} has:{(narcEffects != null ? narcEffects.Count : 0)} NARC effects");
            //int narcEffect = 0;
            //foreach (Effect effect in narcEffects) {
            //    string effectTag = effect?.EffectData?.tagData?.tagList?.FirstOrDefault(t => t.StartsWith(TagPrefixNarcEffect));
            //    if (effectTag != null) {
            //        string[] split = effectTag.Split('_');
            //        if (split.Length == 2) {
            //            int modifier = int.Parse(split[1].Substring(1));
            //            if (modifier > narcEffect) {
            //                narcEffect = modifier;
            //                Mod.Log.Debug($"  Effect:{effect.EffectData.Description.Id} adding modifier:{modifier}.");
            //            }
            //        } else {
            //            Mod.Log.Info($"Actor:{CombatantUtils.Label(source)} - MALFORMED EFFECT TAG -:{effect}");
            //        }
            //    }
            //}
            //if (narcEffect != 0) {
            //    Mod.Log.Debug($"  -- target:{CombatantUtils.Label(source)} has NARC beacon with value:{narcEffect}");
            //    if (protection >= narcEffect) {
            //        Mod.Log.Debug($"  -- target:{CombatantUtils.Label(source)} has NARC beacon mod:{narcEffect} " +
            //            $"and ECM protection:{protection}. NARC has no effect.");
            //        State.RemoveNARCEffect(source);
            //    } else {
            //        int delta = narcEffect - protection;
            //        State.AddNARCEffect(source, delta);
            //        Mod.Log.Debug($"  -- target:{CombatantUtils.Label(source)} has NARC beacon with modifier:{narcEffect} " +
            //            $"and ECM protection:{protection}. Setting narcEffectStrenght to:{delta}");
            //    }
            //} else {
            //    State.RemoveNARCEffect(source);
            //}

            // TODO: FIX
            // Check for TAG effects
            //List<Effect> tagEffects = allEffects != null
            //    ? allEffects.Where(e => e?.EffectData?.tagData?.tagList != null)
            //        .Where(e => e.EffectData.tagData.tagList.Contains(TagPrefixTagEffect))
            //        .ToList()
            //        : new List<Effect>();
            //Mod.Log.Trace($"  -- target:{CombatantUtils.Label(source)} has:{(tagEffects != null ? tagEffects.Count : 0)} TAG effects");
            //int tagEffect = 0;
            //foreach (Effect effect in tagEffects) {
            //    string effectTag = effect?.EffectData?.tagData?.tagList?.FirstOrDefault(t => t.StartsWith(TagPrefixTagEffect));
            //    tagEffect = effect.Duration.numMovementsRemaining;
            //}

            //if (tagEffect != 0) {
            //    State.AddTAGEffect(source, tagEffect);
            //    Mod.Log.Debug($"  -- target:{CombatantUtils.Label(source)} has TAG effect with value:{tagEffect}");
            //} else {
            //    State.RemoveTAGEffect(source);
            //}
        }

        //private static int CalculateECMStrength(AbstractActor target, List<AbstractActor> sources) {
        //    int ecmStrength = 0;
        //    int ecmSourceCount = 0;
        //    foreach (AbstractActor actor in sources) {
                
        //        float actorsDistance = Vector3.Distance(target.CurrentPosition, actor.CurrentPosition);

        //        // if the actor has ECM, add to the ecmStrength
        //        EWState actorStaticState = new EWState(actor);
        //        if (actorStaticState.ecmMod != 0 && actorsDistance <= actorStaticState.ecmRange) {
        //            Mod.Log.Debug($"Target:{CombatantUtils.Label(target)} and ECM source:{CombatantUtils.Label(actor)} are {actorsDistance}m apart, " +
        //                $"within of ECM bubble range of:{actorStaticState.ecmRange}");
        //            if (actorStaticState.ecmMod > ecmStrength) { ecmStrength = actorStaticState.ecmMod; }
        //            ecmSourceCount++;
        //        }
        //    }

        //    if (ecmSourceCount > 1) {
        //        int multiSourceModifier = (ecmSourceCount - 1) * Mod.Config.MultipleECMSourceModifier;
        //        Mod.Log.Debug($"Target:{CombatantUtils.Label(target)} has:{ecmSourceCount} ECM sources within range. " +
        //            $"Additional modifier of:{multiSourceModifier} applied to ecmStrength:{ecmStrength}");
        //        ecmStrength += multiSourceModifier;
        //    }

        //    return ecmStrength;
        //}

    }
}
