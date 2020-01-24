using BattleTech;
using CustomActivatableEquipment;
using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {
    public class ActorHelper {

        // --- Methods manipulating EWState
        public static EWState GetEWState(AbstractActor actor) {
            return actor != null ? new EWState(actor) : new EWState();
        }

        public static int UpdateSensorCheck(AbstractActor actor, bool updateAuras) {

            int checkResult = ModState.GetCheckResult();
            actor.StatCollection.Set<int>(ModStats.CurrentRoundEWCheck, checkResult);
            Mod.Log.Debug($"Actor:{CombatantUtils.Label(actor)} has raw EW Check: {checkResult}");

            if (updateAuras && actor.StatCollection.ContainsStatistic(ModStats.CAESensorsRange)) {
                float sensorsRange = SensorLockHelper.GetSensorsRange(actor);
                actor.StatCollection.Set<float>(ModStats.CAESensorsRange, sensorsRange);

                // TODO: Re-enable once KMission has researched
                //actor.UpdateAuras(false);
            }

            return checkResult;
        }

    }
}
