
using BattleTech;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility {
    static class State {

        private static float mapVisionRange = 0.0f;
        private static float visualIDRange = 0.0f;

        public static float GetMapVisionRange() {
            if (mapVisionRange == 0) {
                InitMapVisionRange();
            }
            return mapVisionRange;
        }

        public static float GetVisualIDRange() {
            if (mapVisionRange == 0) {
                InitMapVisionRange();
            }
            return visualIDRange;
        }

        private static void InitMapVisionRange() {
            mapVisionRange = MapHelper.CalculateMapVisionRange();
            visualIDRange = Math.Min(mapVisionRange, LowVisibility.Config.VisualIDRange);
            LowVisibility.Logger.Log($"Vision ranges: calculated map range:{mapVisionRange} configured visualID range:{LowVisibility.Config.VisualIDRange} map visualID range:{visualIDRange}");
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

        // --- ECM JAMMING STATE TRACKING ---
        public static Dictionary<string, int> jammedActors = new Dictionary<string, int>();

        public static bool IsJammed(AbstractActor actor) {
            bool isJammed = jammedActors.ContainsKey(actor.GUID) ? true : false;
            return isJammed;
        }

        public static int JammingStrength(AbstractActor actor) {
            return jammedActors.ContainsKey(actor.GUID) ? jammedActors[actor.GUID] : 0;
        }

        public static void JamActor(AbstractActor actor, int jammingStrength) {
            if (!jammedActors.ContainsKey(actor.GUID)) {
                jammedActors.Add(actor.GUID, jammingStrength);
            } else if (jammingStrength > jammedActors[actor.GUID]) {
                jammedActors[actor.GUID] = jammingStrength;
            }
            // Send visibility update message
        }
        public static void UnjamActor(AbstractActor actor) {
            if (jammedActors.ContainsKey(actor.GUID)) {
                jammedActors.Remove(actor.GUID);
            }
            // Send visibility update message
        }
    }
}
