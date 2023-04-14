using BattleTech.UI;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{
    class CombatHUDActorInfoPatches
    {

        [HarmonyPatch(typeof(CombatHUDActorInfo), "OnStealthChanged")]
        public static class CombatHUDActorInfo_OnStealthChanged
        {
            public static void Postfix(CombatHUDActorInfo __instance, MessageCenterMessage message, AbstractActor ___displayedActor)
            {
                Mod.Log.Trace?.Write("CHUDAI:OSC entered");

                StealthChangedMessage stealthChangedMessage = message as StealthChangedMessage;
                if (___displayedActor != null && stealthChangedMessage.affectedObjectGuid == ___displayedActor.GUID && __instance.StealthDisplay != null)
                {
                    VfxHelper.CalculateMimeticPips(__instance.StealthDisplay, ___displayedActor);
                }
            }
        }

        [HarmonyPatch(typeof(CombatHUDActorInfo), "RefreshAllInfo")]
        public static class CombatHUDActorInfo_RefreshAllInfo
        {
            public static void Postfix(CombatHUDActorInfo __instance, AbstractActor ___displayedActor)
            {
                Mod.Log.Trace?.Write("CHUDAI:RAI entered");

                if (___displayedActor == null || ModState.LastPlayerActorActivated == null) return;

                if (__instance.StealthDisplay != null)
                {
                    VfxHelper.CalculateMimeticPips(__instance.StealthDisplay, ___displayedActor);
                }
            }
        }

        // Show some elements on the Targeting Computer that are normally hidden from blips
        [HarmonyPatch(typeof(CombatHUDActorInfo), "UpdateItemVisibility")]
        public static class CombatHUDActorInfo_UpdateItemVisibility
        {

            public static void Postfix(CombatHUDActorInfo __instance, AbstractActor ___displayedActor,
                BattleTech.Building ___displayedBuilding, ICombatant ___displayedCombatant)
            {

                if (__instance == null || ___displayedActor == null) return;

                try
                {
                    bool isEnemyOrNeutral = false;
                    VisibilityLevel visibilityLevel = VisibilityLevel.None;
                    if (___displayedCombatant != null)
                    {
                        if (___displayedCombatant.IsForcedVisible)
                        {
                            visibilityLevel = VisibilityLevel.LOSFull;
                        }
                        else if (___displayedBuilding != null)
                        {
                            visibilityLevel = __instance.Combat.LocalPlayerTeam.VisibilityToTarget(___displayedBuilding);
                        }
                        else if (___displayedActor != null)
                        {
                            if (__instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(___displayedActor.team))
                            {
                                visibilityLevel = VisibilityLevel.LOSFull;
                            }
                            else
                            {
                                visibilityLevel = __instance.Combat.LocalPlayerTeam.VisibilityToTarget(___displayedActor);
                                isEnemyOrNeutral = true;
                            }
                        }
                    }

                    // The actual method should handle allied and friendly units fine, so we can just change it for enemies
                    if (isEnemyOrNeutral && visibilityLevel >= VisibilityLevel.Blip0Minimum && ___displayedActor != null)
                    {

                        SensorScanType scanType = SensorLockHelper.CalculateSharedLock(___displayedActor, ModState.LastPlayerActorActivated);
                        bool hasVisualScan = VisualLockHelper.CanSpotTarget(ModState.LastPlayerActorActivated, ModState.LastPlayerActorActivated.CurrentPosition,
                            ___displayedActor, ___displayedActor.CurrentPosition, ___displayedActor.CurrentRotation, ___displayedActor.Combat.LOS);

                        Mod.Log.Debug?.Write($"Updating item visibility for enemy: {CombatantUtils.Label(___displayedActor)} to scanType: {scanType} and " +
                            $"hasVisualScan: {hasVisualScan} from lastActivated: {CombatantUtils.Label(ModState.LastPlayerActorActivated)}");

                        // Values that are always displayed
                        __instance.SetGOActive(__instance.NameDisplay, true);
                        __instance.SetGOActive(__instance.PhaseDisplay, true);

                        if (scanType >= SensorScanType.StructAndWeaponID)
                        {
                            // Show unit summary
                            __instance.SetGOActive(__instance.DetailsDisplay, true);

                            // Show active state
                            __instance.SetGOActive(__instance.InspiredDisplay, false);

                            // Show armor and struct
                            __instance.SetGOActive(__instance.ArmorBar, true);
                            __instance.SetGOActive(__instance.StructureBar, true);

                            if (___displayedActor as Mech != null)
                            {
                                __instance.SetGOActive(__instance.StabilityDisplay, true);
                                __instance.SetGOActive(__instance.HeatDisplay, true);
                            }
                            else
                            {
                                __instance.SetGOActive(__instance.StabilityDisplay, false);
                                __instance.SetGOActive(__instance.HeatDisplay, false);
                            }
                        }
                        else if (scanType >= SensorScanType.ArmorAndWeaponType || hasVisualScan)
                        {
                            // Show unit summary
                            __instance.SetGOActive(__instance.DetailsDisplay, false);

                            // Show active state
                            __instance.SetGOActive(__instance.InspiredDisplay, false);

                            // Show armor and struct
                            __instance.SetGOActive(__instance.ArmorBar, true);
                            __instance.SetGOActive(__instance.StructureBar, true);

                            __instance.SetGOActive(__instance.StabilityDisplay, false);
                            __instance.SetGOActive(__instance.HeatDisplay, false);
                        }
                        else
                        {
                            // Hide unit summary
                            __instance.SetGOActive(__instance.DetailsDisplay, false);

                            // Hide active state
                            __instance.SetGOActive(__instance.InspiredDisplay, false);

                            // Hide armor and struct
                            __instance.SetGOActive(__instance.ArmorBar, false);
                            __instance.SetGOActive(__instance.StructureBar, false);

                            __instance.SetGOActive(__instance.StabilityDisplay, false);
                            __instance.SetGOActive(__instance.HeatDisplay, false);
                        }

                        // TODO: DEBUG TESTING 
                        if (__instance.MarkDisplay != null)
                        {
                            __instance.SetGOActive(__instance.MarkDisplay, true);
                        }

                        __instance.SetGOActive(__instance.StateStack, false);
                    }
                    else
                    {
                        if (__instance.MarkDisplay != null && ___displayedActor != null)
                        {
                            __instance.SetGOActive(__instance.MarkDisplay, ___displayedActor.IsMarked);
                        }

                    }
                }
                catch (Exception e)
                {
                    Mod.Log.Info?.Write($"Error updating item visibility! Error was: {e.Message}");
                }
            }
        }
    }
}
