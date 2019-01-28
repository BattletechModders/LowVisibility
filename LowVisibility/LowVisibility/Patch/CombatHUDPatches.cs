using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.VisibilityHelper;
using static LowVisibility.Object.EWState;

namespace LowVisibility.Patch {

    // Show the targeting computer for blips as well as LOSFull
    [HarmonyPatch(typeof(CombatHUD), "SubscribeToMessages")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class CombatHUD_SubscribeToMessages {

        private static CombatGameState Combat = null;
        private static CombatHUDTargetingComputer TargetingComputer = null;
        private static Traverse ShowTargetMethod = null;

        // TODO: Should cleanup the subscribe here - may be causing teardown bugs
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
                LowVisibility.Logger.LogIfTrace("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility >= Blip0, showing target.");
                ShowTargetMethod.GetValue(combatant);
            } else {
                LowVisibility.Logger.LogIfTrace("CombatHUD:SubscribeToMessages:OnActorTargeted - Visibility < Blip0, hiding target.");
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

        public static void Postfix(CombatHUDActorInfo __instance, AbstractActor ___displayedActor, 
            BattleTech.Building ___displayedBuilding, ICombatant ___displayedCombatant) {

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

            Traverse setGOActiveMethod = Traverse.Create(__instance).Method("SetGOActive", new Type[] { typeof(MonoBehaviour), typeof(bool) });
            // The actual method should handle allied and friendly units fine, so we can just change it for enemies
            if (isEnemyOrNeutral && visibilityLevel > VisibilityLevel.Blip0Minimum && ___displayedActor != null) {
                Locks lockState = State.LastActivatedLocksForTarget(___displayedActor);

                // Values that are always displayed
                setGOActiveMethod.GetValue(__instance.NameDisplay, true);

                if (lockState.sensorLock >= SensorLockType.StructureAnalysis) {
                    // Show unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, true);

                    // Show active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, true);

                    // Show init badge (if actor)
                    if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } 
                    else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);

                    if (___displayedActor as Mech != null) {
                        setGOActiveMethod.GetValue(__instance.StabilityDisplay, true);
                        setGOActiveMethod.GetValue(__instance.HeatDisplay, true);
                    } else {
                        setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                        setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    }
                } else if (lockState.sensorLock >= SensorLockType.SurfaceScan) {
                    // Show unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Show active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Show init badge (if actor)
                    if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } 
                    else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);

                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                } else if (lockState.visualLock == VisualLockType.VisualScan) {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);

                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, true);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, true);
                } else {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Hide armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, false);
                    setGOActiveMethod.GetValue(__instance.StructureBar, false);

                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
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

            if (targetActor != null && __instance.DisplayedWeapon != null) {
                //LowVisibility.Logger.LogIfDebug($"___CombatHUDTargetingComputer - SetHitChance for source:{CombatantHelper.Label(targetActor)} target:{CombatantHelper.Label(targetActor)}");
                Locks lockState = State.LocksForTarget(actor, targetActor);
                float distance = Vector3.Distance(actor.CurrentPosition, targetActor.CurrentPosition);
                EWState attackerEWConfig = State.GetEWState(actor);

                if (lockState.sensorLock == SensorLockType.NoInfo) {
                    AddToolTipDetailMethod.GetValue(new object[] { "NO SENSOR LOCK", LowVisibility.Config.VisionOnlyPenalty});
                }

                if (lockState.visualLock == VisualLockType.None) {
                    AddToolTipDetailMethod.GetValue(new object[] { "NO VISUAL LOCK", LowVisibility.Config.SensorsOnlyPenalty });
                }
                
                VisionModeModifer vismodeMod = attackerEWConfig.CalculateVisionModeModifier(target, distance, __instance.DisplayedWeapon);
                if (vismodeMod.modifier != 0) {
                    AddToolTipDetailMethod.GetValue(new object[] { vismodeMod.label, vismodeMod.modifier });
                }

                EWState targetEWConfig = State.GetEWState(target as AbstractActor);
                if (targetEWConfig.HasStealthRangeMod()) {
                    Weapon weapon = __instance.DisplayedWeapon;
                    int weaponStealthMod = targetEWConfig.CalculateStealthRangeMod(weapon, distance);
                    if (weaponStealthMod != 0) {
                        AddToolTipDetailMethod.GetValue(new object[] { "STEALTH - RANGE", weaponStealthMod });
                    }
                }

                if (targetEWConfig.HasStealthMoveMod()) {
                    int stealthMoveMod = targetEWConfig.CalculateStealthMoveMod(target as AbstractActor);
                    if (stealthMoveMod != 0) {
                        AddToolTipDetailMethod.GetValue(new object[] { "STEALTH - MOVEMENT", stealthMoveMod });
                    }
                }
            }          
        }
    }
}
