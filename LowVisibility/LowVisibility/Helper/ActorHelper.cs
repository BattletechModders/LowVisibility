using BattleTech;
using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {
    public class ActorHelper {

        // --- Methods manipulating EWState
        public static EWState GetEWState(AbstractActor actor) {
            return actor != null ? new EWState(actor) : new EWState();
        }

        public static int UpdateSensorCheck(AbstractActor actor) {

            EWState actorState = new EWState(actor);
            int checkResult = ModState.GetCheckResult();
            actor.StatCollection.Set<int>(ModStats.CurrentRoundEWCheck, checkResult);

            Mod.Log.Debug($"Actor:{CombatantUtils.Label(actor)} has raw EW Check: {checkResult}");

            return checkResult;
        }

    }
}
