using LowVisibility.Helper;
using LowVisibility.Object;
using System.Collections.Generic;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{

    [HarmonyPatch(typeof(MechComponent), "CancelCreatedEffects")]
    public static class MechComponent_CancelCreatedEffects
    {

        public static void Prefix(ref bool __runOriginal, MechComponent __instance, ref bool __state)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Write("MC:CCE:pre entered.");

            // State indicates whether a stealth effect was found
            __state = false;

            Mod.Log.Debug?.Write($" Cancelling effects from component: ({__instance.Name}) on actor: ({CombatantUtils.Label(__instance.parent)})");
            for (int i = 0; i < __instance.createdEffectIDs.Count; i++)
            {
                List<Effect> allEffectsWithID = __instance.parent.Combat.EffectManager.GetAllEffectsWithID(__instance.createdEffectIDs[i]);
                foreach (Effect effect in allEffectsWithID)
                {
                    if (effect.EffectData.effectType == EffectType.StatisticEffect && ModStats.IsStealthStat(effect.EffectData.statisticData.statName))
                    {
                        Mod.Log.Debug?.Write($" -- Found stealth effect to cancel: ({effect.EffectData.Description.Id})");
                        __state = true;
                    }
                }
            }
        }

        public static void Postfix(MechComponent __instance, bool __state)
        {
            Mod.Log.Trace?.Write("MC:CCE:post entered.");

            if (__state)
            {
                Mod.Log.Debug?.Write($" Stealth effect was cancelled, parent visibility needs refreshed.");

                EWState parentState = new EWState(__instance.parent);
                PilotableActorRepresentation par = __instance.parent.GameRep as PilotableActorRepresentation;
                if (parentState.HasStealth())
                {
                    VfxHelper.EnableStealthVfx(__instance.parent);
                }
                else
                {
                    VfxHelper.DisableSensorStealthEffect(__instance.parent);
                }

                if (parentState.HasMimetic())
                {
                    VfxHelper.EnableMimeticEffect(__instance.parent);
                }
                else
                {
                    VfxHelper.DisableMimeticEffect(__instance.parent);
                }

                // Force a refresh in case the signature increased due to stealth loss
                // TODO: Make this player hostile only
                List<ICombatant> allLivingCombatants = __instance.parent.Combat.GetAllLivingCombatants();
                __instance.parent.VisibilityCache.UpdateCacheReciprocal(allLivingCombatants);
            }
        }
    }

    //[HarmonyPatch(typeof(MechComponent), "DamageComponent")]
    //public static class MechComponent_DamageComponent {

    //    public static void Postfix(MechComponent __instance, WeaponHitInfo hitInfo, ComponentDamageLevel damageLevel, bool applyEffects) {
    //        Mod.Log.Trace?.Write("MC:DC:post entered.");

    //        Mod.Log.Debug?.Write($" Damaging component: ({__instance.Name}) on actor: ({CombatantUtils.Label(__instance.parent)} from hitInfo: {hitInfo.targetId}" +
    //            $" applying damageLevel: {damageLevel} with applyEffects: {applyEffects}");
    //    }
    //}


}
