﻿using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(PilotableActorRepresentation), "OnPlayerVisibilityChanged")]
    public static class PilotableActorRepresentation_OnPlayerVisibilityChanged {
        public static void Postfix(PilotableActorRepresentation __instance, VisibilityLevel newLevel, CapsuleCollider ___mainCollider) {
            Mod.Log.Trace("PAR:OPVC entered.");

            Traverse parentT = Traverse.Create(__instance).Property("parentActor");
            AbstractActor parentActor = parentT.GetValue<AbstractActor>();

            if (parentActor == null) {
                Mod.Log.Trace($"ParentActor is null, skipping!");
                return;
            }

            EWState parentState = new EWState(parentActor);

            if (newLevel == VisibilityLevel.LOSFull) {
                if (parentState.HasStealth()) {
                    VfxHelper.EnableStealthVfx(parentActor);
                } else {
                    VfxHelper.DisableSensorStealthEffect(parentActor);
                }

                if (parentState.HasMimetic()) {
                    VfxHelper.EnableMimeticEffect(parentActor);
                } else {
                    VfxHelper.DisableMimeticEffect(parentActor);
                }
            } else if (newLevel >= VisibilityLevel.Blip0Minimum) {
                Mod.Log.Debug($"Actor: {CombatantUtils.Label(parentActor)} has changed visibility to: {newLevel}");

                if (parentActor.team.IsFriendly(parentActor.Combat.LocalPlayerTeam)) {
                    Mod.Log.Debug($" Actor is friendly, forcing blip off");
                    // Force the blip to be hidden
                    // TODO: Does this work?
                    __instance.BlipObjectUnknown.SetActive(false);
                } else {
                    Mod.Log.Debug($" Actor is a foe,  disabling the identified blip and showing the object");
                    __instance.VisibleObject.SetActive(true);
                    __instance.BlipObjectIdentified.SetActive(false);

                    __instance.BlipObjectUnknown.transform.localScale = new Vector3(1f, 0.8f, 1f);
                    __instance.BlipObjectUnknown.SetActive(true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PilotableActorRepresentation), "updateBlips")]
    public static class PilotableActorRepresentation_updateBlips {
        public static void Prefix(PilotableActorRepresentation __instance, ref Vector3 ___blipPendingPosition) {
            //Mod.Log.Debug($" UPDATE BLIPS INVOKED");
            if (__instance.BlipObjectUnknown.activeSelf && __instance.VisibleObject.activeSelf
                && !__instance.BlipObjectIdentified.activeSelf) {
                float height = Math.Min(__instance.VisibleObject.transform.position.y + 20f, ___blipPendingPosition.y + 20f);
                ___blipPendingPosition.y = height;
            }
        }
    }

    // Required to move the floating actor info above a blip closer to the source. It natively assumes it anchors off the head, 
    //   while the blip centers on the ground. This leaves significant empty space.
    [HarmonyPatch(typeof(CombatHUDNumFlagHex), "LateUpdate")]
    public static class CombatHUDNumFlagHex_LateUpdate {
        public static void Postfix(CombatHUDNumFlagHex __instance) {
            if (__instance.DisplayedActor != null && __instance.DisplayedActor.IsActorOnScreen()) {
                PilotableActorRepresentation par = (PilotableActorRepresentation)__instance.DisplayedActor.GameRep;
                if (par != null && par.VisibleObject.activeSelf && par.BlipObjectUnknown.activeSelf) {
                    __instance.anchorPosition = CombatHUDInWorldScalingActorInfo.AnchorPosition.Feet;
                } else {
                    __instance.anchorPosition = CombatHUDInWorldScalingActorInfo.AnchorPosition.Head;
                }
            }
            
        }
    }

}
