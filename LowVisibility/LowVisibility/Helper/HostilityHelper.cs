using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LowVisibility.Helper {
    public static class HostilityHelper {

        public static bool IsPlayer(AbstractActor actor) {
            return actor != null && actor.team == actor.Combat.LocalPlayerTeam;
        }
        public static bool IsLocalPlayerEnemy(AbstractActor actor) {
            return actor != null && actor.Combat.HostilityMatrix.IsLocalPlayerEnemy(actor.team);
        }
        public static bool IsLocalPlayerNeutral(AbstractActor actor) {
            return actor != null && actor.Combat.HostilityMatrix.IsLocalPlayerNeutral(actor.team);
        }
        public static bool IsLocalPlayerAlly(AbstractActor actor) {
            return actor != null && actor.Combat.HostilityMatrix.IsLocalPlayerFriendly(actor.team) && !IsPlayer(actor);
        }

        public static List<AbstractActor> PlayerActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => aa.TeamId == Combat.LocalPlayerTeamGuid)
                .ToList();
        }

        public static List<AbstractActor> AlliedToLocalPlayerActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => Combat.HostilityMatrix.IsLocalPlayerFriendly(aa.TeamId) && aa.TeamId != Combat.LocalPlayerTeamGuid)
                .ToList();
        }

        public static List<AbstractActor> EnemyToLocalPlayerActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => Combat.HostilityMatrix.IsLocalPlayerEnemy(aa.TeamId))
                .ToList();
        }

        public static List<AbstractActor> NeutralToLocalPlayerActors(CombatGameState Combat) {
            return Combat.AllActors
                .Where(aa => Combat.HostilityMatrix.IsLocalPlayerNeutral(aa.TeamId))
                .ToList();
        }
    }
}
