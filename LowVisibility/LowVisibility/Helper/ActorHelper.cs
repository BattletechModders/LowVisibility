using BattleTech;
using LowVisibility.Object;
using System.Collections.Generic;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {
    public class ActorHelper {

        // --- Methods manipulating EWState
        public static EWState GetEWState(AbstractActor actor) {
            return actor != null ? new EWState(actor) : new EWState();
        }

        public static int UpdateSensorCheck(AbstractActor actor) {

            EWState actorState = new EWState(actor);
            int checkResult = State.GetCheckResult();
            actor.StatCollection.Set<int>(ModStats.CurrentRoundEWCheck, checkResult);

            Mod.Log.Debug($"Actor:{CombatantUtils.Label(actor)} has raw EW Check: {checkResult}");

            return checkResult;
        }

        public static bool IsECMCarrier(AbstractActor actor) {
            List<Effect> list = actor.Combat.EffectManager.GetAllEffectsTargeting(actor)
                .FindAll((Effect x) =>
                    x.EffectData.effectType == EffectType.StatisticEffect
                    && x.EffectData.statisticData.statName == ModStats.ECMCarrier);

            Mod.Log.Debug($" ACTOR HAS ECM: Actor: {CombatantUtils.Label(actor)} hasECM: {list.Count > 0}");
            return list.Count > 0;
        }

    }
}
