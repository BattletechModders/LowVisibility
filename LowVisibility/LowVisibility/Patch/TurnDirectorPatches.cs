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

        public static bool IsFromSave = false;

        public static void Prefix(TurnDirector __instance) {
            Mod.Log.Log("=== TurnDirector:OnEncounterBegin:pre - entered.");

            // Initialize the probabilities
            State.InitializeCheckResults();
            State.InitMapConfig();
            State.TurnDirectorStarted = true;

            // Do a pre-encounter populate 
            if (__instance != null && __instance.Combat != null && __instance.Combat.AllActors != null) {
                // If we are coming from a save, don't recalculate everything - just roll with what we already have
                if (!IsFromSave) {
                    AbstractActor randomPlayerActor = null;
                    foreach (AbstractActor actor in __instance.Combat.AllActors) {
                        if (actor != null) {
                            // Parse their EW config
                            EWState actorEWConfig = new EWState(actor);
                            State.EWState[actor.GUID] = actorEWConfig;

                            // Make a pre-encounter detectCheck for them
                            State.BuildEWState(actor);
                            Mod.Log.LogIfDebug($"  Actor:{CombatantHelper.Label(actor)} has rangeCheck:{State.GetEWState(actor).rangeCheck} at load/start");

                            bool isPlayer = actor.TeamId == __instance.Combat.LocalPlayerTeamGuid;
                            if (isPlayer && randomPlayerActor == null) {
                                randomPlayerActor = actor;
                            }

                        } else {
                            Mod.Log.LogIfDebug($"  Actor:{CombatantHelper.Label(actor)} was NULL!");
                        }
                    }
                }

            }
        }
    }

    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Prefix(TurnDirector __instance, int round) {
            Mod.Log.Log($"=== TurnDirector - Beginning round:{round}");

            // Update the current vision for all allied and friendly units
            foreach (AbstractActor actor in __instance.Combat.AllActors) {

                if (__instance.CurrentRound == 0) {
                    State.BuildEWState(actor);                    
                } else {
                    EWState ewState = State.GetEWState(actor);
                    ewState.UpdateChecks();

                    if (State.ECMJamming(actor) > 0) {
                        // Send a floatie indicating the jamming
                        MessageCenter mc = __instance.Combat.MessageCenter;
                        mc.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "SENSOR CHECK FAILED!", FloatieMessage.MessageNature.Debuff));
                    }
                }

            }

        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Postfix(TurnDirector __instance) {
            // Remove all combat state
            State.ClearStateOnCombatGameDestroyed();
            CombatHUD_SubscribeToMessages.OnCombatGameDestroyed(__instance.Combat);
        }
    }

    [HarmonyPatch()]
    public static class EncounterLayerParent_InitFromSavePassTwo {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(EncounterLayerParent), "InitFromSavePassTwo", new Type[] { typeof(CombatGameState) });
        }

        public static void Postfix(EncounterLayerParent __instance, CombatGameState combat) {
            Mod.Log.Log("EncounterLayerParent:InitFromSavePassTwo:post - entered.");

            TurnDirector_OnEncounterBegin.IsFromSave = true;
        }
    }
}
