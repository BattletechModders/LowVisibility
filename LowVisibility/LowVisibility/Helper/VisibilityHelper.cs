using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void PublishVisibilityChange(CombatGameState Combat, HashSet<string> targets) {
            foreach (String targetGUID in targets) {
                MessageCenter mc = Combat.MessageCenter;
                mc.PublishMessage(new PlayerVisibilityChangedMessage(targetGUID));
                LowVisibility.Logger.LogIfDebug($"Publishing state change for target:{targetGUID} -> actor:{ActorLabel(Combat.FindActorByGUID(targetGUID))}");
            }
        }

        public static void CalculateTargetLocks(AbstractActor source, HashSet<LockState> updatedLocks, HashSet<string> visibilityUpdates) {

            List<AbstractActor> enemyOrNeutralActors = EnemyAndNeutralActors(source.Combat);
            foreach (AbstractActor target in enemyOrNeutralActors) {
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
    }

}
