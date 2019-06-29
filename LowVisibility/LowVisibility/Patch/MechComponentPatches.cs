using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System.Collections.Generic;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(MechComponent), "CancelCreatedEffects")]
    public static class MechComponent_CancelCreatedEffects {

        public static void Prefix(MechComponent __instance, ref bool __state) {
            Mod.Log.Trace("MC:CCE:pre entered.");

            // State indicates whether a stealth effect was found
            __state = false;

            Mod.Log.Debug($" Cancelling effects from component: ({__instance.Name}) on actor: ({CombatantUtils.Label(__instance.parent)})");
            for (int i = 0; i < __instance.createdEffectIDs.Count; i++) {
                List<Effect> allEffectsWithID = __instance.parent.Combat.EffectManager.GetAllEffectsWithID(__instance.createdEffectIDs[i]);
                foreach (Effect effect in allEffectsWithID) {
                    if (effect.EffectData.effectType == EffectType.StatisticEffect &&
                        (effect.EffectData.statisticData.statName == ModStats.SensorStealth || effect.EffectData.statisticData.statName == ModStats.VisionStealth)) {
                        Mod.Log.Debug($" -- Found stealth effect to cancel: ({effect.EffectData.Description.Id})");
                        __state = true;
                    }
                }
            }
        }

        public static void Postfix(MechComponent __instance, bool __state) {
            Mod.Log.Trace("MC:CCE:post entered.");

            if (__state) {
                Mod.Log.Debug($" Stealth effect was cancelled, parent visibility needs refreshed.");

                EWState parentState = new EWState(__instance.parent);
                PilotableActorRepresentation par = __instance.parent.GameRep as PilotableActorRepresentation;
                if (parentState.SensorStealth != 0) {
                    Mod.Log.Debug($" Actor: ({CombatantUtils.Label(__instance.parent)}) has sensor stealth, enabling sparkles");
                    VfxHelper.EnableSensorStealthEffect(__instance.parent);
                } else if (parentState.VisionStealth != 0) {
                    Mod.Log.Debug($" Actor: ({CombatantUtils.Label(__instance.parent)}) has vision stealth, enabling blip.");
                    VfxHelper.EnableVisionStealthEffect(__instance.parent);
                } else {
                    Mod.Log.Debug($" Actor: ({CombatantUtils.Label(__instance.parent)}) lost stealth, disabling collider.");
                    VfxHelper.DisableSensorStealthEffect(__instance.parent);
                    VfxHelper.DisableVisionStealthEffect(__instance.parent);
                }

                // Force a refresh in case the signature increased due to stealth loss
                // TODO: Make this player hostile only
                List<ICombatant> allLivingCombatants = __instance.parent.Combat.GetAllLivingCombatants();
                __instance.parent.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
            }
        }
    }

    [HarmonyPatch(typeof(MechComponent), "DamageComponent")]
    public static class MechComponent_DamageComponent {

        public static void Postfix(MechComponent __instance, WeaponHitInfo hitInfo, ComponentDamageLevel damageLevel, bool applyEffects) {
            Mod.Log.Trace("MC:DC:post entered.");

            Mod.Log.Debug($" Damaging component: ({__instance.Name}) on actor: ({CombatantUtils.Label(__instance.parent)} from hitInfo: {hitInfo.targetId}" +
                $" applying damageLevel: {damageLevel} with applyEffects: {applyEffects}");
        }
    }


}
