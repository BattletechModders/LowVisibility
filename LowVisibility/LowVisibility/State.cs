
using BattleTech;
using LowVisibility.Helper;
using System.Collections.Generic;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility {
    static class State {

        private static float mapVisionRange = 0.0f;

        public static float GetMapVisionRange() {
            if (mapVisionRange == 0) {
                mapVisionRange = MapHelper.CalculateMapVisionRange();
            }
            return mapVisionRange;
        }

        public static Dictionary<string, RoundDetectRange> roundDetectResults = new Dictionary<string, RoundDetectRange>();
        public static RoundDetectRange GetOrCreateRoundDetectResults(AbstractActor actor) {
            if (!roundDetectResults.ContainsKey(actor.GUID)) {
                RoundDetectRange detectRange = MakeSensorRangeCheck(actor);
                roundDetectResults[actor.GUID] = detectRange;
            }
            return roundDetectResults[actor.GUID];
        }

        public static Dictionary<string, ActorEWConfig> actorEWConfig = new Dictionary<string, ActorEWConfig>();
        public static ActorEWConfig GetOrCreateActorEWConfig(AbstractActor actor) {
            if (!actorEWConfig.ContainsKey(actor.GUID)) {
                ActorEWConfig config = CalculateEWConfig(actor);
                actorEWConfig[actor.GUID] = config;
            }
            return actorEWConfig[actor.GUID];
        }
    }
}
