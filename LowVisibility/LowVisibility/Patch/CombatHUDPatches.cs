using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    // Show the targeting computer for blips as well as LOSFull
    [HarmonyPatch(typeof(CombatHUD), "SubscribeToMessages")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class CombatHUD_SubscribeToMessages {

        private static CombatGameState Combat = null;
        private static CombatHUDTargetingComputer TargetingComputer = null;
        private static Traverse ShowTargetMethod = null;

        public static void Postfix(CombatHUD __instance, bool shouldAdd) {
            //LowVisibility.Logger.LogIfDebug("CombatHUD:SubscribeToMessages:post - entered.");
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
            //LowVisibility.Logger.LogIfDebug("CombatHUD:SubscribeToMessages:OnActorTargeted - entered.");
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

    // Show some elements on the Targeting Computer that are normally hidden from blips
    [HarmonyPatch()]
    public static class CombatHUDActorInfo_UpdateItemVisibility {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDActorInfo), "UpdateItemVisibility", new Type[] { });
        }

        public static void Postfix(CombatHUDActorInfo __instance, AbstractActor ___displayedActor, BattleTech.Building ___displayedBuilding, ICombatant ___displayedCombatant) {
            bool isEnemyOrNeutral = false;
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
                        isEnemyOrNeutral = true;
                    }
                }
            }
            Traverse setGOActiveMethod = Traverse.Create(__instance).Method("SetGOActive", new Type[] { typeof(UnityEngine.MonoBehaviour), typeof(bool) });

            // The actual method should handle allied and friendly units fine, so we can just change it for enemies
            if (isEnemyOrNeutral && visibilityLevel > VisibilityLevel.Blip0Minimum) {
                LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(___displayedActor.Combat), ___displayedActor);

                // Values that are always displayed
                setGOActiveMethod.GetValue(__instance.NameDisplay, true);
                setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                setGOActiveMethod.GetValue(__instance.StructureBar, true);

                if (lockState.sensorType == SensorLockType.ProbeID) {
                    // Show unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, true);

                    // Show active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, true);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, true);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, true);

                    // Show init badge (if actor)
                    if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } 
                    else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                } else if (lockState.sensorType == SensorLockType.SensorID) {
                    // Show unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Show active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Show init badge (if actor)
                    if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } 
                    else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                } else if (lockState.visionType == VisionLockType.VisualID) {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                } else if (lockState.visionType == VisionLockType.Silhouette) {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Hide armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, false);
                    setGOActiveMethod.GetValue(__instance.StructureBar, false);
                } else {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Hide armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, false);
                    setGOActiveMethod.GetValue(__instance.StructureBar, false);
                }

                CombatHUDStateStack stateStack = (CombatHUDStateStack)Traverse.Create(__instance).Property("StateStack").GetValue();
                setGOActiveMethod.GetValue(stateStack, false);
            }
        }
    }
    
    [HarmonyPatch(typeof(CombatHUDWeaponSlot), "SetHitChance", new Type[] { typeof(ICombatant) })]
    public static class CombatHUDWeaponSlot_SetHitChance {

        private static void Postfix(CombatHUDWeaponSlot __instance, ICombatant target) {
            if (__instance == null || target == null) { return;  }

            AbstractActor actor = __instance.DisplayedWeapon.parent;
            AbstractActor targetActor = target as AbstractActor;
            Traverse AddToolTipDetailMethod = Traverse.Create(__instance).Method("AddToolTipDetail", new Type[] { typeof(string), typeof(int) });

            //LowVisibility.Logger.LogIfDebug($"___CombatHUDTargetingComputer - SetHitChance for source:{ActorLabel(targetActor)} target:{ActorLabel(targetActor)}");
            LockState lockState = State.GetUnifiedLockStateForTarget(actor, targetActor);
            if (lockState.sensorType == SensorLockType.None) {
                AddToolTipDetailMethod.GetValue(new object[] { "NO SENSOR LOCK", (int)LowVisibility.Config.NoSensorLockAttackPenalty });
            }
            if (lockState.visionType == VisionLockType.None) {
                AddToolTipDetailMethod.GetValue(new object[] { "NO VISUAL LOCK", (int)LowVisibility.Config.NoVisualLockAttackPenalty });
            }

            ActorEWConfig targetEWConfig = State.GetOrCreateActorEWConfig(target as AbstractActor);
            if (targetEWConfig.HasStealthRangeMod()) {
                float distance = Vector3.Distance(actor.CurrentPosition, targetActor.CurrentPosition);
                Weapon weapon = __instance.DisplayedWeapon;
                int weaponStealthMod = targetEWConfig.StealthRangeModAtDistance(weapon, distance);
                if (weaponStealthMod != 0) {
                    AddToolTipDetailMethod.GetValue(new object[] { "STEALTH - RANGE", weaponStealthMod });
                }
            }

            if (targetEWConfig.HasStealthMoveMod()) {
                int stealthMoveMod = targetEWConfig.StealthMoveModForActor(target as AbstractActor);
                if (stealthMoveMod != 0) {
                    AddToolTipDetailMethod.GetValue(new object[] { "STEALTH - MOVEMENT", stealthMoveMod});
                }
            }
        }
    }
}
