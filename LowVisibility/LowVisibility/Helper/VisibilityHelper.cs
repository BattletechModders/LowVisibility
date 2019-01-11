using BattleTech;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Helper {

    public static class VisibilityHelper {

        public enum VisionLockType {
            None,
            Silhouette,
            VisualID
        }

        public enum SensorLockType {
            None,
            SensorID,
            ProbeID
        }

        public class LockState {
            public string sourceGUID;
            public string targetGUID;
            public VisionLockType visionType;
            public SensorLockType sensorType;

            public LockState() {}

            public LockState(LockState source) {
                this.sourceGUID = source.sourceGUID;
                this.targetGUID = source.targetGUID;
                this.visionType = source.visionType;
                this.sensorType = source.sensorType;                
            } 

            public override string ToString() {
                return $"visionLock:{visionType}, sensorLock:{sensorType}";
            }
        }

        public static List<AbstractActor> PlayerActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => aa.TeamId == Combat.LocalPlayerTeamGuid)
                .ToList();
        }

        public static List<AbstractActor> PlayerAndAlliedActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => aa.TeamId == Combat.LocalPlayerTeamGuid || Combat.HostilityMatrix.IsFriendly(Combat.LocalPlayerTeamGuid, aa.TeamId))
                .ToList();
        }

        public static List<AbstractActor> EnemyAndNeutralActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => Combat.HostilityMatrix.IsEnemy(Combat.LocalPlayerTeamGuid, aa.TeamId) || Combat.HostilityMatrix.IsNeutral(Combat.LocalPlayerTeamGuid, aa.TeamId))
                .ToList();
        }

        public static void CalculateTargetLocks(AbstractActor source, List<AbstractActor> targets, HashSet<LockState> updatedLocks, HashSet<string> visibilityUpdates) {

            foreach (AbstractActor target in targets) {
                LockState lockState = CalculateLock(source, target);
                LowVisibility.Logger.LogIfDebug($"Updated lockState for source:{ActorLabel(source)} vs. target:{ActorLabel(target)} is lockState:{lockState}");
                updatedLocks.Add(lockState);

                // Check for update
                HashSet<LockState> currentLocks = State.SourceActorLockStates.ContainsKey(source.GUID) ? State.SourceActorLockStates[source.GUID] : new HashSet<LockState>();
                LockState currentLockState = currentLocks.FirstOrDefault(ls => ls.targetGUID == target.GUID);
                if (currentLockState == null || currentLockState.visionType != lockState.visionType || currentLockState.sensorType != lockState.sensorType) {
                    LowVisibility.Logger.LogIfDebug($"Target:{ActorLabel(target)} lockState changed, addings to refresh targets.");
                    visibilityUpdates.Add(target.GUID);
                }
            }
        }

        // TODO: Allies don't impact this calculation
        public static LockState CalculateLock(AbstractActor source, AbstractActor target) {

            LockState lockState = new LockState {
                sourceGUID = source.GUID,
                targetGUID = target.GUID,
                visionType = VisionLockType.None,
                sensorType = SensorLockType.None,
            };

            ActorEWConfig sourceEWConfig = State.GetOrCreateActorEWConfig(source);
            RoundDetectRange roundDetect = State.GetOrCreateRoundDetectResults(source);
            LowVisibility.Logger.Log($"  -- actor:{ActorLabel(source)} has roundCheck:{roundDetect} and ewConfig:{sourceEWConfig}");

            // Determine visual lock level
            VisibilityLevelAndAttribution visLevelAndAttrib = source.VisibilityCache.VisibilityToTarget(target);
            if (visLevelAndAttrib.VisibilityLevel == VisibilityLevel.LOSFull) {
                lockState.visionType = VisionLockType.Silhouette;
            }

            float distance = Vector3.Distance(source.CurrentPosition, target.CurrentPosition);
            float targetVisibility = CalculateTargetVisibility(target);

            float pilotVisualLockRange = GetVisualIDRangeForActor(source);
            float visualLockRange = pilotVisualLockRange * targetVisibility;

            if (distance <= visualLockRange) { lockState.visionType = VisionLockType.VisualID; }
            LowVisibility.Logger.Log($"  -- source:{ActorLabel(source)} has visualLockRange:{visualLockRange} and is distance:{distance} " +
                $"from target:{ActorLabel(target)} with visibility:{targetVisibility} - visionLockType:{lockState.visionType}");

            // Determine sensor lock level
            ActorEWConfig targetEWConfig = State.GetOrCreateActorEWConfig(target);
            if (targetEWConfig.stealthTier > sourceEWConfig.probeTier) {
                LowVisibility.Logger.Log($"  -- target:{ActorLabel(target)} has stealth of a higher tier than source:{ActorLabel(source)}'s probe. It cannot be detected.");
                lockState.sensorType = SensorLockType.None;
            } else {
                float sourceSensorRange = CalculateSensorRange(source);
                float targetSignature = CalculateTargetSignature(target);
                float lockRange = sourceSensorRange * targetSignature;
                if (distance <= lockRange) {
                    if (sourceEWConfig.probeTier >= 0 && !State.IsJammed(source)) {
                        lockState.sensorType = SensorLockType.ProbeID;
                    } else {
                        lockState.sensorType = SensorLockType.SensorID;
                    }
                }
                LowVisibility.Logger.Log($"  -- source:{ActorLabel(source)} has sensorsRange:{sourceSensorRange} and is distance:{distance} " +
                    $"from target:{ActorLabel(target)} with signature:{targetSignature} - sensorLockType:{lockState.sensorType}");

            }

            return lockState;
        }

    }

}
