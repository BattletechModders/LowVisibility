using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Reflection;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static void Prefix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfTrace("=== TurnDirector:OnEncounterBegin:pre - entered.");

            // Initialize the probabilities
            State.InitializeCheckResults();
            State.InitMapConfig();
            State.TurnDirectorStarted = true;

            // Do a pre-encounter populate 
            if (__instance != null && __instance.Combat != null && __instance.Combat.AllActors != null) {
                AbstractActor randomPlayerActor = null;
                foreach (AbstractActor actor in __instance.Combat.AllActors) {
                    if (actor != null) {
                        // Parse their EW config
                        StaticEWState actorEWConfig = new StaticEWState(actor);
                        State.StaticEWState[actor.GUID] = actorEWConfig;

                        // Make a pre-encounter detectCheck for them
                        State.BuildDynamicState(actor);
                        LowVisibility.Logger.LogIfDebug($"  Actor:{CombatantHelper.Label(actor)} has detectCheck:{State.GetDynamicState(actor).ToString()} at load/start");

                        bool isPlayer = actor.TeamId == __instance.Combat.LocalPlayerTeamGuid;
                        if (isPlayer && randomPlayerActor == null) {
                            randomPlayerActor = actor;
                        }

                    } else {
                        LowVisibility.Logger.LogIfDebug($"  Actor:{CombatantHelper.Label(actor)} was NULL!");
                    }
                }
                VisibilityHelper.UpdateDetectionForAllActors(__instance.Combat);
                VisibilityHelper.UpdateVisibilityForAllTeams(__instance.Combat);
            }
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "InitFromSave")]
    public static class TurnDirector_InitFromSave {
        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:InitFromSave:post - entered.");
            // TODO: VERIFY THIS RUNS WITH OR WITHOUT INIT()
            State.InitMapConfig();
        }
    }

    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Prefix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfTrace("=== TurnDirector:BeginNewRound:post - entered.");

            // Update the current vision for all allied and friendly units
            foreach (AbstractActor actor in __instance.Combat.AllActors) {                
                

                if (LowVisibility.Config.FirstTurnForceFailedChecks && __instance.CurrentRound == 0) {
                    LowVisibility.Logger.Log("=== TurnDirector:Forcing sensor checks to negative values for first round.");
                    State.DynamicEWState[actor.GUID] = new DynamicEWState {
                        detailCheck = -15,
                        rangeCheck = -15
                    };
                } else {
                    State.BuildDynamicState(actor);
                }

                DynamicEWState dynamicState = State.GetDynamicState(actor);
                if (dynamicState.detailCheck < 0 || dynamicState.rangeCheck < 0) {
                    // Send a floatie indicating the jamming
                    MessageCenter mc = __instance.Combat.MessageCenter;
                    mc.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "SENSOR CHECK FAILED!", FloatieMessage.MessageNature.Debuff));
                }
            }

            VisibilityHelper.UpdateDetectionForAllActors(__instance.Combat);
            VisibilityHelper.UpdateVisibilityForAllTeams(__instance.Combat);
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Postfix(TurnDirector __instance) {
            // Remove all combat state
            State.DynamicEWState.Clear();
            State.StaticEWState.Clear();
            State.SourceActorLockStates.Clear();
            State.LastPlayerActivatedActorGUID = null;
            State.ECMJammedActors.Clear();
            State.ECMProtectedActors.Clear();
            State.TurnDirectorStarted = false;
        }
    }


}
