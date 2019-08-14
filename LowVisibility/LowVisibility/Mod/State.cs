
using BattleTech;
using LowVisibility.Helper;
using System;
using us.frostraptor.modUtils.Redzen;
using static LowVisibility.Helper.MapHelper;

namespace LowVisibility {
    static class State {

        // -- Const and statics
        public const string ModSaveSubdir = "LowVisibility";
        public const string ModSavesDir = "ModSaves";

        // Map data
        public static MapConfig MapConfig;

        // TODO: Do I need this anymore?
        //public static string LastPlayerActor;
        public static AbstractActor LastPlayerActorActivated;

        public static bool TurnDirectorStarted = false;
        public const int ResultsToPrecalcuate = 16384;
        public static double[] CheckResults = new double[ResultsToPrecalcuate];
        public static int CheckResultIdx = 0;

        // --- Methods Below ---
        public static void ClearStateOnCombatGameDestroyed() {
            State.TurnDirectorStarted = false;
        }

        public static float GetMapVisionRange() {
            if (MapConfig == null) {
                InitMapConfig();
            }
            return MapConfig == null ? 0.0f : MapConfig.visionRange;
        }

        public static float GetVisualIDRange() {
            if (MapConfig == null) {
                InitMapConfig();
            }
            return MapConfig == null ? 0.0f : MapConfig.scanRange;
        }

        public static void InitMapConfig() {
            MapConfig = MapHelper.ParseCurrentMap();
        }

        // --- Methods manipulating CheckResults
        public static void InitializeCheckResults() {
            Mod.Log.Info($"Initializing a new random buffer of size:{ResultsToPrecalcuate}");
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double mean = Mod.Config.ProbabilityMu;
            double stdDev = Mod.Config.ProbabilitySigma;
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

    }
}
