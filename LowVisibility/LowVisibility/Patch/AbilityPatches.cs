using LowVisibility.Helper;
using System.Collections.Generic;

namespace LowVisibility.Patch
{
    public static class Ability_Confirm
    {

        /*
		 * RogueTech decided to add LV_PROBE_PING and LV_PROBE_CARRIER as effects that can be triggered. When this happens, VisiblityCache won't understand a state change
		 *   as the underlying VisibilityLevel stays the same, even if the SensorDetailsLevel changes. As such, force a refresh - which should trigger a recompute 
		 *   of the various LowVis data across every actor on the board.
		 */
        public static void Postfix(Ability __instance, AbstractActor creator)
        {
            List<Effect> allEffectsWithID = __instance.Combat.EffectManager.GetAllEffectsWithID(__instance.Def.Id);
            for (int i = 0; i < allEffectsWithID.Count; i++)
            {
                if (allEffectsWithID[i].creatorID == creator.GUID &&
                    allEffectsWithID[i].EffectData.targetingData.forceVisRebuild &&
                    allEffectsWithID[i].EffectData.statisticData != null &&
                    (allEffectsWithID[i].EffectData.statisticData.statName.Equals(ModStats.ProbeCarrier) || allEffectsWithID[i].EffectData.statisticData.statName.Equals(ModStats.PingedByProbe))
                    )
                {
                    CombatHUDHelper.ForceNameRefresh(__instance.Combat);
                }
            }
        }
    }
}
