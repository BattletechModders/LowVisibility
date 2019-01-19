using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using LowVisibility.Object;
using UnityEngine;

namespace LowVisibility.Helper {

    class ECMHelper {
        public static void UpdateECMState(AbstractActor source) {

            List<AbstractActor> playerActors = HostilityHelper.PlayerActors(source.Combat)
                .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();
            List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(source.Combat)
                .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();
            List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(source.Combat)
                .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();
            List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(source.Combat)
                .Where(aa => !aa.IsTeleportedOffScreen && !aa.IsDead && !aa.IsFlaggedForDeath).ToList();

            List<AbstractActor> hostiles = new List<AbstractActor>();
            List<AbstractActor> friendlies = new List<AbstractActor>();
            if (HostilityHelper.IsLocalPlayerEnemy(source)) {
                hostiles = playerActors.Union(alliedActors).ToList();
                friendlies = enemyActors;                
            } else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
                hostiles = enemyActors;
                friendlies = neutralActors;
            } else if (HostilityHelper.IsLocalPlayerAlly(source) || HostilityHelper.IsPlayer(source)) {
                hostiles = enemyActors;
                friendlies = playerActors.Union(alliedActors).ToList();
            }

            // Determine ECM jamming
            int jamming = CalculateECMStrength(source, hostiles);
            if (jamming > 0) {
                LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(source)} has ECM jamming:{jamming}");
                State.AddECMJamming(source, jamming);
            } else {
                State.RemoveECMJamming(source);
            }

            // Determine ECM protection
            int protection = CalculateECMStrength(source, friendlies);
            if (protection > 0) {
                LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(source)} has ECM protection:{protection}");
                State.AddECMProtection(source, protection);
            } else {
                State.RemoveECMProtection(source);
            }

            // Check for Narc effects
            List<Effect> allEffects = source.Combat.EffectManager.GetAllEffectsTargeting(source);
            List<Effect> narcEffects = allEffects != null
                ? allEffects.Where(e => e?.EffectData?.tagData?.tagList != null)
                    .Where(e => e.EffectData.tagData.tagList.Any(s => s.Contains(StaticEWState.TagPrefixNarcEffect)))
                    .ToList()
                : new List<Effect>();
            LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(source)} has:{(narcEffects != null ? narcEffects.Count : 0)} NARC effects");
            int narcEffect = 0;
            foreach (Effect effect in narcEffects) {
                string effectTag = effect?.EffectData?.tagData?.tagList?.FirstOrDefault(t => t.StartsWith(StaticEWState.TagPrefixNarcEffect));
                if (effectTag != null) {
                    string[] split = effectTag.Split('_');
                    if (split.Length == 2) {
                        int modifier = int.Parse(split[1].Substring(1));
                        if (modifier > narcEffect) {
                            narcEffect = modifier;
                            LowVisibility.Logger.LogIfDebug($"  Effect:{effect.EffectData.Description.Id} adding modifier:{modifier}.");
                        }
                    } else {
                        LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(source)} - MALFORMED EFFECT TAG -:{effect}");
                    }
                }
            }
            if (narcEffect != 0) {
                LowVisibility.Logger.LogIfDebug($"  -- target:{CombatantHelper.Label(source)} has NARC beacon with value:{narcEffect}");
                if (protection >= narcEffect) {
                    LowVisibility.Logger.LogIfDebug($"  -- target:{CombatantHelper.Label(source)} has NARC beacon mod:{narcEffect} " +
                        $"and ECM protection:{protection}. NARC has no effect.");
                    State.RemoveNARCEffect(source);
                } else {
                    int delta = narcEffect - protection;
                    State.AddNARCEffect(source, delta);
                    LowVisibility.Logger.LogIfDebug($"  -- target:{CombatantHelper.Label(source)} has NARC beacon with modifier:{narcEffect} " +
                        $"and ECM protection:{protection}. Setting narcEffectStrenght to:{delta}");
                }
            } else {
                State.RemoveNARCEffect(source);
            }

            // Check for TAG effects
            List<Effect> tagEffects = allEffects != null
                ? allEffects.Where(e => e?.EffectData?.tagData?.tagList != null)
                    .Where(e => e.EffectData.tagData.tagList.Contains(StaticEWState.TagPrefixTagEffect))
                    .ToList()
                    : new List<Effect>();
            LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(source)} has:{(tagEffects != null ? tagEffects.Count : 0)} TAG effects");
            int tagEffect = 0;
            foreach (Effect effect in narcEffects) {
                string effectTag = effect?.EffectData?.tagData?.tagList?.FirstOrDefault(t => t.StartsWith(StaticEWState.TagPrefixTagEffect));
                tagEffect = effect.Duration.numMovementsRemaining;
            }

            if (tagEffect != 0) {
                State.AddTAGEffect(source, tagEffect);
                LowVisibility.Logger.LogIfDebug($"  -- target:{CombatantHelper.Label(source)} has TAG effect with value:{tagEffect}");
            } else {
                State.RemoveTAGEffect(source);
            }
        }

        private static int CalculateECMStrength(AbstractActor target, List<AbstractActor> sources) {
            int ecmStrength = 0;
            int ecmSourceCount = 0;
            foreach (AbstractActor actor in sources) {
                
                float actorsDistance = Vector3.Distance(target.CurrentPosition, actor.CurrentPosition);

                // if the actor has ECM, add to the ecmStrength
                StaticEWState actorStaticState = State.GetStaticState(actor);
                if (actorStaticState.ecmMod != 0 && actorsDistance <= actorStaticState.ecmRange) {
                    LowVisibility.Logger.LogIfDebug($"Target:{CombatantHelper.Label(target)} and ECM source:{CombatantHelper.Label(actor)} are {actorsDistance}m apart, " +
                        $"within of ECM bubble range of:{actorStaticState.ecmRange}");
                    if (actorStaticState.ecmMod > ecmStrength) { ecmStrength = actorStaticState.ecmMod; }
                    ecmSourceCount++;
                }
            }

            if (ecmSourceCount > 1) {
                int multiSourceModifier = (ecmSourceCount - 1) * LowVisibility.Config.MultipleECMSourceModifier;
                LowVisibility.Logger.LogIfDebug($"Target:{CombatantHelper.Label(target)} has:{ecmSourceCount} ECM sources within range. " +
                    $"Additional modifier of:{multiSourceModifier} applied to ecmStrength:{ecmStrength}");
                ecmStrength += multiSourceModifier;
            }

            return ecmStrength;
        }

    }
}
