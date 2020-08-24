using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using Harmony;
using LowVisibility.Object;

namespace LowVisibility.Patch
{
    public static class VisibilityCachePatches
    {
        public delegate AbstractActor OwningActor(VisibilityCache cache);

        public static OwningActor GetOwningActor =
            (OwningActor) Delegate.CreateDelegate(typeof(OwningActor), typeof(VisibilityCache).GetProperty("OwningActor", AccessTools.all).GetMethod);

        [HarmonyPatch(typeof(VisibilityCache), nameof(VisibilityCache.RebuildCache))]
        public static class VisibilityCache_RebuildCache
        {
            // Lowest priority
            [HarmonyPriority(0)]
            public static bool Prefix(VisibilityCache __instance)
            {
                EWState.InBatchProcess = true;
                EWState.EWStateCache.Clear();

                if (VisibilityCacheGate.Active)
                {
                    VisibilityCacheGate.AddActorToRefresh(GetOwningActor(__instance));
                    return false;
                }

                return true;
            }

            // Highest priority
            [HarmonyPriority(900)]
            public static void Postfix()
            {
                EWState.InBatchProcess = false;
            }
        }

        [HarmonyPatch(typeof(VisibilityCache), nameof(VisibilityCache.UpdateCacheReciprocal))]
        public static class VisibilityCache_UpdateCacheReciprocal
        {
            // Lowest priority
            [HarmonyPriority(0)]
            public static bool Prefix(VisibilityCache __instance)
            {
                EWState.InBatchProcess = true;
                EWState.EWStateCache.Clear();

                if (VisibilityCacheGate.Active)
                {
                    VisibilityCacheGate.AddActorToRefresh(GetOwningActor(__instance));
                    return false;
                }

                return true;
            }

            [HarmonyPriority(900)]
            public static void Postfix()
            {
                EWState.InBatchProcess = false;
            }
        }
    }
}
