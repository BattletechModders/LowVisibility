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



        //public static HashSet<Locks> CalculateTargetLocks(AbstractActor source, List<AbstractActor> targets) {

        //    if (source == null || source.VisibilityCache == null || targets == null) { return null; }

        //    HashSet<Locks> locksToTargets = new HashSet<Locks>();

        //    foreach (AbstractActor target in targets) {
        //        if (target.GUID == source.GUID) { continue;  }

        //        Locks lockState = CalculateLock(source, target);
        //        LowVisibility.Logger.LogIfTrace($"  {CombatantHelper.Label(source)} --> {CombatantHelper.Label(target)} is lockState:{lockState}");
        //        locksToTargets.Add(lockState);
        //    }

        //    return locksToTargets;
        //}

      

        //// Calculate the lock level between a source and target actor
        //public static Locks CalculateLock(AbstractActor source, AbstractActor target) {

        //    Locks newLockState = new Locks {
        //        sourceGUID = source.GUID,
        //        targetGUID = target.GUID,
        //        visualLock = VisualScanType.None,
        //        sensorLock = SensorScanType.NoInfo,
        //    };

        //    LowVisibility.Logger.LogIfTrace($"Calculating Lock {CombatantHelper.Label(source)} ==> {CombatantHelper.Label(target)}");

        //    if (source.IsDead || source.IsFlaggedForDeath || source.IsTeleportedOffScreen || target.IsTeleportedOffScreen) {
        //        // If we're dead, we can't have vision or sensors. If we're off the map, we can't either. If the target is off the map, we can't see it.
        //        newLockState.visualLock = VisualScanType.None;
        //        newLockState.sensorLock = SensorScanType.NoInfo;
        //        LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} is dead or dying. Forcing no visibility.");
        //        return newLockState;
        //    } else if (target.IsDead || target.IsFlaggedForDeath) {
        //        // If the target is dead, we can't have sensor but we have vision 
        //        newLockState.visualLock = VisualScanType.Silhouette;
        //        newLockState.sensorLock = SensorScanType.NoInfo;
        //        LowVisibility.Logger.LogIfTrace($"  -- target:{CombatantHelper.Label(target)} is dead or dying. Forcing no sensor lock, vision based upon visibility.");
        //        return newLockState;
        //    } else if (source.GUID == target.GUID || source.Combat.HostilityMatrix.IsFriendly(source.TeamId, target.TeamId)) {
        //        // If they are us, or allied, automatically give full vision
        //        newLockState.visualLock = VisualScanType.VisualID;
        //        newLockState.sensorLock = SensorScanType.DentalRecords;
        //        LowVisibility.Logger.LogIfDebug($"  -- source:{CombatantHelper.Label(source)} is friendly to target:{CombatantHelper.Label(target)}. Forcing full visibility.");
        //        return newLockState;
        //    } 

        //    // --- Determine visual lock 
        //    VisibilityLevelAndAttribution visLevelAndAttrib = source.VisibilityCache.VisibilityToTarget(target);

        //    // If we have LOS to the target, we at least have their silhouette
        //    if (visLevelAndAttrib.VisibilityLevel == VisibilityLevel.LOSFull) {
        //        newLockState.visualLock = VisualScanType.Silhouette;
        //    }

        //    float distance = Vector3.Distance(source.CurrentPosition, target.CurrentPosition);
        //    //float targetVisibility = CalculateTargetVisibility(target);
        //    float targetVisibility = 1f;

        //    float visualScanRange = VisualLockHelper.GetVisualScanRange(source);
        //    float targetModifiedScanRange = visualScanRange * targetVisibility;
        //    if (targetModifiedScanRange < LowVisibility.Config.MinimumVisionRange()) {
        //        targetModifiedScanRange = LowVisibility.Config.MinimumVisionRange();
        //    }

        //    if (distance <= targetModifiedScanRange) { newLockState.visualLock = VisualScanType.VisualID; }

        //    // TODO: Check for failed visual lock and adjust appropriately. Requires visionLockLevel to change to DetectionLevel
        //    LowVisibility.Logger.LogIfTrace($"  -- source:{CombatantHelper.Label(source)} has visualLockRange:{targetModifiedScanRange} and is distance:{distance} " +
        //        $"from target:{CombatantHelper.Label(target)} with visibility:{targetVisibility} - visionLockType:{newLockState.visualLock}");
            
        //    return newLockState;

        //}

        //public static void UpdateVisibilityForAllTeams(CombatGameState Combat) {
        //    LowVisibility.Logger.LogIfDebug($"  ==== Updating Visibility for All Teams ====");

        //    List<AbstractActor> playerActors = HostilityHelper.PlayerActors(Combat);
        //    List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(Combat);
        //    List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(Combat);
        //    List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(Combat);
            
        //    LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Players to Neutral and Enemies ====");
        //    List<AbstractActor> targets = enemyActors.Union(neutralActors).ToList();
        //    List<ICombatant> combatants = new List<ICombatant>(targets.ToArray());
        //    foreach (AbstractActor source in playerActors) {                
        //        source.VisibilityCache.UpdateCacheReciprocal(combatants);
        //    }

        //    LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Allies to Neutrals and Enemies ====");
        //    foreach (AbstractActor source in alliedActors) {
        //        source.VisibilityCache.UpdateCacheReciprocal(combatants);
        //    }

        //    LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Enemies to Neutrals. ====");
        //    targets = neutralActors;
        //    combatants = new List<ICombatant>(targets.ToArray());
        //    foreach (AbstractActor source in enemyActors) {
        //        LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility source:{CombatantHelper.Label(source)}. ====");
        //        source.VisibilityCache.UpdateCacheReciprocal(combatants);
        //    }

        //    //LowVisibility.Logger.LogIfTrace($"  ==== Updating Visibility for Neutrals to Enemies. ====");
        //    //targets = playerActors.Union(alliedActors).Union(enemyActors).ToList();
        //    //combatants = new List<ICombatant>(targets.ToArray());
        //    //foreach (AbstractActor source in neutralActors) {
        //    //    source.VisibilityCache.UpdateCacheReciprocal(combatants);
        //    //}
        //}

        //// PRESUMES LOCK IS UP TO DATE
        //public static void UpdateVisibilityforPlayer(AbstractActor playerActor) {
        //    LowVisibility.Logger.LogIfDebug($"  ==== Updating Visibility for Player Actor ====");
        //    // Update enemy
        //    List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(playerActor.Combat);
        //    List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(playerActor.Combat);
        //    List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(playerActor.Combat);
        //    List<AbstractActor> targets = enemyActors.Union(neutralActors).Union(alliedActors).ToList();

        //    foreach (AbstractActor actor in targets) {
        //        playerActor.VisibilityCache.CalcVisValueToTarget(actor);
        //    }
        //}

        //public static void UpdateDetectionForActor(AbstractActor actor) {

        //    List<AbstractActor> playerActors = HostilityHelper.PlayerActors(actor.Combat);
        //    List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(actor.Combat);
        //    List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(actor.Combat);
        //    List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(actor.Combat);

        //    HashSet<Locks> sourceActorLocks = new HashSet<Locks>();
        //    if (HostilityHelper.IsLocalPlayerEnemy(actor)) {
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, playerActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, alliedActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, neutralActors));
        //    } else if (HostilityHelper.IsLocalPlayerNeutral(actor)) {
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, playerActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, alliedActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, enemyActors));
        //    } else if (HostilityHelper.IsLocalPlayerAlly(actor)) {
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, playerActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, enemyActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, neutralActors));
        //    } else if (HostilityHelper.IsPlayer(actor)) {
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, alliedActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, enemyActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(actor, neutralActors));
        //    }
        //    State.SourceActorLockStates[actor.GUID] = sourceActorLocks;
        //}

        // Updates the detection state for all units. Called at start of round, and after each enemy movement.
        //public static void UpdateDetectionForAllActors(CombatGameState Combat) {

        //    List<AbstractActor> playerActors = HostilityHelper.PlayerActors(Combat);
        //    List<AbstractActor> alliedActors = HostilityHelper.AlliedToLocalPlayerActors(Combat);
        //    List<AbstractActor> enemyActors = HostilityHelper.EnemyToLocalPlayerActors(Combat);
        //    List<AbstractActor> neutralActors = HostilityHelper.NeutralToLocalPlayerActors(Combat);
            
        //    // Update friendlies
        //    LowVisibility.Logger.LogIfTrace($"  ==== Updating Players ====");
        //    foreach (AbstractActor source in playerActors) {
        //        HashSet<Locks> sourceActorLocks = new HashSet<Locks>();
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, alliedActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, enemyActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, neutralActors));                
        //        State.SourceActorLockStates[source.GUID] = sourceActorLocks;
        //        LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
        //    }

        //    // Update allies
        //    LowVisibility.Logger.LogIfTrace($"  ==== Updating PlayerAllies ====");
        //    foreach (AbstractActor source in alliedActors) {
        //        HashSet<Locks> sourceActorLocks = new HashSet<Locks>();
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, playerActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, enemyActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, neutralActors));
        //        State.SourceActorLockStates[source.GUID] = sourceActorLocks;
        //        LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
        //    }

        //    // Update neutrals
        //    LowVisibility.Logger.LogIfTrace($"  ==== Updating NeutralToPlayer ====");
        //    foreach (AbstractActor source in neutralActors) {
        //        HashSet<Locks> sourceActorLocks = new HashSet<Locks>();
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, playerActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, alliedActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, enemyActors));
        //        State.SourceActorLockStates[source.GUID] = sourceActorLocks;
        //        LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
        //    }


        //    // Update foes
        //    LowVisibility.Logger.LogIfTrace($"  ==== Updating EnemiesToPlayer ====");
        //    foreach (AbstractActor source in enemyActors) {
        //        HashSet<Locks> sourceActorLocks = new HashSet<Locks>();
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, playerActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, alliedActors));
        //        sourceActorLocks.UnionWith(CalculateTargetLocks(source, neutralActors));
        //        State.SourceActorLockStates[source.GUID] = sourceActorLocks;
        //        LowVisibility.Logger.LogIfTrace($" ----- source:{CombatantHelper.Label(source)} locks calculated.");
        //    }
        //}

        // Get a unified LockState for the target, representing the details that the source should be able to see.
        //public static Locks GetUnifiedLockStateForTarget(AbstractActor source, AbstractActor target) {
        //    if (source == null || target == null) { return null; }

        //    if (source.GUID == target.GUID) {
        //        StackTrace st = new StackTrace();
        //        LowVisibility.Logger.Log($"UnifiedLockState for self from: {st.GetFrame(1)}");
        //        return new Locks() {
        //            visualLock = VisualLockType.VisualScan,
        //            sensorLock = SensorLockType.DentalRecords,
        //            sourceGUID = source.GUID,
        //            targetGUID = source.GUID
        //        };
        //    }

        //    // Check for shared lock state 
        //    Locks unifiedLockState = null;

        //    bool sourceIsPlayer = source.team == source.Combat.LocalPlayerTeam;
        //    bool sourceIsLocalPlayerEnemy = source.Combat.HostilityMatrix.IsLocalPlayerEnemy(source.team);
        //    bool sourceIsLocalPlayerNeutral = source.Combat.HostilityMatrix.IsLocalPlayerNeutral(source.team);
        //    bool sourceIsLocalPlayerAlly = source.Combat.HostilityMatrix.IsLocalPlayerFriendly(source.team) && !sourceIsPlayer;

        //    if (HostilityHelper.IsLocalPlayerEnemy(source)) {
        //        unifiedLockState = UnifyLockState(source, HostilityHelper.EnemyToLocalPlayerActors(source.Combat), target);
        //        LowVisibility.Logger.LogIfTrace($" == EnemyToLocalPlayerActors unifiedLockState is:{unifiedLockState}");
        //    } else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
        //        unifiedLockState = UnifyLockState(source, HostilityHelper.NeutralToLocalPlayerActors(source.Combat), target);
        //        LowVisibility.Logger.LogIfTrace($" == NeutralToLocalPlayerActors unifiedLockState is:{unifiedLockState}");
        //    } else if (HostilityHelper.IsLocalPlayerAlly(source)) {
        //        List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
        //            .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
        //            .ToList();
        //        unifiedLockState = UnifyLockState(source, actorsSharingVision, target);
        //        LowVisibility.Logger.LogIfTrace($" == AlliedToLocalPlayerActors unifiedLockState is:{unifiedLockState}");
        //    } else if (HostilityHelper.IsPlayer(source)) {
        //        List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
        //            .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
        //            .ToList();
        //        unifiedLockState = UnifyLockState(source, actorsSharingVision, target);
        //        LowVisibility.Logger.LogIfTrace($" == PlayerActors unifiedLockState is:{unifiedLockState}");
        //    }

        //    //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: exiting");
        //    return unifiedLockState;
        //}

        //public static Locks UnifyLockState(AbstractActor source, List<AbstractActor> actorsSharingState, AbstractActor target) {
        //    Locks lockState = new Locks {                
        //        targetGUID = target.GUID,
        //        sensorLock = SensorLockType.NoInfo,
        //        visualLock = VisualLockType.None
        //    };

        //    foreach (AbstractActor actor in actorsSharingState) {
        //        if (State.SourceActorLockStates.ContainsKey(actor.GUID)) {                                        
        //            Locks actorLockToTarget = State.SourceActorLockStates[actor.GUID]?.FirstOrDefault(ls => ls.targetGUID == target.GUID);
        //            if (actorLockToTarget != null) {                                                
        //                // Vision is always shared
        //                if (actorLockToTarget.visualLock > lockState.visualLock) {                            
        //                    lockState.visualLock = actorLockToTarget.visualLock;
        //                    lockState.sourceGUID = actor.GUID;
        //                }

        //                // If we are the source, use our sensors.
        //                if (actor.GUID == source.GUID && actorLockToTarget.sensorLock > lockState.sensorLock) {
        //                    //LowVisibility.Logger.LogIfDebug($"  using source's sensors:{CombatantHelper.Label(actor)}");
        //                    lockState.sensorLock = actorLockToTarget.sensorLock;
        //                    lockState.sourceGUID = actor.GUID;
        //                }

        //                // Sensors are conditionally shared
        //                EWState actorEWConfig = State.GetEWState(actor);
        //                if (actorEWConfig.sharesSensors && actorLockToTarget.sensorLock > lockState.sensorLock) {
        //                    LowVisibility.Logger.LogIfDebug($"  sharing sensors from actor:{CombatantHelper.Label(actor)}");
        //                    lockState.sensorLock = actorLockToTarget.sensorLock;
        //                    lockState.sourceGUID = actor.GUID;
        //                }
        //            }
        //        }
        //    }

        //    return lockState;
        //}

        // Determine the HBS VisibilityLevel to show in game. This is the in-game representation of the unit, either as a 3d model, blip or blob.
        //public static VisibilityLevel GetUnifiedVisibilityLevel(AbstractActor source, AbstractActor target) {

        //    if (source == null || target == null) { return VisibilityLevel.None; }

        //    if (source.GUID == target.GUID) {                
        //        LowVisibility.Logger.LogIfTrace($"UnifiedVisibility from self is always LOSFull");
        //        return VisibilityLevel.LOSFull;
        //    }

        //    // Get the source lock state first            
        //    if (!State.SourceActorLockStates.ContainsKey(source.GUID)) {
        //        LowVisibility.Logger.Log($"WARNING: source:{source.GUID} is missing a lock state, THIS SHOULD NOT HAPPEN!");                
        //    }

        //    //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: Looking for sourceLocks for actor: {CombatantHelper.Label(source)} vs: {CombatantHelper.Label(target)}");
        //    HashSet<Locks> sourceLocks = State.SourceActorLockStates[source.GUID];
        //    Locks sourceLockState = sourceLocks?.FirstOrDefault(ls => ls.targetGUID == target.GUID);

        //    VisibilityLevel unifiedVisibility = VisibilityLevel.None;            
        //    if (HostilityHelper.IsLocalPlayerEnemy(source)) {
        //        unifiedVisibility = UnifyVision(HostilityHelper.EnemyToLocalPlayerActors(source.Combat), target);
        //        LowVisibility.Logger.LogIfTrace($" == EnemyToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
        //    } else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
        //        unifiedVisibility = UnifyVision(HostilityHelper.NeutralToLocalPlayerActors(source.Combat), target);
        //        LowVisibility.Logger.LogIfTrace($" == NeutralToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
        //    } else if (HostilityHelper.IsLocalPlayerAlly(source)) {
        //        List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
        //            .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
        //            .ToList();
        //        unifiedVisibility = UnifyVision(actorsSharingVision, target);
        //        LowVisibility.Logger.LogIfTrace($" == AlliedToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");                
        //    } else if (HostilityHelper.IsPlayer(source)) {
        //        List<AbstractActor> actorsSharingVision = HostilityHelper.PlayerActors(source.Combat)
        //            .Union(HostilityHelper.AlliedToLocalPlayerActors(source.Combat))
        //            .ToList();
        //        unifiedVisibility = UnifyVision(actorsSharingVision, target);
        //        LowVisibility.Logger.LogIfTrace($" == PlayerActors unifiedVisibility is:{unifiedVisibility}");
        //    }

        //    // Next, check for sensor lock
        //    if (unifiedVisibility == VisibilityLevel.None) {
        //        LowVisibility.Logger.LogIfTrace($" Checking sensorLocks for source:{CombatantHelper.Label(source)} ==> target:{CombatantHelper.Label(target)}");

        //        if (HostilityHelper.IsLocalPlayerEnemy(source)) {
        //            unifiedVisibility = UnifySensorVisibility(HostilityHelper.EnemyToLocalPlayerActors(source.Combat), target);
        //            LowVisibility.Logger.LogIfTrace($" == EnemyToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
        //        } else if (HostilityHelper.IsLocalPlayerNeutral(source)) {
        //            unifiedVisibility = UnifySensorVisibility(HostilityHelper.NeutralToLocalPlayerActors(source.Combat), target);
        //            LowVisibility.Logger.LogIfTrace($" == NeutralToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");
        //        } else if (HostilityHelper.IsLocalPlayerAlly(source)) {
        //            unifiedVisibility = UnifySensorVisibility(HostilityHelper.AlliedToLocalPlayerActors(source.Combat), target);
        //            LowVisibility.Logger.LogIfTrace($" == AlliedToLocalPlayerActors unifiedVisibility is:{unifiedVisibility}");

        //            // Check for shared sensors on player
        //            List<AbstractActor> sensorSharingPlayers = HostilityHelper.PlayerActors(source.Combat)
        //                .Where(aa => State.GetEWState(aa).sharesSensors).ToList();
        //            VisibilityLevel playerVisibility = UnifySensorVisibility(sensorSharingPlayers, target);
        //            if (playerVisibility > unifiedVisibility) {
        //                LowVisibility.Logger.LogIfTrace($"----- PlayerActors have greater visibility:{playerVisibility}, sharing with AlliedToLocalPlayerActors.");
        //                unifiedVisibility = playerVisibility;
        //            }

        //        } else if (HostilityHelper.IsPlayer(source)) {
        //            unifiedVisibility = UnifySensorVisibility(HostilityHelper.PlayerActors(source.Combat), target);
        //            LowVisibility.Logger.LogIfTrace($" == PlayerActors unifiedVisibility is:{unifiedVisibility}");

        //            // Check for shared sensors on allies
        //            List<AbstractActor> sensorSharingAllies = HostilityHelper.AlliedToLocalPlayerActors(source.Combat)
        //                .Where(aa => State.GetEWState(aa).sharesSensors).ToList();
        //            VisibilityLevel alliedVisibility = UnifySensorVisibility(sensorSharingAllies, target);
        //            if (alliedVisibility > unifiedVisibility) {
        //                LowVisibility.Logger.LogIfTrace($"----- AlliedToLocalPlayerActors have greater visibility:{alliedVisibility}, sharing with PlayerActors.");
        //                unifiedVisibility = alliedVisibility;
        //            }
        //        }

        //    }

        //    LowVisibility.Logger.LogIfTrace($" UnifiedVisibilityLevel for source:{CombatantHelper.Label(source)} ==> target:{CombatantHelper.Label(target)} is {unifiedVisibility}");
        //    return unifiedVisibility;
        //}

        //public static VisibilityLevel UnifyVision(List<AbstractActor> unitsSharingVision, AbstractActor target) {
        //    VisibilityLevel visibilityLevel = VisibilityLevel.None;

        //    foreach (AbstractActor actor in unitsSharingVision) {
        //        if (State.SourceActorLockStates.ContainsKey(actor.GUID)) {
        //            HashSet<Locks> actorsLockStates = State.SourceActorLockStates[actor.GUID];
        //            if (actorsLockStates != null) {
        //                //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: friendlyLocks found actor:{CombatantHelper.Label(friendly)}");
        //                Locks lockState = actorsLockStates?.FirstOrDefault(ls => ls.targetGUID == target.GUID);

        //                //LowVisibility.Logger.LogIfDebug($"----- GetUnifiedLockStateForTarget: friendly actor:{CombatantHelper.Label(friendly)} has lockState:{friendlyLockState}");
        //                // Vision is always shared
        //                if (lockState != null && lockState.visualLock > VisualLockType.None) {
        //                    //LowVisibility.Logger.LogIfDebug($"friendly:{CombatantHelper.Label(friendly)} has a superior vision lock to target:{CombatantHelper.Label(target)}, using their lock.");
        //                    visibilityLevel = VisibilityLevel.LOSFull;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    return visibilityLevel;
        //}

        //public static VisibilityLevel UnifySensorVisibility(List<AbstractActor> unitsSharingSensors, AbstractActor target) {
        //    VisibilityLevel visibilityLevel = VisibilityLevel.None;

        //    foreach (AbstractActor sensorSharingActor in unitsSharingSensors) {
        //        if (State.SourceActorLockStates.ContainsKey(sensorSharingActor.GUID)) {
        //            HashSet<Locks> sensorActorLockStates = State.SourceActorLockStates[sensorSharingActor.GUID];
        //            if (sensorActorLockStates != null) {
        //                Locks actorLockState = sensorActorLockStates?.FirstOrDefault(ls => ls.targetGUID == target.GUID);
        //                if (actorLockState != null) {
        //                    VisibilityLevel actorVisibility = VisibilityFromSensorLock(actorLockState.sensorLock);                            
        //                    if (actorVisibility > visibilityLevel) {
        //                        LowVisibility.Logger.LogIfTrace($"----- actor:{CombatantHelper.Label(sensorSharingActor)} ==> {CombatantHelper.Label(target)} " +
        //                            $"has visibility:{actorVisibility}, increasing team visibility.");
        //                        visibilityLevel = actorVisibility;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return visibilityLevel;
        //}

        //private static VisibilityLevel VisibilityFromSensorLock(SensorScanType sensorLockLevel) {
        //    VisibilityLevel visibilityLevel = VisibilityLevel.None;

        //    if (sensorLockLevel >= SensorScanType.Silhouette) {
        //        visibilityLevel = VisibilityLevel.Blip4Maximum;
        //    } else if (sensorLockLevel >= SensorScanType.Type) {
        //        visibilityLevel = VisibilityLevel.Blip1Type;
        //    } else if (sensorLockLevel >= SensorScanType.Location) {
        //        visibilityLevel = VisibilityLevel.Blip0Minimum;
        //    } 

        //    return visibilityLevel;
        //}


    }

}
