
using BattleTech;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using static LowVisibility.Helper.ActorHelper;
using static LowVisibility.Helper.VisibilityHelper;

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

        // TODO: Add tracking
        //public static Dictionary<string, VisionLockType> 
        public static Dictionary<string, HashSet<LockState>> SourceActorLockStates = new Dictionary<string, HashSet<LockState>>();

        // Updates the detection state for all units. Called at start of round, and after each enemy movement.
        public static void UpdateDetectionOnRoundBegin(CombatGameState Combat) {

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

            bool isPlayer = source.team == source.Combat.LocalPlayerTeam;
            List<AbstractActor> targets = isPlayer ? EnemyAndNeutralActors(source.Combat) : PlayerAndAlliedActors(source.Combat);
            HashSet<LockState> updatedLocks = new HashSet<LockState>();
            LowVisibility.Logger.LogIfDebug($"=== Updating ActorDetection for source:{ActorLabel(source)} which isPlayer:{isPlayer} for target count:{targets.Count}");
            CalculateTargetLocks(source, targets, updatedLocks, targetsWithVisibilityChanges);
            SourceActorLockStates[source.GUID] = updatedLocks;

            // Send a message updating the visibility of any actors that changed
            PublishVisibilityChange(source.Combat, targetsWithVisibilityChanges);
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
        public static AbstractActor LastPlayerActivatedActor;
        public static AbstractActor GetLastPlayerActivatedActor(CombatGameState Combat) {
            if (LastPlayerActivatedActor == null) {
                List<AbstractActor> playerActors = PlayerActors(Combat);
                LastPlayerActivatedActor = playerActors[0];
            }
            return LastPlayerActivatedActor;
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

                // Send a floatie indicating the jamming
                MessageCenter mc = actor.Combat.MessageCenter;
                mc.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "JAMMED BY ECM", FloatieMessage.MessageNature.Debuff));

                //
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
