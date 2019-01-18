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

            int jamming = CalculateECMStrength(source, hostiles);
            if (jamming > 0) {
                State.AddECMJamming(source, jamming);
            } else {
                State.RemoveECMJamming(source);
            }

            int protection = CalculateECMStrength(source, friendlies);
            if (protection > 0) {
                State.AddECMProtection(source, protection);
            } else {
                State.RemoveECMProtection(source);
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
