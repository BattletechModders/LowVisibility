
using BattleTech;
using LowVisibility.Helper;
using LowVisibility.Object;
using LowVisibility.Redzen;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LowVisibility.Helper.MapHelper;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility {
    static class State {

        // -- Const and statics
        public const string ModSaveSubdir = "LowVisibility";
        public const string ModSavesDir = "ModSaves";

        // Map data
        public static MapConfig MapConfig;

        // -- Mutable state
        public static Dictionary<string, DynamicEWState> DynamicEWState = new Dictionary<string, DynamicEWState>();
        public static Dictionary<string, StaticEWState> StaticEWState = new Dictionary<string, StaticEWState>();
        public static Dictionary<string, HashSet<LockState>> SourceActorLockStates = new Dictionary<string, HashSet<LockState>>();
        
        // TODO: Do I need this anymore?
        public static string LastPlayerActivatedActorGUID;

        // -- State related to ECM/effects
        public static Dictionary<string, int> ECMJammedActors = new Dictionary<string, int>();
        public static Dictionary<string, int> ECMProtectedActors = new Dictionary<string, int>();
        public static Dictionary<string, int> NarcedActors = new Dictionary<string, int>();
        public static Dictionary<string, int> TaggedActors = new Dictionary<string, int>();
        
        public static bool TurnDirectorStarted = false;
        public const int ResultsToPrecalcuate = 16384;
        public static double[] CheckResults = new double[ResultsToPrecalcuate];
        public static int CheckResultIdx = 0;

        // --- Methods Below ---
        public static void ClearStateOnCombatGameDestroyed() {
            State.DynamicEWState.Clear();
            State.StaticEWState.Clear();
            State.SourceActorLockStates.Clear();

            State.LastPlayerActivatedActorGUID = null;

            State.ECMJammedActors.Clear();
            State.ECMProtectedActors.Clear();
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
                LowVisibility.Logger.Log($"WARNING: DynamicEWState for actor:{actor.GUID} was not found. Creating!");
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
                List<AbstractActor> playerActors = HostilityHelper.PlayerActors(Combat);
                LastPlayerActivatedActorGUID = playerActors[0].GUID;
            }
            return Combat.FindActorByGUID(LastPlayerActivatedActorGUID);
        }

        // --- ECM JAMMING STATE TRACKING ---
        public static int ECMJamming(AbstractActor actor) {
            return ECMJammedActors.ContainsKey(actor.GUID) ? ECMJammedActors[actor.GUID] : 0;
        }

        public static void AddECMJamming(AbstractActor actor, int modifier) {
            if (!ECMJammedActors.ContainsKey(actor.GUID)) {
                ECMJammedActors.Add(actor.GUID, modifier);
            } else if (modifier > ECMJammedActors[actor.GUID]) {
                ECMJammedActors[actor.GUID] = modifier;
            }            
        }
        public static void RemoveECMJamming(AbstractActor actor) {
            if (ECMJammedActors.ContainsKey(actor.GUID)) {
                ECMJammedActors.Remove(actor.GUID);
            }            
        }

        // --- ECM PROTECTION STATE TRACKING
        public static int ECMProtection(AbstractActor actor) {
            return ECMProtectedActors.ContainsKey(actor.GUID) ? ECMProtectedActors[actor.GUID] : 0;
        }

        public static void AddECMProtection(AbstractActor actor, int modifier) {            
            if (!ECMProtectedActors.ContainsKey(actor.GUID)) {
                ECMProtectedActors.Add(actor.GUID, modifier);
            } else if (modifier > ECMProtectedActors[actor.GUID]) {
                ECMProtectedActors[actor.GUID] = modifier;
            }
        }
        public static void RemoveECMProtection(AbstractActor actor) {
            if (ECMProtectedActors.ContainsKey(actor.GUID)) {
                ECMProtectedActors.Remove(actor.GUID);
            }
        }

        // --- ECM NARC EFFECT
        public static int NARCEffect(AbstractActor actor) {
            return NarcedActors.ContainsKey(actor.GUID) ? NarcedActors[actor.GUID] : 0;
        }

        public static void AddNARCEffect(AbstractActor actor, int modifier) {
            if (!NarcedActors.ContainsKey(actor.GUID)) {
                NarcedActors.Add(actor.GUID, modifier);
            } else if (modifier > NarcedActors[actor.GUID]) {
                NarcedActors[actor.GUID] = modifier;
            }
        }
        public static void RemoveNARCEffect(AbstractActor actor) {
            if (NarcedActors != null && actor != null && NarcedActors.ContainsKey(actor.GUID)) {
                NarcedActors.Remove(actor.GUID);
            }
        }

        // --- ECM TAG EFFECT
        public static int TAGEffect(AbstractActor actor) {
            return TaggedActors.ContainsKey(actor.GUID) ? TaggedActors[actor.GUID] : 0;
        }

        public static void AddTAGEffect(AbstractActor actor, int modifier) {
            if (!TaggedActors.ContainsKey(actor.GUID)) {
                TaggedActors.Add(actor.GUID, modifier);
            } else if (modifier > TaggedActors[actor.GUID]) {
                TaggedActors[actor.GUID] = modifier;
            }
        }
        public static void RemoveTAGEffect(AbstractActor actor) {
            if (TaggedActors != null && actor != null && TaggedActors.ContainsKey(actor.GUID)) {
                TaggedActors.Remove(actor.GUID);
            }
        }

        // --- FILE SAVE/READ BELOW ---
        public class SerializationState {
            public Dictionary<string, DynamicEWState> dynamicState;
            public Dictionary<string, StaticEWState> staticState;
            public Dictionary<string, HashSet<LockState>> SourceActorLockStates;

            public string LastPlayerActivatedActorGUID;

            public Dictionary<string, int> ecmJammedActors;
            public Dictionary<string, int> ecmProtectedActors;
            public Dictionary<string, int> narcedActors;
            public Dictionary<string, int> taggedActors;
        }

        public static void LoadStateData(string saveFileID) {
            ECMJammedActors.Clear();
            ECMProtectedActors.Clear();
            NarcedActors.Clear();
            TaggedActors.Clear();
            SourceActorLockStates.Clear();
            DynamicEWState.Clear();
            StaticEWState.Clear();

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

                    State.DynamicEWState = savedState.dynamicState;
                    LowVisibility.Logger.Log($"  -- DynamicEWState.count: {savedState.dynamicState.Count}");
                    State.StaticEWState = savedState.staticState;
                    LowVisibility.Logger.Log($"  -- StaticEWState.count: {savedState.staticState.Count}");
                    State.SourceActorLockStates = savedState.SourceActorLockStates;
                    LowVisibility.Logger.Log($"  -- SourceActorLockStates.count: {savedState.SourceActorLockStates.Count}");

                    State.LastPlayerActivatedActorGUID = savedState.LastPlayerActivatedActorGUID;
                    LowVisibility.Logger.Log($"  -- LastPlayerActivatedActorGUID: {LastPlayerActivatedActorGUID}");

                    State.ECMJammedActors = savedState.ecmJammedActors;
                    LowVisibility.Logger.Log($"  -- ecmJammedActors.count: {savedState.ecmJammedActors.Count}");
                    State.ECMProtectedActors = savedState.ecmProtectedActors;
                    LowVisibility.Logger.Log($"  -- ecmProtectedActors.count: {savedState.ecmProtectedActors.Count}");
                    State.NarcedActors = savedState.narcedActors;
                    LowVisibility.Logger.Log($"  -- narcedActors.count: {savedState.narcedActors.Count}");
                    State.TaggedActors = savedState.taggedActors;
                    LowVisibility.Logger.Log($"  -- taggedActors.count: {savedState.taggedActors.Count}");

                    LowVisibility.Logger.Log($"Loaded save state from file:{stateFilePath.FullName}.");
                } catch (Exception e) {
                    LowVisibility.Logger.Log($"Failed to read saved state due to e: '{e.Message}'");                    
                }
            } else {
                LowVisibility.Logger.Log($"FilePath:{stateFilePath} does not exist, not loading file.");
            }
        }

        public static void SaveStateData(string saveFileID) {
            string normalizedFileID = saveFileID.Substring(5);
            FileInfo saveStateFilePath = CalculateFilePath(normalizedFileID);
            LowVisibility.Logger.Log($"Saving to filePath:{saveStateFilePath.FullName}.");
            if (saveStateFilePath.Exists) {
                // Make a backup
                saveStateFilePath.CopyTo($"{saveStateFilePath.FullName}.bak", true);
            }

            try {
                SerializationState state = new SerializationState {
                    dynamicState = State.DynamicEWState,
                    staticState = State.StaticEWState,
                    SourceActorLockStates = State.SourceActorLockStates,

                    LastPlayerActivatedActorGUID = State.LastPlayerActivatedActorGUID,

                    ecmJammedActors = State.ECMJammedActors,
                    ecmProtectedActors = State.ECMProtectedActors,
                    narcedActors = State.NarcedActors,
                    taggedActors = State.TaggedActors
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

        private static FileInfo CalculateFilePath(string saveID) {
            // Starting path should be battletech\mods\KnowYourFoe
            DirectoryInfo modsDir = Directory.GetParent(LowVisibility.ModDir);
            DirectoryInfo battletechDir = modsDir.Parent;

            // We want to write to Battletech\ModSaves\<ModName>
            DirectoryInfo modSavesDir = battletechDir.CreateSubdirectory(ModSavesDir);
            DirectoryInfo modSaveSubdir = modSavesDir.CreateSubdirectory(ModSaveSubdir);
            LowVisibility.Logger.Log($"Mod saves will be written to: ({modSaveSubdir.FullName}).");

            //Finally combine the paths
            string campaignFilePath = Path.Combine(modSaveSubdir.FullName, $"{saveID}.json");
            LowVisibility.Logger.Log($"campaignFilePath is: ({campaignFilePath}).");
            return new FileInfo(campaignFilePath);
        }

    }

    
}
