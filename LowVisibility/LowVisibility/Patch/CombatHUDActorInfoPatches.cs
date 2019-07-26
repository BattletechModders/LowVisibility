using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Reflection;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {
    class CombatHUDActorInfoPatches {

        [HarmonyPatch(typeof(CombatHUDActorInfo), "OnStealthChanged")]
        public static class CombatHUDActorInfo_OnStealthChanged {
            public static void Postfix(CombatHUDActorInfo __instance, MessageCenterMessage message, AbstractActor ___displayedActor) {
                Mod.Log.Debug("CHUDAI:OSC entered");

                StealthChangedMessage stealthChangedMessage = message as StealthChangedMessage;
                if (___displayedActor != null && stealthChangedMessage.affectedObjectGuid == ___displayedActor.GUID && __instance.StealthDisplay != null) {
                    VfxHelper.CalculateMimeticPips(__instance.StealthDisplay, ___displayedActor);
                }
            }
        }

        [HarmonyPatch(typeof(CombatHUDActorInfo), "RefreshAllInfo")]
        public static class CombatHUDActorInfo_RefreshAllInfo {
            public static void Postfix(CombatHUDActorInfo __instance, AbstractActor ___displayedActor) {
                Mod.Log.Trace("CHUDAI:RAI entered");

                if (___displayedActor != null && __instance.StealthDisplay != null) {
                    VfxHelper.CalculateMimeticPips(__instance.StealthDisplay, ___displayedActor);
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

                    SensorScanType scanType = SensorLockHelper.CalculateSharedLock(___displayedActor, State.LastPlayerActorActivated);

                    // Values that are always displayed
                    setGOActiveMethod.GetValue(__instance.NameDisplay, true);

                    if (scanType >= SensorScanType.StructureAnalysis) {
                        // Show unit summary
                        setGOActiveMethod.GetValue(__instance.DetailsDisplay, true);

                        // Show active state
                        setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                        setGOActiveMethod.GetValue(__instance.MarkDisplay, true);

                        // Show init badge (if actor)
                        if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

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
                    } else if (scanType >= SensorScanType.SurfaceScan) {
                        // Show unit summary
                        setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                        // Show active state
                        setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                        setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                        // Show init badge (if actor)
                        if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

                        // Show armor and struct
                        setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                        setGOActiveMethod.GetValue(__instance.StructureBar, true);

                        setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                        setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    } else if (State.LastPlayerActorActivated.VisibilityToTargetUnit(___displayedActor) == VisibilityLevel.LOSFull) {
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

                        if (___displayedActor as Mech != null) {
                            setGOActiveMethod.GetValue(__instance.StabilityDisplay, true);
                            setGOActiveMethod.GetValue(__instance.HeatDisplay, true);
                        } else {
                            setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                            setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                        }
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
    }
}
