using BattleTech;
using CustomActivatableEquipment;
using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {
    public static class ActorHelper {

        // --- Methods manipulating EWState
        public static EWState GetEWState(this AbstractActor actor) {
            if (EWState.InBatchProcess) {
                if (EWState.EWStateCache.TryGetValue(actor, out EWState state))
                    return state;

                return EWState.EWStateCache[actor] = new EWState(actor);
            }

            return new EWState(actor);
        }

        public static int UpdateSensorCheck(AbstractActor actor, bool updateAuras) {

            int checkResult = ModState.GetCheckResult();
            actor.StatCollection.Set<int>(ModStats.CurrentRoundEWCheck, checkResult);
            Mod.ActorStateLog.Info?.Write($"Actor:{CombatantUtils.Label(actor)} has raw EW Check: {checkResult}");

            if (updateAuras && actor.StatCollection.ContainsStatistic(ModStats.CAESensorsRange)) {
                float sensorsRange = SensorLockHelper.GetSensorsRange(actor);
                actor.StatCollection.Set<float>(ModStats.CAESensorsRange, sensorsRange);

                // TODO: Re-enable once KMission has researched
                actor.UpdateAuras(false);
            }

            return checkResult;
        }

    }
}
