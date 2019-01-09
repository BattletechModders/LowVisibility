using BattleTech;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Reflection;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static void Prefix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("=== TurnDirector:OnEncounterBegin:pre - entered.");

            // Do a pre-encounter populate 
            if (__instance != null && __instance.Combat != null && __instance.Combat.AllActors != null) {                
                foreach (AbstractActor actor in __instance.Combat.AllActors) {
                    // Parse their EW config
                    ActorEWConfig actorEWConfig = new ActorEWConfig(actor);
                    State.ActorEWConfig[actor.GUID] = actorEWConfig;

                    // Make a pre-encounter detectCheck for them\
                    RoundDetectRange detectRange = MakeSensorRangeCheck(actor, false);
                    LowVisibility.Logger.LogIfDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has detectRange:{detectRange} at load/start");
                    State.RoundDetectResults[actor.GUID] = detectRange;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "InitFromSave")]
    public static class TurnDirector_InitFromSave {
        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:InitFromSave:post - entered.");
        }
    }

    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Prefix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("=== TurnDirector:BeginNewRound:post - entered.");

            // Update the current vision for all allied and friendly units
            State.UpdateDetectionForAllActors(__instance.Combat);
        }
    }





}
