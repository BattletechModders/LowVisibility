using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Reflection;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static bool IsFromSave = false;

        public static void Prefix(TurnDirector __instance) {
            Mod.Log.Trace("TD:OEB:pre entered.");

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
                            ActorHelper.UpdateSensorCheck(actor);

                            bool isPlayer = actor.TeamId == __instance.Combat.LocalPlayerTeamGuid;
                            if (isPlayer && randomPlayerActor == null) {
                                randomPlayerActor = actor;
                            }

                        } else {
                            Mod.Log.Debug($"  Actor:{CombatantUtils.Label(actor)} was NULL!");
                        }
                    }
                }

            }

            // Initialize the VFX materials
            VfxHelper.Initialize();
        }
    }

    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Prefix(TurnDirector __instance, int round) {
            Mod.Log.Trace($"TD:BNR entered");
            Mod.Log.Debug($"=== TurnDirector - Beginning round:{round}");

            // Update the current vision for all allied and friendly units
            foreach (AbstractActor actor in __instance.Combat.AllActors) {
                ActorHelper.UpdateSensorCheck(actor);
            }

        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Postfix(TurnDirector __instance) {
            Mod.Log.Trace($"TD:OCGD entered");
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
            Mod.Log.Trace($"TD:IFSPT entered");

            TurnDirector_OnEncounterBegin.IsFromSave = true;
        }
    }
}
