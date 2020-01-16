
using BattleTech;
using BattleTech.Rendering.Mood;
using LowVisibility.Helper;
using System;
using us.frostraptor.modUtils.Redzen;
using static LowVisibility.Helper.MapHelper;

namespace LowVisibility {
    static class ModState {

        private static MapConfig MapConfig;
        private static MoodController MoodController;
        public static AbstractActor LastPlayerActorActivated;
        public static bool TurnDirectorStarted = false;
        public static bool IsNightVisionMode = false;

        // Gaussian probabilities
        public const int ResultsToPrecalcuate = 16384;
        public static double[] CheckResults = new double[ResultsToPrecalcuate];
        public static int CheckResultIdx = 0;

        // --- Methods Below ---
        public static MapConfig GetMapConfig() {
            if (MapConfig == null) {
                MapConfig = MapHelper.ParseCurrentMap();
            }
            return ModState.MapConfig;
        }

        public static void InitMapConfig() {
            MapConfig = MapHelper.ParseCurrentMap();
        }

        public static MoodController GetMoodController() {
            if (MoodController == null) {
                // This is a VERY slow call, that can add 30-40ms just to execute. Cache it!
                ModState.MoodController = UnityEngine.Object.FindObjectOfType<MoodController>();
            }
            return ModState.MoodController;
        }


        // --- Methods manipulating CheckResults
        public static void InitializeCheckResults() {
            Mod.Log.Info($"Initializing a new random buffer of size:{ResultsToPrecalcuate}");
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double mean = Mod.Config.Probability.Mu;
            double stdDev = Mod.Config.Probability.Sigma;
            ZigguratGaussian.Sample(rng, mean, stdDev, CheckResults);
            CheckResultIdx = 0;
        }

        public static int GetCheckResult() {
            if (CheckResultIdx < 0 || CheckResultIdx > ResultsToPrecalcuate) {
                Mod.Log.Info($"ERROR: CheckResultIdx of {CheckResultIdx} is out of bounds! THIS SHOULD NOT HAPPEN!");
            }

            double result = CheckResults[CheckResultIdx];
            CheckResultIdx++;

            // Normalize floats to integer buckets for easier comparison
            if (result > 0) {
                result = Math.Floor(result);
            } else if (result < 0) {
                result = Math.Ceiling(result);
            }

            return (int)result;
        }

        public static void Reset() {
            // Reinitialize state
            MapConfig = null;
            MoodController = null;
            LastPlayerActorActivated = null;
            TurnDirectorStarted = false;
            IsNightVisionMode = false;

            CheckResults = new double[ResultsToPrecalcuate];
            CheckResultIdx = 0;
        }

    }
}
