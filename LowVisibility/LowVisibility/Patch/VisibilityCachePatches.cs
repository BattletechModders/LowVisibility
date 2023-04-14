using LowVisibility.Object;

namespace LowVisibility.Patch
{
    [HarmonyPatch(typeof(VisibilityCache), nameof(VisibilityCache.RebuildCache))]
    public static class VisibilityCache_RebuildCache
    {
        // Lowest priority
        [HarmonyPriority(0)]
        public static void Prefix(ref bool __runOriginal)
        {
            if (!__runOriginal) return;

            EWState.InBatchProcess = true;
            EWState.EWStateCache.Clear();
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
        public static void Prefix(ref bool __runOriginal)
        {
            if (!__runOriginal) return;

            EWState.InBatchProcess = true;
            EWState.EWStateCache.Clear();
        }

        [HarmonyPriority(900)]
        public static void Postfix()
        {
            EWState.InBatchProcess = false;
        }
    }
}
