using BattleTech;
using BattleTech.Save.Test;
using Harmony;
using LowVisibility.Helper;
using System;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(TurnDirector), "Dehydrate")]
    [HarmonyPatch(new Type[] { typeof(SerializableReferenceContainer) })]
    public static class TurnDirector_Dehydrate {
        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:Dehydrate:post - entered.");
        }        
    }

    [HarmonyPatch(typeof(TurnDirector), "Hydrate")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(SerializableReferenceContainer) })]
    public static class TurnDirector_Hydrate {
        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:Hydrate:post - entered.");
            foreach (AbstractActor actor in __instance.Combat.AllActors) {
                RoundDetectRange detectRange = ActorHelper.MakeSensorRangeCheck(actor);
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has detectRange:{detectRange} this round!");
                State.roundDetectResults[actor.GUID] = detectRange;
            }
        }
    }

}
