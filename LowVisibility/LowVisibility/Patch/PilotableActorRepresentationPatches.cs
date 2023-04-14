using BattleTech.UI;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{

    [HarmonyPatch(typeof(PilotableActorRepresentation), "OnPlayerVisibilityChanged")]
    public static class PilotableActorRepresentation_OnPlayerVisibilityChanged
    {
        public static void Postfix(PilotableActorRepresentation __instance, VisibilityLevel newLevel)
        {
            Mod.Log.Trace?.Write("PAR:OPVC entered.");

            AbstractActor parentActor = __instance.parentActor;

            if (parentActor == null)
            {
                Mod.Log.Trace?.Write($"ParentActor is null, skipping!");
                return;
            }

            EWState parentState = new EWState(parentActor);

            if (newLevel == VisibilityLevel.LOSFull)
            {
                if (parentState.HasStealth())
                {
                    VfxHelper.EnableStealthVfx(parentActor);
                }
                else
                {
                    VfxHelper.DisableSensorStealthEffect(parentActor);
                }

                if (parentState.HasMimetic())
                {
                    VfxHelper.EnableMimeticEffect(parentActor);
                }
                else
                {
                    VfxHelper.DisableMimeticEffect(parentActor);
                }

                if (__instance.BlipObjectUnknown != null) __instance.BlipObjectUnknown.SetActive(false);
                if (__instance.BlipObjectUnknown != null) __instance.BlipObjectUnknown.SetActive(false);
            }
            else if (newLevel >= VisibilityLevel.Blip0Minimum)
            {
                Mod.Log.Debug?.Write($"Actor: {CombatantUtils.Label(parentActor)} has changed player visibility to: {newLevel}");

                if (parentActor.team.IsFriendly(parentActor.Combat.LocalPlayerTeam))
                {
                    Mod.Log.Debug?.Write($" Target actor is friendly, forcing blip off");
                    if (__instance.BlipObjectUnknown != null) __instance.BlipObjectUnknown.SetActive(false);
                }
                else
                {
                    // Because Blip1 corresponds to ArmorAndWeapon, go ahead and show the model as the chassis is 'known'
                    if (newLevel >= VisibilityLevel.Blip1Type)
                    {
                        Mod.Log.Debug?.Write($" Actor is a foe,  disabling the identified blip and showing the object");
                        __instance.VisibleObject.SetActive(true);
                        if (__instance.BlipObjectIdentified != null) __instance.BlipObjectIdentified.SetActive(false);

                        if (__instance.BlipObjectUnknown != null) __instance.BlipObjectUnknown.transform.localScale = new Vector3(1f, 0.8f, 1f);
                        if (__instance.BlipObjectUnknown != null) __instance.BlipObjectUnknown.SetActive(true);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PilotableActorRepresentation), "updateBlips")]
    public static class PilotableActorRepresentation_updateBlips
    {
        //public static Vector3 getNewBlipPosition()
        public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, ref Vector3 ___blipPendingPosition)
        {
            if (!__runOriginal) return;

            //Mod.Log.Debug?.Write($" UPDATE BLIPS INVOKED");
            if (__instance.BlipObjectUnknown.activeSelf && __instance.VisibleObject.activeSelf
                && !__instance.BlipObjectIdentified.activeSelf)
            {
                float height = Math.Min(__instance.VisibleObject.transform.position.y + 20f, ___blipPendingPosition.y + 20f);
                ___blipPendingPosition.y = height;
            }
        }
    }

    // Required to move the floating actor info above a blip closer to the source. It natively assumes it anchors off the head, 
    //   while the blip centers on the ground. This leaves significant empty space.
    [HarmonyPatch(typeof(CombatHUDNumFlagHex), "LateUpdate")]
    public static class CombatHUDNumFlagHex_LateUpdate
    {
        public static void Postfix(CombatHUDNumFlagHex __instance)
        {
            if (__instance.DisplayedActor != null && __instance.DisplayedActor.IsActorOnScreen())
            {
                PilotableActorRepresentation par = (PilotableActorRepresentation)__instance.DisplayedActor.GameRep;
                if (par != null && par.VisibleObject.activeSelf && par.BlipObjectUnknown.activeSelf)
                {
                    __instance.anchorPosition = CombatHUDInWorldScalingActorInfo.AnchorPosition.Feet;
                }
                else
                {
                    __instance.anchorPosition = CombatHUDInWorldScalingActorInfo.AnchorPosition.Head;
                }
            }

        }
    }

}
