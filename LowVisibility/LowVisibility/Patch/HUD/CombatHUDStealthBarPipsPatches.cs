﻿using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {
    class CombatHUDStealthBarPipsPatches {

        [HarmonyPatch(typeof(CombatHUDStealthBarPips), "ShowValue")]
        public static class CombatHUDStealthBarPips_ShowValue {
            public static void Postfix(CombatHUDStealthBarPips __instance, float current, float projected) {
                Mod.Log.Trace?.Write("CHUDSBP:SV entered");

                Mod.Log.Trace?.Write($"StealthBarPips incoming count is: {current} with projected: {projected}");

                Traverse HUDT = Traverse.Create(__instance).Property("HUD");
                CombatHUD HUD = HUDT.GetValue<CombatHUD>();

                Traverse actorT = Traverse.Create(HUD).Field("selectedUnit");
                AbstractActor selectedActor = actorT.GetValue<AbstractActor>();
                Mod.Log.Trace?.Write($"  selectedActor: ({CombatantUtils.Label(selectedActor)})");

                Traverse floorCurrentT = Traverse.Create(__instance).Field("floorCurrent");
                int floorCurrent = floorCurrentT.GetValue<int>();

                Traverse floorLockedT = Traverse.Create(__instance).Field("floorLocked");
                int floorLocked = floorLockedT.GetValue<int>();

                Traverse floorProjectedT = Traverse.Create(__instance).Field("floorProjected");
                int floorProjected = floorProjectedT.GetValue<int>();

                Traverse remainderT = Traverse.Create(__instance).Field("remainder");
                float remainder = remainderT.GetValue<float>();
                Mod.Log.Trace?.Write($"  floorCurrent: {floorCurrent} floorLocked: {floorLocked} floorProjected: {floorProjected} remainder: {remainder}");

                Traverse pipsT = Traverse.Create(__instance).Property("Pips");
                List<Graphic> pips = pipsT.GetValue<List<Graphic>>();

                for (int i = 0; i < pips.Count; i++) {
                    Mod.Log.Trace?.Write($"    -- pips graphic: {i} isEnabled: {pips[i].IsActive()}");
                }
            }
        }

        [HarmonyPatch(typeof(MoveStatusPreview), "DisplayPreviewStatus")]
        public static class MoveStatusPreview_DisplayPreviewStatus {

            public static void Prefix(MoveStatusPreview __instance, AbstractActor actor, Vector3 worldPos, MoveType moveType) {
                Mod.Log.Trace?.Write("MSP:DPS entered.");

                if (actor.CurrentPosition != worldPos) {
                    float distance = Vector3.Distance(actor.CurrentPosition, worldPos);
                    int steps = (int)Math.Ceiling(distance / 30f);
                    Mod.Log.Trace?.Write($" position change for: ({CombatantUtils.Label(actor)}), moved {distance}m = {steps} steps");
                    actor.StatCollection.Set(ModStats.MimeticCurrentSteps, steps);
                }
            }
        }
    }

}
