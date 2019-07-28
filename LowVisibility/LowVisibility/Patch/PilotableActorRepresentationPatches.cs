using BattleTech;
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


    [HarmonyPatch(typeof(PilotableActorRepresentation), "SetBlipPositionRotation")]
    public static class PilotableActorRepresentation_SetBlipPositionRotation {
        public static void Postfix(PilotableActorRepresentation __instance, Vector3 position, Quaternion rotation, Vector3 ___blipPendingPosition) {
            Mod.Log.Debug($" ON POSITION UPDATE CALLED FOR POSITION: {position} and rotation: {rotation}");
            if (__instance.BlipObjectUnknown.activeSelf && __instance.VisibleObject.activeSelf 
                && !__instance.BlipObjectIdentified.activeSelf) {
                Mod.Log.Debug($" BLIP CONDITION IDENTIFIED");
                ___blipPendingPosition.y += 90f;
            }

        }
    }

    [HarmonyPatch(typeof(PilotableActorRepresentation), "updateBlips")]
    public static class PilotableActorRepresentation_updateBlips {
        public static void Prefix(PilotableActorRepresentation __instance, ref Vector3 ___blipPendingPosition) {
            //Mod.Log.Debug($" UPDATE BLIPS INVOKED");
            if (__instance.BlipObjectUnknown.activeSelf && __instance.VisibleObject.activeSelf
                && !__instance.BlipObjectIdentified.activeSelf) {
                Mod.Log.Debug($" updateBlips: BLIP CONDITION IDENTIFIED, setting pos: {___blipPendingPosition} to y+20");
                float height = Math.Min(__instance.VisibleObject.transform.position.y + 20f, ___blipPendingPosition.y + 20f);
                ___blipPendingPosition.y = height;
                //Vector3 currentPos = __instance.BlipObjectUnknown.transform.position;
                //currentPos.y += 100;
                //__instance.BlipObjectUnknown.transform.position = currentPos;
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDNumFlagHex), "LateUpdate")]
    public static class CombatHUDNumFlagHex_LateUpdate {
        public static void Postfix(CombatHUDNumFlagHex __instance) {
            if (__instance.DisplayedActor != null && __instance.DisplayedActor.IsActorOnScreen()) {
                PilotableActorRepresentation par = (PilotableActorRepresentation)__instance.DisplayedActor.GameRep;
                if (par != null && par.VisibleObject.activeSelf && par.BlipObjectUnknown.activeSelf) {
                    Mod.Log.Debug($" lateUpdate: BLIP CONDITION IDENTIFIED, setting pos: {par.transform.position} to y+20");
                    __instance.anchorPosition = CombatHUDInWorldScalingActorInfo.AnchorPosition.Feet;
                    //Vector3 originalPos = __instance.transform.position;
                    //Vector3 newPos = new Vector3(originalPos.x, originalPos.y + 20f, originalPos.z);
                    //__instance.transform.position = newPos;
                } else {
                    __instance.anchorPosition = CombatHUDInWorldScalingActorInfo.AnchorPosition.Head;
                }
            }
            
        }
    }

}
