
using BattleTech;
using LowVisibility.Helper;
using LowVisibility.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using us.frostraptor.modUtils.Redzen;
using static LowVisibility.Helper.MapHelper;

namespace LowVisibility {
    static class State {

        // -- Const and statics
        public const string ModSaveSubdir = "LowVisibility";
        public const string ModSavesDir = "ModSaves";

        // Map data
        public static MapConfig MapConfig;

        // -- Mutable state
        public static Dictionary<string, EWState> EWState = new Dictionary<string, EWState>();
        
        // TODO: Do I need this anymore?
        //public static string LastPlayerActor;
        public static AbstractActor LastPlayerActorActivated;

        // -- State related to ECM/effects
        public static Dictionary<string, int> NarcedActors = new Dictionary<string, int>();
        public static Dictionary<string, int> TaggedActors = new Dictionary<string, int>();
        
        public static bool TurnDirectorStarted = false;
        public const int ResultsToPrecalcuate = 16384;
        public static double[] CheckResults = new double[ResultsToPrecalcuate];
        public static int CheckResultIdx = 0;

        // --- Methods Below ---
        public static void ClearStateOnCombatGameDestroyed() {
            State.EWState.Clear();

            State.NarcedActors.Clear();
            State.TaggedActors.Clear();

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
        
        // --- FILE SAVE/READ BELOW ---
        public class SerializationState {
            public Dictionary<string, EWState> staticState;
            
            //public string LastPlayerActivatedActorGUID;

        }

        public static void LoadStateData(string saveFileID) {
            //ECMJammedActors.Clear();
            //ECMProtectedActors.Clear();
            NarcedActors.Clear();
            TaggedActors.Clear();
            EWState.Clear();

            string normalizedFileID = saveFileID.Substring(5);
            FileInfo stateFilePath = CalculateFilePath(normalizedFileID);
            if (stateFilePath.Exists) {
                //LowVisibility.Logger.Log($"Reading saved state from file:{stateFilePath.FullName}.");
                // Read the file
                try {
                    SerializationState savedState = null;
                    using (StreamReader r = new StreamReader(stateFilePath.FullName)) {
                        string json = r.ReadToEnd();
                        //LowVisibility.Logger.Log($"State json is: {json}");
                        savedState = JsonConvert.DeserializeObject<SerializationState>(json);
                    }

                    // TODO: NEED TO REFRESH STATIC STATE ON ACTORS
                    State.EWState = savedState.staticState;
                    Mod.Log.Info($"  -- StaticEWState.count: {savedState.staticState.Count}");

                    Mod.Log.Info($"Loaded save state from file:{stateFilePath.FullName}.");
                } catch (Exception e) {
                    Mod.Log.Info($"Failed to read saved state due to e: '{e.Message}'");                    
                }
            } else {
                Mod.Log.Info($"FilePath:{stateFilePath} does not exist, not loading file.");
            }
        }

        public static void SaveStateData(string saveFileID) {
            string normalizedFileID = saveFileID.Substring(5);
            FileInfo saveStateFilePath = CalculateFilePath(normalizedFileID);
            Mod.Log.Info($"Saving to filePath:{saveStateFilePath.FullName}.");
            if (saveStateFilePath.Exists) {
                // Make a backup
                saveStateFilePath.CopyTo($"{saveStateFilePath.FullName}.bak", true);
            }

            try {
                SerializationState state = new SerializationState {
                    staticState = State.EWState,
                };
                            
                using (StreamWriter w = new StreamWriter(saveStateFilePath.FullName, false)) {
                    string json = JsonConvert.SerializeObject(state);
                    w.Write(json);
                    Mod.Log.Info($"Persisted state to file:{saveStateFilePath.FullName}.");
                }
            } catch (Exception e) {
                Mod.Log.Info($"Failed to persist to disk at path {saveStateFilePath.FullName} due to error: {e.Message}");
            }
        }

        private static FileInfo CalculateFilePath(string saveID) {
            // Starting path should be battletech\mods\KnowYourFoe
            DirectoryInfo modsDir = Directory.GetParent(Mod.ModDir);
            DirectoryInfo battletechDir = modsDir.Parent;

            // We want to write to Battletech\ModSaves\<ModName>
            DirectoryInfo modSavesDir = battletechDir.CreateSubdirectory(ModSavesDir);
            DirectoryInfo modSaveSubdir = modSavesDir.CreateSubdirectory(ModSaveSubdir);
            Mod.Log.Info($"Mod saves will be written to: ({modSaveSubdir.FullName}).");

            //Finally combine the paths
            string campaignFilePath = Path.Combine(modSaveSubdir.FullName, $"{saveID}.json");
            Mod.Log.Info($"campaignFilePath is: ({campaignFilePath}).");
            return new FileInfo(campaignFilePath);
        }

    }

    
}
