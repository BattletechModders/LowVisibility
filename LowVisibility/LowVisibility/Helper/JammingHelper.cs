using System;
using BattleTech;
using LowVisibility.Object;
using UnityEngine;

namespace LowVisibility.Helper {
    class JammingHelper {
        public static void ResolveJammingState(AbstractActor source) {

            StaticEWState sourceStaticState = State.GetStaticState(source);
            int jammingStrength = 0;
            int numJammers = 0;
            foreach(AbstractActor actor in source.Combat.AllActors) {
                if (source.Combat.HostilityMatrix.IsLocalPlayerEnemy(actor.TeamId) && !actor.IsTeleportedOffScreen && !actor.IsDead && !actor.IsFlaggedForDeath) {
                    StaticEWState enemyStaticState = State.GetStaticState(actor);
                    float actorsDistance = Vector3.Distance(source.CurrentPosition, actor.CurrentPosition);

                    // If the enemy has ECM, jam the source                
                    if (enemyStaticState.ecmMod != 0 && actorsDistance <= enemyStaticState.ecmRange) {
                        LowVisibility.Logger.LogIfDebug($"Source:{CombatantHelper.Label(source)} and target:{CombatantHelper.Label(actor)} are {actorsDistance}m apart, " +
                            $"within of ECM bubble range of:{enemyStaticState.ecmRange}");
                        if (enemyStaticState.ecmMod > jammingStrength) { jammingStrength = enemyStaticState.ecmMod; }
                        numJammers++;
                    }
                }
            }

            if (numJammers > 1) {
                int additionalPenalty = (numJammers - 1) * LowVisibility.Config.MultipleJammerPenalty;
                LowVisibility.Logger.LogIfDebug($"Source:{CombatantHelper.Label(source)} has:{numJammers} jammers within range. " +
                    $"Additional penalty:{additionalPenalty} applied to jammingStrength:{jammingStrength}");
                jammingStrength += additionalPenalty;
            }

            if (jammingStrength > 0) {
                State.JamActor(source, jammingStrength);
            } else {
                State.UnjamActor(source);
            }
        }

    }
}
