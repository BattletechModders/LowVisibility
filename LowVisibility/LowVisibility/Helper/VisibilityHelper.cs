using BattleTech;
using LowVisibility.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Helper {

    public static class VisibilityHelper {

        public static DetectionLevel DetectionLevelForCheck(int checkResult) {
            DetectionLevel level = DetectionLevel.NoInfo;
            if (checkResult == -1) {
                level = DetectionLevel.Location;
            } else if (checkResult == 0) {
                level = DetectionLevel.Type;
            } else if (checkResult == 1) {
                level = DetectionLevel.Silhouette;
            } else if (checkResult == 2) {
                level = DetectionLevel.Vector;
            } else if (checkResult == 3 || checkResult == 4) {
                level = DetectionLevel.SurfaceScan;
            } else if (checkResult == 5 || checkResult == 6) {
                level = DetectionLevel.SurfaceAnalysis;
            } else if (checkResult == 7) {
                level = DetectionLevel.WeaponAnalysis;
            } else if (checkResult == 8) {
                level = DetectionLevel.StructureAnalysis;
            } else if (checkResult == 9) {
                level = DetectionLevel.DeepScan;
            } else if (checkResult >= 10) {
                level = DetectionLevel.DentalRecords;
            }
            return level;
        }

        public static List<AbstractActor> PlayerActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => aa.TeamId == Combat.LocalPlayerTeamGuid)
                .ToList();
        }

        public static List<AbstractActor> PlayerAndAlliedActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => aa.TeamId == Combat.LocalPlayerTeamGuid 
                    || Combat.HostilityMatrix.IsFriendly(Combat.LocalPlayerTeamGuid, aa.TeamId))
                .ToList();
        }

        public static List<AbstractActor> EnemyAndNeutralActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => Combat.HostilityMatrix.IsEnemy(Combat.LocalPlayerTeamGuid, aa.TeamId) 
                    || Combat.HostilityMatrix.IsNeutral(Combat.LocalPlayerTeamGuid, aa.TeamId))
                .ToList();
        }

        public static void CalculateTargetLocks(AbstractActor source, List<AbstractActor> targets, 
            HashSet<LockState> updatedLocks, HashSet<string> visibilityUpdates) {

            foreach (AbstractActor target in targets) {
                if (target.GUID == source.GUID) { continue;  }

                LockState lockState = CalculateLock(source, target);
                LowVisibility.Logger.LogIfDebug($"Updated lockState for source:{ActorLabel(source)} vs. target:{ActorLabel(target)} is lockState:{lockState}");
                updatedLocks.Add(lockState);

                // Check for update
                HashSet<LockState> currentLocks = State.SourceActorLockStates.ContainsKey(source.GUID) ? 
                    State.SourceActorLockStates[source.GUID] : new HashSet<LockState>();
                LockState currentLockState = currentLocks.FirstOrDefault(ls => ls.targetGUID == target.GUID);
                if (currentLockState == null || 
                    currentLockState.visionLockLevel != lockState.visionLockLevel || 
                    currentLockState.sensorLockLevel != lockState.sensorLockLevel) {
                    LowVisibility.Logger.LogIfDebug($"Target:{ActorLabel(target)} lockState changed, addings to refresh targets.");
                    visibilityUpdates.Add(target.GUID);
                }
            }
        }


        // Calculate the lock level between a source and target actor
        public static LockState CalculateLock(AbstractActor source, AbstractActor target) {

            LockState newLockState = new LockState {
                sourceGUID = source.GUID,
                targetGUID = target.GUID,
                visionLockLevel = VisionLockType.None,
                sensorLockLevel = DetectionLevel.NoInfo,
            };            

            // --- Determine visual lock 
            VisibilityLevelAndAttribution visLevelAndAttrib = source.VisibilityCache.VisibilityToTarget(target);

            // If we have LOS to the target, we at least have their silhouette
            if (visLevelAndAttrib.VisibilityLevel == VisibilityLevel.LOSFull) {
                newLockState.visionLockLevel = VisionLockType.Silhouette;
            }

            float distance = Vector3.Distance(source.CurrentPosition, target.CurrentPosition);
            float targetVisibility = CalculateTargetVisibility(target);

            float pilotVisualLockRange = GetVisualIDRangeForActor(source);
            float visualLockRange = pilotVisualLockRange * targetVisibility;

            if (distance <= visualLockRange) { newLockState.visionLockLevel = VisionLockType.VisualID; }

            // TODO: Check for failed visual lock and adjust appropriately. Requires visionLockLevel to change to DetectionLevel
            LowVisibility.Logger.LogIfDebug($"  -- source:{ActorLabel(source)} has visualLockRange:{visualLockRange} and is distance:{distance} " +
                $"from target:{ActorLabel(target)} with visibility:{targetVisibility} - visionLockType:{newLockState.visionLockLevel}");

            // -- Determine sensor lock            
            StaticEWState sourceStaticState = State.GetStaticState(source);
            DynamicEWState sourceDynamicState = State.GetDynamicState(source);
            int modifiedSourceCheck = sourceDynamicState.currentCheck;
            LowVisibility.Logger.LogIfDebug($"  -- source actor:{ActorLabel(source)} has dynamicState:{sourceDynamicState}");

            // --- Source modifiers: ECM, Active Probe, SensorBoost tag
            // Check for ECM strength
            if (State.IsJammed(source)) {
                modifiedSourceCheck -= State.JammingStrength(source);
                LowVisibility.Logger.LogIfDebug($"  -- source actor:{ActorLabel(source)} is jammed with strength:{State.JammingStrength(source)}, " +
                    $"reducing sourceCheckResult to:{modifiedSourceCheck}");
            }

            if (sourceStaticState.probeMod != 0) {
                modifiedSourceCheck += sourceStaticState.probeMod;
                LowVisibility.Logger.LogIfDebug($"  -- source actor:{ActorLabel(source)} has probe with strength:{sourceStaticState.probeMod}, " +
                    $"increasing sourceCheckResult to:{modifiedSourceCheck}");
            }

            if (sourceStaticState.sensorMod != 0) {
                modifiedSourceCheck += sourceStaticState.probeMod;
                LowVisibility.Logger.LogIfDebug($"  -- source actor:{ActorLabel(source)} has sensorMod with strength:{sourceStaticState.sensorMod}, " +
                    $"increasing sourceCheckResult to:{modifiedSourceCheck}");
            }

            // --- Target Modifiers: Stealth, Narc, Tag
            StaticEWState targetStaticState = State.GetStaticState(target);

            // Check for target stealth
            if (targetStaticState.stealthMod != 0) {
                modifiedSourceCheck -= targetStaticState.stealthMod;
                LowVisibility.Logger.LogIfDebug($"  -- target actor:{ActorLabel(target)} has stealthModifier:{targetStaticState.stealthMod}, " +
                    $"reducing sourceCheckResult to:{modifiedSourceCheck}");
            }

            // TODO: Check for a Narc effect
            // TODO: Check for a Tag effect

            // Determine the final lockLevelCheck
            newLockState.sensorLockLevel = VisibilityHelper.DetectionLevelForCheck(modifiedSourceCheck);

            // If they fail their check, they get no sensor range
            float sourceSensorsRange = CalculateSensorRange(source);
            float sourceSensorLockRange = newLockState.sensorLockLevel != DetectionLevel.NoInfo ? sourceSensorsRange : 0.0f;
            LowVisibility.Logger.LogIfDebug($"  -- source actor:{ActorLabel(source)} has " +
                $"sensorRange:{sourceSensorsRange} and lockRange:{sourceSensorLockRange}");

            // Finally, check for range
            float targetSignature = CalculateTargetSignature(target);
            float sensorLockRange = sourceSensorLockRange * targetSignature;
            if (distance > sourceSensorLockRange) {
                newLockState.sensorLockLevel = DetectionLevel.NoInfo;
            }
            LowVisibility.Logger.Log($"  -- source:{ActorLabel(source)} has sensorsRange:{sensorLockRange} and is distance:{distance} " +
                $"from target:{ActorLabel(target)} with signature:{targetSignature} - sensorLockType:{newLockState.sensorLockLevel}");

            return newLockState;
        }

        // Updates the detection state for all units. Called at start of round, and after each enemy movement.
        public static void UpdateDetectionForAllActors(CombatGameState Combat, AbstractActor updateSource = null) {

            HashSet<string> targetsWithVisibilityChanges = new HashSet<string>();

            List<AbstractActor> enemyAndNeutralActors = EnemyAndNeutralActors(Combat);
            List<AbstractActor> playerAndAlliedActors = PlayerAndAlliedActors(Combat);

            // Update friendlies
            LowVisibility.Logger.LogIfDebug($"  ==== Updating PlayerAndAllies ====");
            foreach (AbstractActor source in playerAndAlliedActors) {
                HashSet<LockState> updatedLocks = new HashSet<LockState>();
                CalculateTargetLocks(source, enemyAndNeutralActors, updatedLocks, targetsWithVisibilityChanges);
                State.SourceActorLockStates[source.GUID] = updatedLocks;
            }

            // Update foes
            LowVisibility.Logger.LogIfDebug($"  ==== Updating FoesAndNeutral ====");
            foreach (AbstractActor source in enemyAndNeutralActors) {
                HashSet<LockState> updatedLocks = new HashSet<LockState>();
                CalculateTargetLocks(source, playerAndAlliedActors, updatedLocks, targetsWithVisibilityChanges);
                State.SourceActorLockStates[source.GUID] = updatedLocks;
            }

            // Send a message updating the visibility of any actors that changed
            LowVisibility.Logger.LogIfDebug($"  ==== Updating visibility on changed actors ====");
            foreach (string targetGUID in targetsWithVisibilityChanges) {
                AbstractActor target = Combat.FindActorByGUID(targetGUID);
                LowVisibility.Logger.Log($"  -- Updating vis on actor:{ActorLabel(target)}");
                target.UpdateVisibilityCache(Combat.GetAllCombatants());

                bool isPlayer = target.TeamId == Combat.LocalPlayerTeamGuid;
                if (updateSource != null && !isPlayer) {
                    LockState targetLockState = GetUnifiedLockStateForTarget(updateSource, target);
                    LowVisibility.Logger.Log($"  -- Unified lockState on actor:{ActorLabel(target)} from source:{ActorLabel(updateSource)} is now:{targetLockState}");
                    VisibilityLevel targetVisLevel = VisibilityLevel.None;

                    if (targetLockState.visionLockLevel != VisionLockType.None) {
                        targetVisLevel = VisibilityLevel.LOSFull;
                        LowVisibility.Logger.Log($"Visual lock actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    } else if (targetLockState.visionLockLevel == VisionLockType.None && targetLockState.sensorLockLevel != DetectionLevel.NoInfo) {
                        // TODO: This really should account for the tactics of the shared vision
                        int normdTactics = SkillHelper.NormalizeSkill(updateSource.SkillTactics);
                        targetVisLevel = VisibilityLevelByTactics(normdTactics);
                        LowVisibility.Logger.Log($"Only sensor lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    } else {
                        LowVisibility.Logger.Log($"No vision or sensor lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    }

                    //if (targetlockState.visionLockLevel != VisionLockType.None && targetlockState.sensorLockLevel == SensorLockType.None) {
                    //    targetVisLevel = VisibilityLevel.LOSFull;
                    //    LowVisibility.Logger.Log($"Only vision lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    //} else if (targetlockState.visionLockLevel == VisionLockType.None && targetlockState.sensorLockLevel != SensorLockType.None) {
                    //    // TODO: This really should account for the tactics of the shared vision
                    //    int normdTactics = SkillHelper.NormalizeSkill(updateSource.SkillTactics);
                    //    targetVisLevel = VisibilityLevelByTactics(normdTactics);
                    //    LowVisibility.Logger.Log($"Only sensor lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    //} else {
                    //    LowVisibility.Logger.Log($"No vision or sensor lock to actor:{ActorLabel(target)}, setting visibility to:{targetVisLevel}");
                    //}
                    LowVisibility.Logger.Log($"Setting actor:{ActorLabel(target)} visibility to:{targetVisLevel}");
                    target.OnPlayerVisibilityChanged(targetVisLevel);
                }
            }

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
            State.SourceActorLockStates[source.GUID] = updatedLocks;

            // Send a message updating the visibility of any actors that changed
            //PublishVisibilityChange(source.Combat, targetsWithVisibilityChanges); 

            // TESTING: This works better, but doesn't do it on activation for some reason?
            // ALSO: APPLIES TO BOTH PLAYER AND ENEMY - only update enemies!

        }

        public static LockState GetUnifiedLockStateForTarget(AbstractActor source, AbstractActor target) {
            if (source == null || target == null) { return null; }

            // Get the source lock state first            
            if (!State.SourceActorLockStates.ContainsKey(source.GUID)) {
                LowVisibility.Logger.LogIfDebug($"----- source:{source.GUID} is missing, THIS SHOULD NOT HAPPEN!");
                //UpdateActorDetection(source);
            }
            //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Looking for sourceLocks for actor: {ActorLabel(source)} vs: {ActorLabel(target)}");
            HashSet<LockState> sourceLocks = State.SourceActorLockStates[source.GUID];
            LockState sourceLockState = sourceLocks?.FirstOrDefault(ls => ls.targetGUID == target.GUID);
            LockState unifiedLockState = sourceLockState != null ? new LockState(sourceLockState) : CalculateLock(source, target);

            // Finally check for shared visibility
            //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Looking for shared vision");
            bool isPlayer = source.team == source.Combat.LocalPlayerTeam;
            List<AbstractActor> friendlies = isPlayer ? PlayerAndAlliedActors(source.Combat) : EnemyAndNeutralActors(source.Combat);
            foreach (AbstractActor friendly in friendlies) {
                if (State.SourceActorLockStates.ContainsKey(friendly.GUID)) {
                    //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Checking shared vision for actor:{ActorLabel(friendly)}");
                    HashSet<LockState> friendlyLocks = State.SourceActorLockStates[friendly.GUID];
                    if (friendlyLocks != null) {
                        //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: friendlyLocks found actor:{ActorLabel(friendly)}");
                        LockState friendlyLockState = friendlyLocks?.FirstOrDefault(ls => ls.targetGUID == target.GUID);

                        //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: friendly actor:{ActorLabel(friendly)} has lockState:{friendlyLockState}");
                        // Vision is always shared
                        if (friendlyLockState != null && friendlyLockState.visionLockLevel > unifiedLockState.visionLockLevel) {
                            //LowVisibility.Logger.LogIfDebug($"friendly:{ActorLabel(friendly)} has a superior vision lock to target:{ActorLabel(target)}, using their lock.");
                            unifiedLockState.visionLockLevel = friendlyLockState.visionLockLevel;
                        }

                        // Sensors are conditionally shared
                        StaticEWState friendlyEWConfig = State.GetStaticState(friendly);
                        if (friendlyLockState != null && friendlyEWConfig.sharesSensors && friendlyLockState.sensorLockLevel > unifiedLockState.sensorLockLevel ) {
                            unifiedLockState.sensorLockLevel = friendlyLockState.sensorLockLevel;
                            //LowVisibility.Logger.LogIfDebug($"friendly:{ActorLabel(friendly)} shares sensors and has a superior sensor lock to target:{ActorLabel(target)}, using their lock.");
                        }
                    }
                }
            }

            //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: exiting");
            return unifiedLockState;
        }

    }

}
