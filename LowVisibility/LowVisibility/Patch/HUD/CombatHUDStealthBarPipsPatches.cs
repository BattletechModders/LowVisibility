using BattleTech.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{
    class CombatHUDStealthBarPipsPatches
    {

        [HarmonyPatch(typeof(CombatHUDStealthBarPips), "ShowValue")]
        public static class CombatHUDStealthBarPips_ShowValue
        {
            public static void Postfix(CombatHUDStealthBarPips __instance, float current, float projected)
            {
                Mod.Log.Trace?.Write("CHUDSBP:SV entered");

                Mod.Log.Trace?.Write($"StealthBarPips incoming count is: {current} with projected: {projected}");

                CombatHUD HUD = __instance.HUD;

                AbstractActor selectedActor = HUD.selectedUnit;
                Mod.Log.Trace?.Write($"  selectedActor: ({CombatantUtils.Label(selectedActor)})");

                int floorCurrent = __instance.floorCurrent;
                int floorLocked = __instance.floorLocked;
                int floorProjected = __instance.floorProjected;
                float remainder = __instance.remainder;
                Mod.Log.Trace?.Write($"  floorCurrent: {floorCurrent} floorLocked: {floorLocked} floorProjected: {floorProjected} remainder: {remainder}");

                List<Graphic> pips = __instance.Pips;
                for (int i = 0; i < pips.Count; i++)
                {
                    Mod.Log.Trace?.Write($"    -- pips graphic: {i} isEnabled: {pips[i].IsActive()}");
                }
            }
        }

        [HarmonyPatch(typeof(MoveStatusPreview), "DisplayPreviewStatus")]
        public static class MoveStatusPreview_DisplayPreviewStatus
        {

            public static void Prefix(ref bool __runOriginal, MoveStatusPreview __instance, AbstractActor actor, Vector3 worldPos, MoveType moveType)
            {
                if (!__runOriginal) return;

                Mod.Log.Trace?.Write("MSP:DPS entered.");

                if (actor.CurrentPosition != worldPos)
                {
                    float distance = Vector3.Distance(actor.CurrentPosition, worldPos);
                    int steps = (int)Math.Ceiling(distance / 30f);
                    Mod.Log.Trace?.Write($" position change for: ({CombatantUtils.Label(actor)}), moved {distance}m = {steps} steps");
                    actor.StatCollection.Set(ModStats.MimeticCurrentSteps, steps);
                }
            }
        }
    }

}
