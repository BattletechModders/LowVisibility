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
            foreach (AbstractActor enemy in source.Combat.GetAllEnemiesOf(source)) {
                StaticEWState enemyStaticState = State.GetStaticState(enemy);
                float actorsDistance = Vector3.Distance(source.CurrentPosition, enemy.CurrentPosition);

                // If the enemy has ECM, jam the source                
                if (enemyStaticState.ecmMod != 0 && actorsDistance <= enemyStaticState.ecmRange) {
                    LowVisibility.Logger.LogIfDebug($"Source:{ActorLabel(source)} and target:{ActorLabel(enemy)} are {actorsDistance}m apart, " +
                        $"within of ECM bubble range of:{enemyStaticState.ecmRange}");
                    if (enemyStaticState.ecmMod > jammingStrength) { jammingStrength = enemyStaticState.ecmMod; }
                    numJammers++;
                }

                // TODO: This seems redundant; if I check everytime at start of actor activation/movement it should naturally occur
                // If the source has ECM, jam the target 
                //if (sourceStaticState.ecmTier > -1 && enemyStaticState.probeTier < sourceStaticState.ecmTier) {
                //    if (actorsDistance > sourceStaticState.ecmRange) {
                //        LowVisibility.Logger.Log($"Actors are {actorsDistance}m apart, outside of ECM bubble range of:{sourceStaticState.ecmRange}");
                //    } else {
                //        LowVisibility.Logger.Log($"Enemy:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name} is within ECM bubble of source actor:{source.DisplayName}_{source.GetPilot().Name} .");
                //        State.JamActor(enemyActor, sourceStaticState.ecmModifier);
                //    }
                //}

            }

            if (numJammers > 1) {
                int additionalPenalty = (numJammers - 1) * LowVisibility.Config.MultipleJammerAdditionalPenalty;
                LowVisibility.Logger.LogIfDebug($"Source:{ActorLabel(source)} has:{numJammers} jammers within range. " +
                    $"Additional penalty:{additionalPenalty} applied to jammingStrength:{jammingStrength}");
                jammingStrength += additionalPenalty;
            }

            if (jammingStrength > 0) {
                State.JamActor(source, jammingStrength);
                // Send a floatie indicating the jamming
                MessageCenter mc = source.Combat.MessageCenter;
                mc.PublishMessage(new FloatieMessage(source.GUID, source.GUID, "JAMMED BY ECM", FloatieMessage.MessageNature.Debuff));
            } else {
                State.UnjamActor(source);
            }
        }

        private static object ActorLabel(AbstractActor source) {
            throw new NotImplementedException();
        }
    }
}
