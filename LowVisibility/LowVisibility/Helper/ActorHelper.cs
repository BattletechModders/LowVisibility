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

            int tacticsModifier = 0;
            if (actor.StatCollection.ContainsStatistic(ModStats.TacticsMod)) {
                tacticsModifier = actor.StatCollection.GetStatistic(ModStats.TacticsMod).Value<int>();
            } else {
                tacticsModifier = SkillUtils.GetTacticsModifier(actor.GetPilot());
                actor.StatCollection.Set<int>(ModStats.TacticsMod, tacticsModifier);
            }

            int probeModifier = 0;
            if (actor.StatCollection.ContainsStatistic(ModStats.Probe)) {
                probeModifier = actor.StatCollection.GetStatistic(ModStats.Probe).Value<int>();
            }

            int checkResult = State.GetCheckResult();

            int sensorCheck = checkResult + tacticsModifier + probeModifier;
            Mod.Log.Debug($" SensorCheck: {sensorCheck} = checkResult: {checkResult} + " +
                $"tacticsMod: {tacticsModifier} + probeMod: {probeModifier} for Actor:{CombatantUtils.Label(actor)}");

            actor.StatCollection.Set<int>(ModStats.SensorCheck, sensorCheck);
            Mod.Log.Debug($" Set SensorCheck: {sensorCheck}");

            return checkResult;
        }

    }
}
