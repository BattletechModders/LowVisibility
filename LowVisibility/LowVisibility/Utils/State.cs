
using BattleTech;
using LowVisibility.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LowVisibility.Helper.ActorHelper;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility {
    static class State {

        public const string ModSaveSubdir = "LowVisibility";
        public const string ModSavesDir = "ModSaves";

        // The vision range of the map
        private static float mapVisionRange = 0.0f;

        // The range at which you can do visualID
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

        public static Dictionary<string, RoundDetectRange> RoundDetectResults = new Dictionary<string, RoundDetectRange>();
        public static Dictionary<string, ActorEWConfig> ActorEWConfig = new Dictionary<string, ActorEWConfig>();
        public static Dictionary<string, HashSet<LockState>> SourceActorLockStates = new Dictionary<string, HashSet<LockState>>();
        public static string LastPlayerActivatedActorGUID;
        public static Dictionary<string, int> JammedActors = new Dictionary<string, int>();


        public static RoundDetectRange GetOrCreateRoundDetectResults(AbstractActor actor) {
            if (!RoundDetectResults.ContainsKey(actor.GUID)) {
                RoundDetectRange detectRange = MakeSensorRangeCheck(actor);
                RoundDetectResults[actor.GUID] = detectRange;
            }
            return RoundDetectResults[actor.GUID];
        }

        public static ActorEWConfig GetOrCreateActorEWConfig(AbstractActor actor) {
            if (!ActorEWConfig.ContainsKey(actor.GUID)) {
                ActorEWConfig config = new ActorEWConfig(actor);
                ActorEWConfig[actor.GUID] = config;
            }
            return ActorEWConfig[actor.GUID];
        }

        // Updates the detection state for all units. Called at start of round, and after each enemy movement.
        public static void UpdateDetectionForAllActors(CombatGameState Combat) {

            HashSet<string> targetsWithVisibilityChanges = new HashSet<string>();

            List<AbstractActor> enemyAndNeutralActors = EnemyAndNeutralActors(Combat);
            List<AbstractActor> playerAndAlliedActors = PlayerAndAlliedActors(Combat);

            // Update friendlies
            LowVisibility.Logger.LogIfDebug($"=== Updating DetectionOnRoundBegin for PlayerAndAllies");
            foreach (AbstractActor source in playerAndAlliedActors) {                
                HashSet<LockState> updatedLocks = new HashSet<LockState>();
                CalculateTargetLocks(source, enemyAndNeutralActors, updatedLocks, targetsWithVisibilityChanges);                
                SourceActorLockStates[source.GUID] = updatedLocks;
            }

            // Update foes
            LowVisibility.Logger.LogIfDebug($"=== Updating DetectionOnRoundBegin for FoesAndNeutral");
            foreach (AbstractActor source in enemyAndNeutralActors) {
                HashSet<LockState> updatedLocks = new HashSet<LockState>();
                CalculateTargetLocks(source, playerAndAlliedActors, updatedLocks, targetsWithVisibilityChanges);
                SourceActorLockStates[source.GUID] = updatedLocks;
            }

            // Send a message updating the visibility of any actors that changed
            PublishVisibilityChange(Combat, targetsWithVisibilityChanges);
        }
       
        // Updates the detection state for a friendly actor. Call before activation, and after friendly movement.
        public static void UpdateActorDetection(AbstractActor source) {

            HashSet<string> targetsWithVisibilityChanges = new HashSet<string>();

            // TODO: Should calculate friendly and enemy changes on each iteration of this

            bool isPlayer = source.team == source.Combat.LocalPlayerTeam;
            List<AbstractActor> targets = isPlayer ? EnemyAndNeutralActors(source.Combat) : PlayerAndAlliedActors(source.Combat);
            HashSet<LockState> updatedLocks = new HashSet<LockState>();
            LowVisibility.Logger.LogIfDebug($"=== Updating ActorDetection for source:{ActorLabel(source)} which isPlayer:{isPlayer} for target count:{targets.Count}");
            CalculateTargetLocks(source, targets, updatedLocks, targetsWithVisibilityChanges);
            SourceActorLockStates[source.GUID] = updatedLocks;

            // Send a message updating the visibility of any actors that changed
            //PublishVisibilityChange(source.Combat, targetsWithVisibilityChanges); 

            // TESTING: This works better, but doesn't do it on activation for some reason?
            // ALSO: APPLIES TO BOTH PLAYER AND ENEMY - only update enemies!
            if (isPlayer) {
                foreach (string targetGUID in targetsWithVisibilityChanges) {
                    AbstractActor target = source.Combat.FindActorByGUID(targetGUID);
                    LockState targetLockState = GetUnifiedLockStateForTarget(source, target);
                    VisibilityLevel targetVisLevel = VisibilityLevel.None;
                    if (targetLockState.visionType != VisionLockType.None && targetLockState.sensorType == SensorLockType.None) {                        
                        targetVisLevel = VisibilityLevel.LOSFull;
                        LowVisibility.Logger.Log($"Only vision lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    } else if (targetLockState.visionType == VisionLockType.None && targetLockState.sensorType != SensorLockType.None) {
                        // TODO: This really should account for the tactics of the shared vision
                        targetVisLevel = VisibilityLevelByTactics(source.GetPilot().Tactics);
                        LowVisibility.Logger.Log($"Only sensor lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    } else {
                        LowVisibility.Logger.Log($"No vision or sensor lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    }
                    LowVisibility.Logger.Log($"Setting actor:{ActorLabel(target)} visibility to:{targetVisLevel}");
                    target.OnPlayerVisibilityChanged(targetVisLevel);
                }
            }
        }

        public static LockState GetUnifiedLockStateForTarget(AbstractActor source, AbstractActor target) {
            // Get the source lock state first            
            if (!SourceActorLockStates.ContainsKey(source.GUID)) {
                LowVisibility.Logger.LogIfDebug($"----- source:{source.GUID} is missing, updating their detection state.");
                UpdateActorDetection(source);
            }
            //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Looking for sourceLocks");
            HashSet<LockState> sourceLocks = SourceActorLockStates[source.GUID];
            LockState sourceLockState = sourceLocks.First(ls => ls.targetGUID == target.GUID);
            LockState unifiedLockState = new LockState(sourceLockState);

            // Finally check for shared visibility
            //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Looking for shared vision");
            bool isPlayer = source.team == source.Combat.LocalPlayerTeam;
            List<AbstractActor> friendlies = isPlayer ? PlayerAndAlliedActors(source.Combat) : EnemyAndNeutralActors(source.Combat);
            foreach (AbstractActor friendly in friendlies) {
                if (SourceActorLockStates.ContainsKey(friendly.GUID)) {
                    //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Checking shared vision for actor:{ActorLabel(friendly)}");
                    HashSet<LockState> friendlyLocks = SourceActorLockStates[friendly.GUID];
                    LockState friendlyLockState = friendlyLocks.First(ls => ls.targetGUID == target.GUID);

                    // Vision is always shared
                    if (friendlyLockState.visionType > unifiedLockState.visionType) {
                        //LowVisibility.Logger.LogIfDebug($"friendly:{ActorLabel(friendly)} has a superior vision lock to target:{ActorLabel(target)}, using their lock.");
                        unifiedLockState.visionType = friendlyLockState.visionType;
                    }

                    // Sensors are shared conditionally
                    ActorEWConfig friendlyEWConfig = GetOrCreateActorEWConfig(friendly);
                    if (friendlyLockState.sensorType > unifiedLockState.sensorType && friendlyEWConfig.sharesSensors) {
                        unifiedLockState.sensorType = friendlyLockState.sensorType;
                        //LowVisibility.Logger.LogIfDebug($"friendly:{ActorLabel(friendly)} shares sensors and has a superior sensor lock to target:{ActorLabel(target)}, using their lock.");
                    }
                }
            }

            return unifiedLockState;
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
            if (!JammedActors.ContainsKey(actor.GUID)) {
                JammedActors.Add(actor.GUID, jammingStrength);

                // Send a floatie indicating the jamming
                MessageCenter mc = actor.Combat.MessageCenter;
                mc.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "JAMMED BY ECM", FloatieMessage.MessageNature.Debuff));

                //
            } else if (jammingStrength > JammedActors[actor.GUID]) {
                JammedActors[actor.GUID] = jammingStrength;
            }
            // Send visibility update message
        }
        public static void UnjamActor(AbstractActor actor) {
            if (JammedActors.ContainsKey(actor.GUID)) {
                JammedActors.Remove(actor.GUID);
            }
            // Send visibility update message
        }

        // --- FILE SAVE/READ BELOW ---
        private class SerializationState {
            public string LastPlayerActivatedActorGUID;
            public Dictionary<string, int> jammedActors;
            public Dictionary<string, HashSet<LockState>> SourceActorLockStates;
            public Dictionary<string, RoundDetectRange> roundDetectResults;
            public Dictionary<string, ActorEWConfig> actorEWConfig;
        }

        public static void LoadStateData(string saveFileID) {
            JammedActors.Clear();
            SourceActorLockStates.Clear();
            RoundDetectResults.Clear();
            ActorEWConfig.Clear();

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
                    RoundDetectResults = savedState != null ? savedState.roundDetectResults : null;
                    ActorEWConfig = savedState != null ? savedState.actorEWConfig : null;

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
                    roundDetectResults = State.RoundDetectResults,
                    actorEWConfig = State.ActorEWConfig
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
