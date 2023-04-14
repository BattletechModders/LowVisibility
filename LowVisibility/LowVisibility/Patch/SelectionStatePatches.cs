using BattleTech.UI;
using LowVisibility.Helper;
using LowVisibility.Object;
using System.Collections.Generic;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{

    [HarmonyPatch(typeof(SelectionStateFire), "ProcessClickedCombatant")]
    static class SelectionStateFire_ProcessClickedCombatant
    {

        static void Prefix(ref bool __runOriginal, SelectionStateFire __instance, ref bool __result, ICombatant combatant)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Write("SSF:PCC:PRE entered");

            if (__instance != null && combatant != null && combatant is AbstractActor targetActor && __instance.SelectedActor != null)
            {

                CombatGameState Combat = __instance.SelectedActor.Combat;
                bool targetIsFriendly = Combat.HostilityMatrix.IsFriendly(combatant.team.GUID, Combat.LocalPlayerTeamGuid);
                if (targetIsFriendly)
                {
                    Mod.Log.Trace?.Write("Friendly target, skipping check");
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SelectionStateFire), "CalcPossibleTargets")]
    static class SelectionStateFire_CalcPossibleTargets
    {
        static void Postfix(SelectionStateFire __instance, List<ICombatant> __result)
        {
            if (__instance == null || __instance.SelectedActor == null) return;
            if (__instance.SelectionType != SelectionType.FireMorale && __instance.SelectionType != SelectionType.FireMulti) return;

            EWState attackerState = new EWState(__instance.SelectedActor);

            // Check for units that should be 
            List<ICombatant> filteredList = new List<ICombatant>();
            foreach (ICombatant combatant in __result)
            {
                if (combatant is AbstractActor targetActor)
                {
                    EWState targetState = new EWState(targetActor);
                    if (__instance.SelectionType == SelectionType.FireMorale)
                    {
                        // Prevents blips from being the targets of called shots
                        VisibilityLevel targetVisibility = __instance.SelectedActor.VisibilityToTargetUnit(targetActor);
                        if (targetVisibility < VisibilityLevel.LOSFull)
                        {
                            Mod.Log.Info?.Write($"Target {CombatantUtils.Label(combatant)} is a blip, cannot be targeted by called shot");
                            continue;
                        }

                        float distance = Vector3.Distance(__instance.SelectedActor.CurrentPosition, targetActor.CurrentPosition);
                        bool hasVisualScan = VisualLockHelper.GetVisualScanRange(__instance.SelectedActor) >= distance;
                        SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(targetActor, __instance.SelectedActor);
                        if (sensorScan < SensorScanType.ArmorAndWeaponType && !hasVisualScan)
                        {
                            Mod.Log.Info?.Write($"Target {CombatantUtils.Label(targetActor)} sensor info {sensorScan} is less than SurfaceScan and range:{distance} outside visualScan range, cannot be targeted by called shot");
                            continue;
                        }

                        filteredList.Add(combatant);
                    }
                    else if (__instance.SelectionType == SelectionType.FireMulti)
                    {
                        if (targetState.HasStealth() || targetState.HasMimetic())
                        {
                            Mod.Log.Info?.Write($"Target {CombatantUtils.Label(targetActor)} has stealth, cannot be multi-targeted!");
                            continue;
                        }

                        filteredList.Add(combatant);
                    }
                    else
                    {
                        filteredList.Add(combatant);
                    }

                }
            }

            __result.Clear();
            __result.AddRange(filteredList);
        }


    }

}
