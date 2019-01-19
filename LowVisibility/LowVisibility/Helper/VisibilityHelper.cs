using BattleTech;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Helper {

    public static class VisibilityHelper {

        public static DetectionLevel DetectionLevelForCheck(int checkResult) {
            DetectionLevel level = DetectionLevel.NoInfo;
            if (checkResult == 0) {
                level = DetectionLevel.Location;
            } else if (checkResult == 1) {
                level = DetectionLevel.Type;
            } else if (checkResult == 2) {
                level = DetectionLevel.Silhouette;
            } else if (checkResult == 3) {
                level = DetectionLevel.Vector;
            } else if (checkResult == 4 || checkResult == 5) {
                level = DetectionLevel.SurfaceScan;
            } else if (checkResult == 6 || checkResult == 7) {
                level = DetectionLevel.SurfaceAnalysis;
            } else if (checkResult == 8) {
                level = DetectionLevel.WeaponAnalysis;
            } else if (checkResult == 9) {
                level = DetectionLevel.StructureAnalysis;
            } else if (checkResult == 10) {
                level = DetectionLevel.DeepScan;
            } else if (checkResult >= 11) {
                level = DetectionLevel.DentalRecords;
            }
            return level;
        }

        public static HashSet<LockState> CalculateTargetLocks(AbstractActor source, List<AbstractActor> targets) {
            HashSet<LockState> locksToTargets = new HashSet<LockState>();

            foreach (AbstractActor target in targets) {
                if (target.GUID == source.GUID) { continue;  }

                LockState lockState = CalculateLock(source, target);
                LowVisibility.Logger.LogIfTrace($"  {CombatantHelper.Label(source)} --> {CombatantHelper.Label(target)} is lockState:{lockState}");
                locksToTargets.Add(lockState);
            }

            return locksToTargets;
        }

        // Calculate the lock level between a source and target actor
        public static LockState CalculateLock(AbstractActor source, AbstractActor target) {

            LockState newLockState = new LockState {
                sourceGUID = source.GUID,
                targetGUID = target.GUID,
                visionLockLevel = VisionLockType.None,
                sensorLockLevel = DetectionLevel.NoInfo,
            };

            LowVisibility.Logger.LogIfTrace($"Calculating Lock {CombatantHelper.Label(source)} ==> {CombatantHelper.Label(target)}");

            if (source.IsDead || source.IsFlaggedForDeath || source.IsTeleportedOffScreen || target.IsTeleportedOffScreen) {
                // If we're dead, we can't have vision or sensors. If we're off the map, we can't either. If the target is off the map, we can't see it.
                newLockState.visionLockLevel = VisionLockType.None;
                newLockState.sensorLockLevel = DetectionLevel.NoInfo;
                LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} is dead or dying. Forcing no visibility.");
                return newLockState;
            } else if (target.IsDead || target.IsFlaggedForDeath) {
                // If the target is dead, we can't have sensor but we have vision 
                newLockState.visionLockLevel = VisionLockType.Silhouette;
                newLockState.sensorLockLevel = DetectionLevel.NoInfo;
                LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(target)} is dead or dying. Forcing no sensor lock, vision based upon visibility.");
                return newLockState;
            } else if (source.GUID == target.GUID || source.Combat.HostilityMatrix.IsFriendly(source.TeamId, target.TeamId)) {
                // If they are us, or allied, automatically give full vision
                newLockState.visionLockLevel = VisionLockType.VisualID;
                newLockState.sensorLockLevel = DetectionLevel.DentalRecords;
                LowVisibility.Logger.LogIfDebug($"  -- source:{CombatantHelper.Label(source)} is friendly to target:{CombatantHelper.Label(target)}. Forcing full visibility.");
                return newLockState;
            } 

            // --- Determine visual lock 
            VisibilityLevelAndAttribution visLevelAndAttrib = source.VisibilityCache.VisibilityToTarget(target);

            // If we have LOS to the target, we at least have their silhouette
            if (visLevelAndAttrib.VisibilityLevel == VisibilityLevel.LOSFull) {
                newLockState.visionLockLevel = VisionLockType.Silhouette;
            }

            float distance = Vector3.Distance(source.CurrentPosition, target.CurrentPosition);
            float targetVisibility = CalculateTargetVisibility(target);

            float pilotVisualLockRange = ActorHelper.GetVisualLockRange(source);
            float visualLockRange = pilotVisualLockRange * targetVisibility;

            if (distance <= visualLockRange) { newLockState.visionLockLevel = VisionLockType.VisualID; }

            // TODO: Check for failed visual lock and adjust appropriately. Requires visionLockLevel to change to DetectionLevel
            LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} has visualLockRange:{visualLockRange} and is distance:{distance} " +
                $"from target:{CombatantHelper.Label(target)} with visibility:{targetVisibility} - visionLockType:{newLockState.visionLockLevel}");

            // -- Determine sensor lock            
             if (target.IsDead || target.IsFlaggedForDeath) {
                newLockState.sensorLockLevel = DetectionLevel.NoInfo;
                LowVisibility.Logger.LogIfDebug($"  -- target:{CombatantHelper.Label(target)} is dead or dying, cannot have sensor lock. Forcing detectionLevel to NoInfo.");
            } else {
                StaticEWState sourceStaticState = State.GetStaticState(source);
                DynamicEWState sourceDynamicState = State.GetDynamicState(source);
                int modifiedSourceCheck = sourceDynamicState.detailCheck + sourceStaticState.tacticsBonus;                

                // --- Source modifiers: ECM, Active Probe, SensorBoost tag
                // Check for ECM strength
                if (State.ECMJamming(source) != 0) {
                    modifiedSourceCheck -= State.ECMJamming(source);
                    LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} is jammed with strength:{State.ECMJamming(source)}, " +
                        $"reducing sourceCheckResult to:{modifiedSourceCheck}");
                }

                if (sourceStaticState.probeMod != 0) {
                    modifiedSourceCheck += sourceStaticState.probeMod;
                    LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} has probe with strength:{sourceStaticState.probeMod}, " +
                        $"increasing sourceCheckResult to:{modifiedSourceCheck}");
                }

                if (sourceStaticState.probeBoostMod != 0) {
                    modifiedSourceCheck += sourceStaticState.probeBoostMod;
                    LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} has Probe Boost with strength:{sourceStaticState.probeBoostMod}, " +
                        $"increasing sourceCheckResult to:{modifiedSourceCheck}");
                }

                if (sourceStaticState.sensorBoostMod != 0) {
                    modifiedSourceCheck += sourceStaticState.probeMod;
                    LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} has sensorMod with strength:{sourceStaticState.sensorBoostMod}, " +
                        $"increasing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // --- Target Modifiers: Stealth, Narc, Tag
                StaticEWState targetStaticState = State.GetStaticState(target);

                // Check for target stealth
                if (targetStaticState.stealthMod != 0) {
                    modifiedSourceCheck -= targetStaticState.stealthMod;
                    LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(target)} has stealthMod:{targetStaticState.stealthMod}, " +
                        $"reducing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // Check for target scrambler
                if (targetStaticState.scramblerMod != 0) {
                    modifiedSourceCheck -= targetStaticState.scramblerMod;
                    LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(target)} has scramblerMod:{targetStaticState.scramblerMod}, " +
                        $"reducing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // Check for a Narc effect
                if (State.NARCEffect(target) != 0) {
                    modifiedSourceCheck += State.NARCEffect(target);
                    LowVisibility.Logger.LogIfDebug($"  -- target:{CombatantHelper.Label(target)} has NARC effect:{State.NARCEffect(target)}, " +
                        $"increasing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // Check for a Tag effect
                if (State.TAGEffect(target) != 0) {
                    modifiedSourceCheck += State.TAGEffect(target);
                    LowVisibility.Logger.LogIfDebug($"  -- target:{CombatantHelper.Label(target)} has TAG effect:{State.TAGEffect(target)}, " +
                        $"increasing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // Determine the final lockLevelCheck
                newLockState.sensorLockLevel = VisibilityHelper.DetectionLevelForCheck(modifiedSourceCheck);

                float sourceSensorsRange = GetSensorsRange(source);                                
                float targetSignature = CalculateTargetSignature(target);
                float sensorLockRange = sourceSensorsRange * targetSignature;
                if (distance > sensorLockRange) {
                    newLockState.sensorLockLevel = DetectionLevel.NoInfo;
                }
                LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} ==> target:{CombatantHelper.Label(target)} " +
                    $"distance:{distance} vs sensorLockRange:{sensorLockRange} (sourceSensorRange:{sourceSensorsRange} * targetSignature:{targetSignature}) " +
                    $"yields sensorLockType:{newLockState.sensorLockLevel}");
            }

            return newLockState;

        }

        public static void UpdateVisibilityForAllTeams(CombatGameState Combat) {
            LowVisibility.Logger.LogIfDebug($"  ==== Updating Visibility for All Teams ====");

            List<AbstractActor> playerActors = HostilityHelper.PlayerActors(Combat);
            List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(Combat);
            List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(Combat);
            List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(Combat);
            
            LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Players to Neutral and Enemies ====");
            List<AbstractActor> targets = enemyActors.Union(neutralActors).ToList();
            List<ICombatant> combatants = new List<ICombatant>(targets.ToArray());
            foreach (AbstractActor source in playerActors) {                
                source.VisibilityCache.RebuildCache(combatants);
            }

            LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Allies to Neutral and Enemies ====");
            foreach (AbstractActor source in alliedActors) {
                source.VisibilityCache.RebuildCache(combatants);
            }

            LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Neutrals to players, allies and enemies. ====");
            targets = playerActors.Union(alliedActors).Union(enemyActors).ToList();
            combatants = new List<ICombatant>(targets.ToArray());
            foreach (AbstractActor source in neutralActors) {
                source.VisibilityCache.RebuildCache(combatants);
            }

            LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Enemies to players, allies and neutrals. ====");
            targets = playerActors.Union(alliedActors).Union(neutralActors).ToList();
            combatants = new List<ICombatant>(targets.ToArray());
            foreach (AbstractActor source in enemyActors) {
                source.VisibilityCache.RebuildCache(combatants);
            }
        }

        public static void UpdateDetectionForActor(AbstractActor actor) {

            List<AbstractActor> playerActors = HostilityHelper.PlayerActors(actor.Combat);
            List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(actor.Combat);
            List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(actor.Combat);
            List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(actor.Combat);

            HashSet<LockState> sourceActorLocks = new HashSet<LockState>();
            if (HostilityHelper.IsLocalPlayerEnemy(actor)) {
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, playerActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, alliedActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, neutralActors));
            } else if (HostilityHelper.IsLocalPlayerNeutral(actor)) {
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, playerActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, alliedActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, enemyActors));
            } else if (HostilityHelper.IsLocalPlayerAlly(actor)) {
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, playerActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, enemyActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, neutralActors));
            } else if (HostilityHelper.IsPlayer(actor)) {
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, alliedActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, enemyActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(actor, neutralActors));
            }
            State.SourceActorLockStates[actor.GUID] = sourceActorLocks;
        }

        // Updates the detection state for all units. Called at start of round, and after each enemy movement.
        public static void UpdateDetectionForAllActors(CombatGameState Combat) {

            List<AbstractActor> playerActors = HostilityHelper.PlayerActors(Combat);
            List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(Combat);
            List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(Combat);
            List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(Combat);
            
            // Update friendlies
            LowVisibility.Logger.LogIfTrace($"  ==== Updating Players ====");
            foreach (AbstractActor source in playerActors) {
                HashSet<LockState> sourceActorLocks = new HashSet<LockState>();
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, alliedActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, enemyActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, neutralActors));                
                State.SourceActorLockStates[source.GUID] = sourceActorLocks;
                LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
            }

            // Update allies
            LowVisibility.Logger.LogIfTrace($"  ==== Updating PlayerAllies ====");
            foreach (AbstractActor source in alliedActors) {
                HashSet<LockState> sourceActorLocks = new HashSet<LockState>();
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, playerActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, enemyActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, neutralActors));
                State.SourceActorLockStates[source.GUID] = sourceActorLocks;
                LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
            }

            // Update neutrals
            LowVisibility.Logger.LogIfTrace($"  ==== Updating NeutralToPlayer ====");
            foreach (AbstractActor source in neutralActors) {
                HashSet<LockState> sourceActorLocks = new HashSet<LockState>();
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, playerActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, alliedActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, enemyActors));
                State.SourceActorLockStates[source.GUID] = sourceActorLocks;
                LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
            }


            // Update foes
            LowVisibility.Logger.LogIfTrace($"  ==== Updating EnemiesToPlayer ====");
            foreach (AbstractActor source in enemyActors) {
                HashSet<LockState> sourceActorLocks = new HashSet<LockState>();
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, playerActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, alliedActors));
                sourceActorLocks.UnionWith(CalculateTargetLocks(source, neutralActors));
                State.SourceActorLockStates[source.GUID] = sourceActorLocks;
                LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
            }
        }

        // Get a unified LockState for the target, representing the details that the source should be able to see.
        public static LockState GetUnifiedLockStateForTarget(AbstractActor source, AbstractActor target) {
            if (source == null || target == null) { return null; }

            if (source.GUID == target.GUID) {
                StackTrace st = new StackTrace();
                LowVisibility.Logger.Log($"UnifiedLockState for self from: {st.GetFrame(1)}");
                return new LockState() {
                    visionLockLevel = VisionLockType.VisualID,
                    sensorLockLevel = DetectionLevel.DentalRecords,
                    sourceGUID = source.GUID,
                    targetGUID = source.GUID
                };
            }

            // Check for shared lock state 
            LockState unifiedLockState = null;

            bool sourceIsPlayer = source.team == source.Combat.LocalPlayerTeam;
            bool sourceIsLocalPlayerEnemy = source.Combat.HostilityMatrix.IsLocalPlayerEnemy(source.team);
            bool sourceIsLocalPlayerNeutral = source.Combat.HostilityMatrix.IsLocalPlayerNeutral(source.team);
            bool sourceIsLocalPlayerAlly = source.Combat.HostilityMatrix.IsLocalPlayerFriendly(source.team) && !sourceIsPlayer;

            if (HostilityHelper.IsLocalPlayerEnemy(source)) {
                unifiedLockState = UnifyLockState(HostilityHelper.EnemyToLocalPlayerActors(source.Combat), target);
                LowVisibility.Logger.LogIfTrace($" == EnemyToLocalPlayerActors unifiedLockState is:{unifiedLockState}");
            } else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
                unifiedLockState = UnifyLockState(HostilityHelper.NeutralToLocalPlayerActors(source.Combat), target);
                LowVisibility.Logger.LogIfTrace($" == NeutralToLocalPlayerActors unifiedLockState is:{unifiedLockState}");
            } else if (HostilityHelper.IsLocalPlayerAlly(source)) {
                List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
                    .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
                    .ToList();
                unifiedLockState = UnifyLockState(actorsSharingVision, target);
                LowVisibility.Logger.LogIfTrace($" == AlliedToLocalPlayerActors unifiedLockState is:{unifiedLockState}");
            } else if (HostilityHelper.IsPlayer(source)) {
                List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
                    .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
                    .ToList();
                unifiedLockState = UnifyLockState(actorsSharingVision, target);
                LowVisibility.Logger.LogIfTrace($" == PlayerActors unifiedLockState is:{unifiedLockState}");
            }

            //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: exiting");
            return unifiedLockState;
        }

        public static LockState UnifyLockState(List<AbstractActor> actorsSharingState, AbstractActor target) {
            LockState lockState = new LockState {                
                targetGUID = target.GUID,
                sensorLockLevel = DetectionLevel.NoInfo,
                visionLockLevel = VisionLockType.None
            };

            // TOOD: Check for NEUTRALS
            foreach (AbstractActor actor in actorsSharingState) {
                if (State.SourceActorLockStates.ContainsKey(actor.GUID)) {                    
                    HashSet<LockState> actorLocks = State.SourceActorLockStates[actor.GUID];
                    if (actorLocks != null) {                        
                        LockState actorLockState = actorLocks?.FirstOrDefault(ls => ls.targetGUID == target.GUID);

                        // Vision is always shared
                        if (actorLockState != null && actorLockState.visionLockLevel > lockState.visionLockLevel) {                            
                            lockState.visionLockLevel = actorLockState.visionLockLevel;
                            lockState.sourceGUID = actor.GUID;
                        }

                        // Sensors are conditionally shared
                        StaticEWState friendlyEWConfig = State.GetStaticState(actor);
                        if (actorLockState != null && friendlyEWConfig.sharesSensors && actorLockState.sensorLockLevel > lockState.sensorLockLevel) {
                            lockState.sensorLockLevel = actorLockState.sensorLockLevel;
                            lockState.sourceGUID = actor.GUID;
                        }
                    }
                }
            }

            return lockState;
        }

        // Determine the HBS VisibilityLevel to show in game. This is the in-game representation of the unit, either as a 3d model, blip or blob.
        public static VisibilityLevel GetUnifiedVisibilityLevel(AbstractActor source, AbstractActor target) {

            if (source == null || target == null) { return VisibilityLevel.None; }

            if (source.GUID == target.GUID) {                
                LowVisibility.Logger.LogIfTrace($"UnifiedVisibility from self is always LOSFull");
                return VisibilityLevel.LOSFull;
            }

            // Get the source lock state first            
            if (!State.SourceActorLockStates.ContainsKey(source.GUID)) {
                LowVisibility.Logger.Log($"WARNING: source:{source.GUID} is missing a lock state, THIS SHOULD NOT HAPPEN!");                
            }

            //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Looking for sourceLocks for actor: {CombatantHelper.Label(source)} vs: {CombatantHelper.Label(target)}");
            HashSet<LockState> sourceLocks = State.SourceActorLockStates[source.GUID];
            LockState sourceLockState = sourceLocks?.FirstOrDefault(ls => ls.targetGUID == target.GUID);

            VisibilityLevel unifiedVisibility = VisibilityLevel.None;            
            if (HostilityHelper.IsLocalPlayerEnemy(source)) {
                unifiedVisibility = UnifyVision(HostilityHelper.EnemyToLocalPlayerActors(source.Combat), target);
                LowVisibility.Logger.LogIfTrace($" == EnemyToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
            } else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
                unifiedVisibility = UnifyVision(HostilityHelper.NeutralToLocalPlayerActors(source.Combat), target);
                LowVisibility.Logger.LogIfTrace($" == NeutralToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
            } else if (HostilityHelper.IsLocalPlayerAlly(source)) {
                List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
                    .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
                    .ToList();
                unifiedVisibility = UnifyVision(actorsSharingVision, target);
                LowVisibility.Logger.LogIfTrace($" == AlliedToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");                
            } else if (HostilityHelper.IsPlayer(source)) {
                List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
                    .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
                    .ToList();
                unifiedVisibility = UnifyVision(actorsSharingVision, target);
                LowVisibility.Logger.LogIfTrace($" == PlayerActors unifiedVisibility is:{unifiedVisibility}");
            }

            // Next, check for sensor lock
            if (unifiedVisibility == VisibilityLevel.None) {
                LowVisibility.Logger.LogIfTrace($" Checking sensorLocks for source:{CombatantHelper.Label(source)} ==> target:{CombatantHelper.Label(target)}");

                if (HostilityHelper.IsLocalPlayerEnemy(source)) {
                    unifiedVisibility = UnifySensorVisibility(HostilityHelper.EnemyToLocalPlayerActors(source.Combat), target);
                    LowVisibility.Logger.LogIfTrace($" == EnemyToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
                } else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
                    unifiedVisibility = UnifySensorVisibility(HostilityHelper.NeutralToLocalPlayerActors(source.Combat), target);
                    LowVisibility.Logger.LogIfTrace($" == NeutralToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
                } else if (HostilityHelper.IsLocalPlayerAlly(source)) {
                    unifiedVisibility = UnifySensorVisibility(HostilityHelper.AlliedToLocalPlayerActors(source.Combat), target);
                    LowVisibility.Logger.LogIfTrace($" == AlliedToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");

                    // Check for shared sensors on player
                    List<AbstractActor> sensorSharingPlayers = HostilityHelper.PlayerActors(source.Combat)
                        .Where(aa => State.GetStaticState(aa).sharesSensors).ToList();
                    VisibilityLevel playerVisibility = UnifySensorVisibility(sensorSharingPlayers, target);
                    if (playerVisibility > unifiedVisibility) {
                        LowVisibility.Logger.LogIfTrace($"----- PlayerActors have greater visibility:{playerVisibility}, sharing with AlliedToLocalPlayerActors.");
                        unifiedVisibility = playerVisibility;
                    }

                } else if (HostilityHelper.IsPlayer(source)) {
                    unifiedVisibility = UnifySensorVisibility(HostilityHelper.PlayerActors(source.Combat), target);
                    LowVisibility.Logger.LogIfTrace($" == PlayerActors unifiedVisibility is:{unifiedVisibility}");

                    // Check for shared sensors on allies
                    List<AbstractActor> sensorSharingAllies = HostilityHelper.AlliedToLocalPlayerActors(source.Combat)
                        .Where(aa => State.GetStaticState(aa).sharesSensors).ToList();
                    VisibilityLevel alliedVisibility = UnifySensorVisibility(sensorSharingAllies, target);
                    if (alliedVisibility > unifiedVisibility) {
                        LowVisibility.Logger.LogIfTrace($"----- AlliedToLocalPlayerActors have greater visibility:{alliedVisibility}, sharing with PlayerActors.");
                        unifiedVisibility = alliedVisibility;
                    }
                }

            }

            LowVisibility.Logger.LogIfTrace($" UnifiedVisibilityLevel for source:{CombatantHelper.Label(source)} ==> target:{CombatantHelper.Label(target)} is {unifiedVisibility}");
            return unifiedVisibility;
        }

        private static VisibilityLevel UnifyVision(List<AbstractActor> unitsSharingVision, AbstractActor target) {
            VisibilityLevel visibilityLevel = VisibilityLevel.None;

            foreach (AbstractActor actor in unitsSharingVision) {
                if (State.SourceActorLockStates.ContainsKey(actor.GUID)) {
                    HashSet<LockState> actorsLockStates = State.SourceActorLockStates[actor.GUID];
                    if (actorsLockStates != null) {
                        //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: friendlyLocks found actor:{CombatantHelper.Label(friendly)}");
                        LockState lockState = actorsLockStates?.FirstOrDefault(ls => ls.targetGUID == target.GUID);

                        //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: friendly actor:{CombatantHelper.Label(friendly)} has lockState:{friendlyLockState}");
                        // Vision is always shared
                        if (lockState != null && lockState.visionLockLevel > VisionLockType.None) {
                            //LowVisibility.Logger.LogIfDebug($"friendly:{CombatantHelper.Label(friendly)} has a superior vision lock to target:{CombatantHelper.Label(target)}, using their lock.");
                            visibilityLevel = VisibilityLevel.LOSFull;
                            break;
                        }
                    }
                }
            }

            return visibilityLevel;
        }

        private static VisibilityLevel UnifySensorVisibility(List<AbstractActor> unitsSharingSensors, AbstractActor target) {
            VisibilityLevel visibilityLevel = VisibilityLevel.None;

            foreach (AbstractActor sensorSharingActor in unitsSharingSensors) {
                if (State.SourceActorLockStates.ContainsKey(sensorSharingActor.GUID)) {
                    HashSet<LockState> sensorActorLockStates = State.SourceActorLockStates[sensorSharingActor.GUID];
                    if (sensorActorLockStates != null) {
                        LockState actorLockState = sensorActorLockStates?.FirstOrDefault(ls => ls.targetGUID == target.GUID);
                        if (actorLockState != null) {
                            VisibilityLevel actorVisibility = VisibilityFromSensorLock(actorLockState.sensorLockLevel);                            
                            if (actorVisibility > visibilityLevel) {
                                LowVisibility.Logger.LogIfTrace($"----- actor:{CombatantHelper.Label(sensorSharingActor)} ==> {CombatantHelper.Label(target)} " +
                                    $"has visibility:{actorVisibility}, increasing team visibility.");
                                visibilityLevel = actorVisibility;
                            }
                        }
                    }
                }
            }

            return visibilityLevel;
        }

        private static VisibilityLevel VisibilityFromSensorLock(DetectionLevel sensorLockLevel) {
            VisibilityLevel visibilityLevel = VisibilityLevel.None;

            if (sensorLockLevel >= DetectionLevel.Silhouette) {
                visibilityLevel = VisibilityLevel.Blip4Maximum;
            } else if (sensorLockLevel >= DetectionLevel.Type) {
                visibilityLevel = VisibilityLevel.Blip1Type;
            } else if (sensorLockLevel >= DetectionLevel.Location) {
                visibilityLevel = VisibilityLevel.Blip0Minimum;
            } 

            return visibilityLevel;
        }


    }

}
