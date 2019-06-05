using BattleTech;
using Harmony;
using LowVisibility.Helper;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Initializes any custom status effects. Without this, they wont' be read.
    [HarmonyPatch(typeof(AbstractActor), "InitEffectStats")]
    public static class AbstractActor_InitEffectStats {
        private static void Postfix(AbstractActor __instance) {
            Mod.Log.Trace("AA:IES entered");

            __instance.StatCollection.Set(ModStats.Check, 0);

            __instance.StatCollection.Set(ModStats.Jammer, 0);
            __instance.StatCollection.Set(ModStats.Probe, 0);
            __instance.StatCollection.Set(ModStats.Stealth, 0);

            __instance.StatCollection.Set(ModStats.SharesSensors, false);

            __instance.StatCollection.Set(ModStats.StealthMoveMod, 0);
            __instance.StatCollection.Set(ModStats.VismodeZoom, 0);
            __instance.StatCollection.Set(ModStats.VismodeHeat, 0);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {

        public static void Prefix(AbstractActor __instance, int stackItemID) {
            if (stackItemID == -1 || __instance == null || __instance.HasBegunActivation) {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DOES NOTHING. GAH!
                return;
            }
            
            //ECMHelper.UpdateECMState(__instance);

            //VisibilityHelper.UpdateVisibilityForAllTeams(__instance.Combat);

            Mod.Log.Trace($"=== AbstractActor:OnActivationBegin:pre - processing {CombatantUtils.Label(__instance)}");
            if (__instance.team == __instance.Combat.LocalPlayerTeam) {
                State.LastPlayerActor = __instance.GUID;
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "UpdateLOSPositions")]
    public static class AbstractActor_UpdateLOSPositions {
        public static void Prefix(AbstractActor __instance) {
            // Check for teamID; if it's not present, unit hasn't spawned yet. Defer to UnitSpawnPointGameLogic::SpawnUnit for these updates
            if (State.TurnDirectorStarted && __instance.TeamId != null) {
                Mod.Log.Debug($"AbstractActor_UpdateLOSPositions:pre - entered for {CombatantUtils.Label(__instance)}.");
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "HasLOSToTargetUnit")]
    public static class AbstractActor_HasLOSToTargetUnit {
        public static void Postfix(AbstractActor __instance, ref bool __result, ICombatant targetUnit) {
            //LowVisibility.Logger.Debug("AbstractActor:HasLOSToTargetUnit:post - entered.");

            // Forces you to be able to see targets that are only blips
            __result = __instance.VisibilityToTargetUnit(targetUnit) >= VisibilityLevel.Blip0Minimum;
            Mod.Log.Trace($"Actor{CombatantUtils.Label(__instance)} has LOSToTargetUnit? {__result} " +
                $"to target:{CombatantUtils.Label(targetUnit as AbstractActor)}");
            //LowVisibility.Logger.Trace($"Called from:{new StackTrace(true)}");
        }
    }


    [HarmonyPatch(typeof(AbstractActor), "CanDetectPositionNonCached")]
    public static class AbstractActor_CanDetectPositionNonCached {
        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CDPNC: source{CombatantUtils.Label(__instance)} checking detection " +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CanSeeTargetAtPositionNonCached")]
    public static class AbstractActor_CanSeeTargetAtPositionNonCached {
        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CSTAPNC: source{__instance} checking vision" +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }
}
