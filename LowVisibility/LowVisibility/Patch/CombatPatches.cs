using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:OnEncounterBegin:post - entered.");
        }
    }

    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:BeginNewRound:post - entered.");
            foreach (AbstractActor actor in __instance.Combat.AllActors) {
                RoundDetectRange detectRange = ActorHelper.MakeSensorRangeCheck(actor);
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has detectRange:{detectRange} this round!");
                State.roundDetectResults[actor.GUID] = detectRange;
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {
        public static void Prefix(AbstractActor __instance) {
            LowVisibility.Logger.LogIfDebug("AbstractActor:OnActivationBegin:post - entered.");

        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Mech), "OnMoveComplete")]
    public static class Mech_OnMoveComplete {
        public static void Postfix(Mech __instance) {
            LowVisibility.Logger.LogIfDebug($"Mech:OnMoveComplete:post - entered.");

        }
    }


    [HarmonyPatch()]
    public static class CombatHUDTargetingComputer_OnActorHovered {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDTargetingComputer), "OnActorHovered", new Type[] { typeof(MessageCenterMessage) });
        }

        public static void Postfix(CombatHUDTargetingComputer __instance, MessageCenterMessage message, CombatHUD ___HUD) {
            LowVisibility.Logger.LogIfDebug("CombatHUDTargetingComputer:OnActorHovered:post - entered.");

            if (__instance != null) {

                EncounterObjectMessage encounterObjectMessage = message as EncounterObjectMessage;
                ICombatant combatant = ___HUD.Combat.FindCombatantByGUID(encounterObjectMessage.affectedObjectGuid);
                if (combatant != null) {
                    AbstractActor abstractActor = combatant as AbstractActor;
                    if (combatant.team != ___HUD.Combat.LocalPlayerTeam && (abstractActor == null ||
                        ___HUD.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor) >= VisibilityLevel.Blip0Minimum)) {
                        Traverse.Create(__instance).Property("HoveredCombatant").SetValue(combatant);
                    }
                    /*
                    if (combatant.team != this.HUD.Combat.LocalPlayerTeam && 
                        (abstractActor == null || this.HUD.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor) == VisibilityLevel.LOSFull))
				    {
					    this.HoveredCombatant = combatant;
				    }
                     */
                }
            }

        }
    }


    // TODO: Appears unused?
    [HarmonyPatch(typeof(CombatHUD), "OnActorTargetedMessage")]
    public static class CombatHUD_OnActorTargetedMessage {
        public static void Postfix(CombatHUD __instance, MessageCenterMessage message) {
            LowVisibility.Logger.LogIfDebug("CombatHUD:OnActorTargetedMessage:post - entered.");

            ActorTargetedMessage actorTargetedMessage = message as ActorTargetedMessage;
            ICombatant combatant = __instance.Combat.FindActorByGUID(actorTargetedMessage.affectedObjectGuid);
            if (combatant == null) {
                combatant = __instance.Combat.FindCombatantByGUID(actorTargetedMessage.affectedObjectGuid);
            }
            if (__instance.Combat.LocalPlayerTeam.VisibilityToTarget(combatant) >= VisibilityLevel.Blip0Minimum) {
                Traverse method = Traverse.Create(__instance).Method("ShowTarget", new Type[] { typeof(ICombatant) });
                method.GetValue(combatant);                
            }
        }
    }

    // TODO: Duplicate work - make prefix if necessary?
    [HarmonyPatch()]
    public static class FiringPreviewManager_HasLOS {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(FiringPreviewManager), "HasLOS", new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(List<AbstractActor>) });
        }

        public static void Postfix(FiringPreviewManager __instance, ref bool __result, CombatGameState ___combat,
            AbstractActor attacker, ICombatant target, Vector3 position, List<AbstractActor> allies) {
            LowVisibility.Logger.LogIfDebug("FiringPreviewManager:HasLOS:post - entered.");
            for (int i = 0; i < allies.Count; i++) {
                //if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel == VisibilityLevel.LOSFull) {
                if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel >= VisibilityLevel.Blip0Minimum) {
                    __result = true;
                }
            }
            VisibilityLevel visibilityToTargetWithPositionsAndRotations = 
                ___combat.LOS.GetVisibilityToTargetWithPositionsAndRotations(attacker, position, target);
            //__result = visibilityToTargetWithPositionsAndRotations == VisibilityLevel.LOSFull;
            __result = visibilityToTargetWithPositionsAndRotations >= VisibilityLevel.Blip0Minimum;
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "HasLOSToTargetUnit")]
    public static class AbstractActor_HasLOSToTargetUnit {
        public static void Postfix(AbstractActor __instance, ref bool __result, ICombatant targetUnit) {
            LowVisibility.Logger.LogIfDebug("AbstractActor:HasLOSToTargetUnit:post - entered.");
            __result = __instance.VisibilityToTargetUnit(targetUnit) >= VisibilityLevel.Blip0Minimum;
        }
    }

}
