using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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


    [HarmonyPatch(typeof(CombatHUDTargetingComputer), "Update")]
    public static class CombatHUDTargetingComputer_Update {

        private static Action<CombatHUDTargetingComputer> UIModule_Update;

        public static bool Prepare() {
            BuildCHTCOnComplete();
            return true;
        }

        // Shamelessly stolen from https://github.com/janxious/BT-WeaponRealizer/blob/7422573fa69893ae7c16a9d192d85d2152f90fa2/NumberOfShotsEnabler.cs#L32
        private static void BuildCHTCOnComplete() {
            // build a call to WeaponEffect.OnComplete() so it can be called
            // a la base.OnComplete() from the context of a BallisticEffect
            // https://blogs.msdn.microsoft.com/rmbyers/2008/08/16/invoking-a-virtual-method-non-virtually/
            // https://docs.microsoft.com/en-us/dotnet/api/system.activator?view=netframework-3.5
            // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.dynamicmethod.-ctor?view=netframework-3.5#System_Reflection_Emit_DynamicMethod__ctor_System_String_System_Type_System_Type___System_Type_
            // https://stackoverflow.com/a/4358250/1976
            var method = typeof(UIModule).GetMethod("Update", AccessTools.all);
            var dm = new DynamicMethod("CombatHUDTargetingComputerOnComplete", null, new Type[] { typeof(CombatHUDTargetingComputer) }, typeof(CombatHUDTargetingComputer));
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Ret);
            UIModule_Update = (Action<CombatHUDTargetingComputer>)dm.CreateDelegate(typeof(Action<CombatHUDTargetingComputer>));
        }

        // TODO: Dangerous PREFIX false here!
        public static bool Prefix(CombatHUDTargetingComputer __instance, CombatHUD ___HUD) {
            LowVisibility.Logger.LogIfDebug("CombatHUDTargetingComputer:Update:pre - entered.");

            CombatGameState Combat = ___HUD?.Combat;

            UIModule_Update(__instance);
            if (__instance.ActorInfo != null) {
                __instance.ActorInfo.DisplayedCombatant = __instance.ActivelyShownCombatant;
            }
            if (__instance.ActivelyShownCombatant == null ||
                (__instance.ActivelyShownCombatant.team != Combat.LocalPlayerTeam
                    && !Combat.HostilityMatrix.IsFriendly(__instance.ActivelyShownCombatant.team.GUID, Combat.LocalPlayerTeamGuid)
                    && Combat.LocalPlayerTeam.VisibilityToTarget(__instance.ActivelyShownCombatant) < VisibilityLevel.Blip0Minimum)
                    ) {
                if (__instance.Visible) {
                    __instance.Visible = false;
                }
            } else {
                if (!__instance.Visible) {
                    __instance.Visible = true;
                }
                if (__instance.ActivelyShownCombatant != null) {
                    Traverse method = Traverse.Create(__instance).Method("UpdateStructureAndArmor", new Type[] { });
                    method.GetValue();
                }
            }

            return false;
        }


        //public static void Postfix(CombatHUDTargetingComputer __instance, CombatHUD ___HUD) {
        //    LowVisibility.Logger.LogIfDebug("CombatHUDTargetingComputer:Update:post - entered.");

        //    CombatGameState Combat = ___HUD?.Combat;
        //    if (Combat != null && __instance.ActivelyShownCombatant != null
        //        && !Combat.HostilityMatrix.IsFriendly(__instance.ActivelyShownCombatant.team.GUID, Combat.LocalPlayerTeamGuid)
        //        && Combat.LocalPlayerTeam.VisibilityToTarget(__instance.ActivelyShownCombatant) >= VisibilityLevel.Blip0Minimum) {
        //        __instance.Visible = true; // base.Visible = true;
        //        Traverse method = Traverse.Create(__instance).Method("UpdateStructureAndArmor", new Type[] { });
        //        method.GetValue();
        //    }
        //}

    }

    // TODO: Appears unused?
    [HarmonyPatch(typeof(CombatHUD), "SubscribeToMessages")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class CombatHUD_SubscribeToMessages {

        private static CombatGameState Combat = null;
        private static CombatHUDTargetingComputer TargetingComputer = null;
        private static Traverse ShowTargetMethod = null;

        public static void Postfix(CombatHUD __instance, bool shouldAdd) {
            LowVisibility.Logger.LogIfDebug("CombatHUD:SubscribeToMessages:post - entered.");
            if (shouldAdd) {
                Combat = __instance.Combat;
                TargetingComputer = __instance.TargetingComputer;
                ShowTargetMethod = Traverse.Create(__instance).Method("ShowTarget", new Type[] { typeof(ICombatant) });
                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(OnActorTargeted), shouldAdd);
                // Disable the previous registration 
                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(__instance.OnActorTargetedMessage), false);
            } else {
                Combat = null;
                TargetingComputer = null;
                ShowTargetMethod = null;
                __instance.Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorTargetedMessage,
                    new ReceiveMessageCenterMessage(OnActorTargeted), shouldAdd);
            }

        }

        public static void OnActorTargeted(MessageCenterMessage message) {
            LowVisibility.Logger.LogIfDebug("CombatHUD:SubscribeToMessages:OnActorTargeted - entered.");
            ActorTargetedMessage actorTargetedMessage = message as ActorTargetedMessage;
            ICombatant combatant = Combat.FindActorByGUID(actorTargetedMessage.affectedObjectGuid);
            if (combatant == null) { combatant = Combat.FindCombatantByGUID(actorTargetedMessage.affectedObjectGuid); }
            if (Combat.LocalPlayerTeam.VisibilityToTarget(combatant) >= VisibilityLevel.Blip0Minimum) {
                LowVisibility.Logger.LogIfDebug("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility >= Blip0, showing target.");
                ShowTargetMethod.GetValue(combatant);
            } else {
                LowVisibility.Logger.LogIfDebug("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility < Blip0, hiding target.");
            }
        }
    }

    // TODO: Duplicate work - make prefix if necessary?
    [HarmonyPatch()]
    public static class FiringPreviewManager_HasLOS {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(FiringPreviewManager), "HasLOS", new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(UnityEngine.Vector3), typeof(List<AbstractActor>) });
        }

        public static void Postfix(FiringPreviewManager __instance, ref bool __result, CombatGameState ___combat,
            AbstractActor attacker, ICombatant target, UnityEngine.Vector3 position, List<AbstractActor> allies) {
            //LowVisibility.Logger.LogIfDebug("FiringPreviewManager:HasLOS:post - entered.");
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
            //LowVisibility.Logger.LogIfDebug("AbstractActor:HasLOSToTargetUnit:post - entered.");
            __result = __instance.VisibilityToTargetUnit(targetUnit) >= VisibilityLevel.Blip0Minimum;
        }
    }

    [HarmonyPatch()]
    public static class CombatHUDActorInfo_UpdateItemVisibility {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDActorInfo), "UpdateItemVisibility", new Type[] { });
        }

        public static void Postfix(CombatHUDActorInfo __instance, AbstractActor ___displayedActor, BattleTech.Building ___displayedBuilding, ICombatant ___displayedCombatant) {
            VisibilityLevel visibilityLevel = VisibilityLevel.None;
            if (___displayedCombatant != null) {
                if (___displayedCombatant.IsForcedVisible) {
                    visibilityLevel = VisibilityLevel.LOSFull;
                } else if (___displayedBuilding != null) {
                    visibilityLevel = __instance.Combat.LocalPlayerTeam.VisibilityToTarget(___displayedBuilding);
                } else if (___displayedActor != null) {
                    if (__instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(___displayedActor.team)) {
                        visibilityLevel = VisibilityLevel.LOSFull;
                    } else {
                        visibilityLevel = __instance.Combat.LocalPlayerTeam.VisibilityToTarget(___displayedActor);
                    }
                }
            }
            Traverse setGOActiveMethod = Traverse.Create(__instance).Method("SetGOActive", new Type[] { typeof(UnityEngine.MonoBehaviour), typeof(bool) });

            if (visibilityLevel > VisibilityLevel.Blip0Minimum) {
                setGOActiveMethod.GetValue(__instance.NameDisplay, true);
                if (___displayedBuilding != null) {
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                } else {
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                }
                if (___displayedActor != null) {
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, true);
                } else {
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);
                }
                setGOActiveMethod.GetValue(__instance.DetailsDisplay, true);
                setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                CombatHUDStateStack stateStack = (CombatHUDStateStack)Traverse.Create(__instance).Property("StateStack").GetValue();
                setGOActiveMethod.GetValue(stateStack, false);
            }
        }
    }
}
