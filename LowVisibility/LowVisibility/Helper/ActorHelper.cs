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
            int checkResult = State.GetCheckResult();
            actor.StatCollection.Set<int>(ModStats.Check, checkResult);
            Mod.Log.Debug($" Set SensorCheck: {checkResult} for Actor:{CombatantUtils.Label(actor)}");
            return checkResult;
        }

    }
}
