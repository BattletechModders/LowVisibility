
using BattleTech;
using LowVisibility.Helper;
using LowVisibility.Object;
using LowVisibility.Redzen;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility {
    static class State {

        // -- Const and statics
        public const string ModSaveSubdir = "LowVisibility";
        public const string ModSavesDir = "ModSaves";

        // The vision range of the map
        private static float mapVisionRange = 0.0f;

        // The range at which you can do visualID, modified by the mapVisionRange
        private static float mapVisualScanRange = 0.0f;

        // -- Mutable state
        public static Dictionary<string, DynamicEWState> DynamicEWState = new Dictionary<string, DynamicEWState>();
        public static Dictionary<string, StaticEWState> StaticEWState = new Dictionary<string, StaticEWState>();
        public static Dictionary<string, HashSet<LockState>> SourceActorLockStates = new Dictionary<string, HashSet<LockState>>();
        
        // TODO: Do I need this anymore?
        public static string LastPlayerActivatedActorGUID;
        public static Dictionary<string, int> JammedActors = new Dictionary<string, int>();
        // TODO: Add narc'd actors

        public static bool TurnDirectorStarted = false;
        public const int ResultsToPrecalcuate = 16384;
        public static double[] CheckResults = new double[ResultsToPrecalcuate];
        public static int CheckResultIdx = 0;

        // --- Methods Below ---
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
            return mapVisualScanRange;
        }

        public static void InitMapVisionRange() {
            mapVisionRange = MapHelper.CalculateMapVisionRange();
            mapVisualScanRange = Math.Min(mapVisionRange, LowVisibility.Config.VisualIDRange * 30.0f);
            LowVisibility.Logger.Log($"Vision ranges: calculated map range:{mapVisionRange} configured visualID range:{LowVisibility.Config.VisualIDRange} map visualID range:{mapVisualScanRange}");
        }

        // --- Methods for SourceActorLockStates
        public static HashSet<LockState> GetLocksForLastActivatedPlayerActor(CombatGameState Combat) {
            AbstractActor lastActivatedPlayerActor = GetLastPlayerActivatedActor(Combat);
            if (SourceActorLockStates.ContainsKey(lastActivatedPlayerActor.GUID)) {
                return SourceActorLockStates[lastActivatedPlayerActor.GUID];
            } else {
                // TODO: FIXME
                return null;
            }
        }

        public static LockState GetLockStateForLastActivatedAgainstTarget(AbstractActor target) {
            HashSet<LockState> lockStates = State.GetLocksForLastActivatedPlayerActor(target.Combat);
            LockState lockState = lockStates.First(ls => ls.targetGUID == target.GUID);
            return lockState;
        }

        // --- Methods manipulating DynamicEWState
        public static DynamicEWState GetDynamicState(AbstractActor actor) {
            if (!DynamicEWState.ContainsKey(actor.GUID)) {
                LowVisibility.Logger.Log($"WARNING: DyanmicEWState for actor:{actor.GUID} was not found. Creating!");
                BuildDynamicState(actor);
            }
            return DynamicEWState[actor.GUID];
        }

        public static void BuildDynamicState(AbstractActor actor) {            
            DynamicEWState[actor.GUID] = new DynamicEWState(GetCheckResult(), GetCheckResult());
        }

        // --- Methods manipulating StaticEWState
        public static StaticEWState GetStaticState(AbstractActor actor) {
            if (!StaticEWState.ContainsKey(actor.GUID)) {
                LowVisibility.Logger.Log($"WARNING: StaticEWState for actor:{actor.GUID} was not found. Creating!");
                BuildStaticState(actor);
            }
            return StaticEWState[actor.GUID];
        }

        public static void BuildStaticState(AbstractActor actor) {
            StaticEWState config = new StaticEWState(actor);
            StaticEWState[actor.GUID] = config;
        }

        // --- Methods manipulating CheckResults
        public static void InitializeCheckResults() {
            LowVisibility.Logger.Log($"Initializing a new random buffer of size:{ResultsToPrecalcuate}");
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double mean = LowVisibility.Config.ProbabilityMu;
            double stdDev = LowVisibility.Config.ProbabilitySigma;
            ZigguratGaussian.Sample(rng, mean, stdDev, CheckResults);
            CheckResultIdx = 0;
        }

        public static int GetCheckResult() {
            if (CheckResultIdx < 0 || CheckResultIdx > ResultsToPrecalcuate) {
                LowVisibility.Logger.Log($"ERROR: CheckResultIdx of {CheckResultIdx} is out of bounds! THIS SHOULD NOT HAPPEN!");
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
        
        // The last actor that the player activated. Used to determine visibility in targetingHUD between activations

        public static AbstractActor GetLastPlayerActivatedActor(CombatGameState Combat) {
            if (LastPlayerActivatedActorGUID == null) {
                List<AbstractActor> playerActors = PlayerActors(Combat);
                LastPlayerActivatedActorGUID = playerActors[0].GUID;
            }
            return Combat.FindActorByGUID(LastPlayerActivatedActorGUID);
        }

        // --- ECM JAMMING STATE TRACKING ---
        public static bool IsJammed(AbstractActor actor) {
            bool isJammed = JammedActors.ContainsKey(actor.GUID) ? true : false;
            return isJammed;
        }

        public static int JammingStrength(AbstractActor actor) {
            return JammedActors.ContainsKey(actor.GUID) ? JammedActors[actor.GUID] : 0;
        }

        public static void JamActor(AbstractActor actor, int jammingStrength) {
            DynamicEWState dynamicState = GetDynamicState(actor);
            if (!JammedActors.ContainsKey(actor.GUID)) {
                JammedActors.Add(actor.GUID, jammingStrength);
            } else if (jammingStrength > JammedActors[actor.GUID]) {
                JammedActors[actor.GUID] = jammingStrength;
            }            
        }
        public static void UnjamActor(AbstractActor actor) {
            if (JammedActors.ContainsKey(actor.GUID)) {
                JammedActors.Remove(actor.GUID);
            }            
        }

        // --- FILE SAVE/READ BELOW ---
        private class SerializationState {
            public string LastPlayerActivatedActorGUID;
            public Dictionary<string, int> jammedActors;
            public Dictionary<string, HashSet<LockState>> SourceActorLockStates;
            public Dictionary<string, DynamicEWState> dynamicState;
            public Dictionary<string, StaticEWState> staticState;
        }

        public static void LoadStateData(string saveFileID) {
            JammedActors.Clear();
            SourceActorLockStates.Clear();
            DynamicEWState.Clear();
            StaticEWState.Clear();

            string normalizedFileID = saveFileID.Replace('\\', '_');
            FileInfo stateFilePath = CalculateFilePath(normalizedFileID);
            if (stateFilePath.Exists) {
                //KnowYourFoe.Logger.LogIfDebug($"Reading saved state from file:{campaignFile.FullName}.");
                // Read the file
                try {
                    SerializationState savedState = null;
                    using (StreamReader r = new StreamReader(stateFilePath.FullName)) {
                        string json = r.ReadToEnd();
                        savedState = JsonConvert.DeserializeObject<SerializationState>(json);
                        //KnowYourFoe.Logger.LogIfDebug($"Successfully read state from file:{campaignFile.FullName}.");
                    }

                    LastPlayerActivatedActorGUID = savedState != null ? savedState.LastPlayerActivatedActorGUID : null;
                    JammedActors = savedState != null ? savedState.jammedActors : null;
                    SourceActorLockStates = savedState != null ? savedState.SourceActorLockStates : null;
                    DynamicEWState = savedState != null ? savedState.dynamicState: null;
                    StaticEWState = savedState != null ? savedState.staticState : null;

                    LowVisibility.Logger.Log($"Loaded save state from file:{stateFilePath.FullName}.");
                } catch (Exception e) {
                    LowVisibility.Logger.Log($"Failed to read saved state from:{stateFilePath.FullName} due to e:{e.Message}");                    
                }
            } else {
                //LowVisibility.Logger.Log($"Creating new saved state for campaign seed:{CampaignID}.");
                //// New campaign, create the structure of the file
                //IdentifiedDefs = new Dictionary<string, DetectLevel>();
            }
        }

        public static void SaveStateData(string saveFileID) {
            string normalizedFileID = saveFileID.Replace('\\', '_');
            FileInfo saveStateFilePath = CalculateFilePath(normalizedFileID);
            LowVisibility.Logger.Log($"Saving to filePath:{saveStateFilePath.FullName}.");
            if (saveStateFilePath.Exists) {
                // Make a backup
                saveStateFilePath.CopyTo($"{saveStateFilePath.FullName}.bak", true);
            }

            try {
                SerializationState state = new SerializationState {
                    LastPlayerActivatedActorGUID = State.LastPlayerActivatedActorGUID,
                    jammedActors = State.JammedActors,
                    SourceActorLockStates = State.SourceActorLockStates,
                    dynamicState= State.DynamicEWState,
                    staticState = State.StaticEWState
                };
                            
                using (StreamWriter w = new StreamWriter(saveStateFilePath.FullName, false)) {
                    string json = JsonConvert.SerializeObject(state);
                    w.Write(json);
                    LowVisibility.Logger.Log($"Persisted state to file:{saveStateFilePath.FullName}.");
                }
            } catch (Exception e) {
                LowVisibility.Logger.Log($"Failed to persist to disk at path {saveStateFilePath.FullName} due to error: {e.Message}");
            }
        }

        private static FileInfo CalculateFilePath(string campaignId) {
            // Starting path should be battletech\mods\KnowYourFoe
            string[] directories = LowVisibility.ModDir.Split(Path.DirectorySeparatorChar);
            DirectoryInfo modsDir = Directory.GetParent(LowVisibility.ModDir);
            DirectoryInfo battletechDir = modsDir.Parent;

            // We want to write to Battletech\ModSaves\KnowYourFoe
            DirectoryInfo modSavesDir = battletechDir.CreateSubdirectory(ModSavesDir);
            DirectoryInfo modSaveSubdir = modSavesDir.CreateSubdirectory(ModSaveSubdir);

            // Finally check to see if the file exists
            string campaignFilePath = Path.Combine(modSaveSubdir.FullName, $"{campaignId}.json");
            return new FileInfo(campaignFilePath);
        }

    }

    
}
