using UnityEngine.PostProcessing;

namespace LowVisibility.Patch
{

    [HarmonyPatch(typeof(GrainComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class PatchGrainComp
    {
        public static void Prefix(ref bool __runOriginal, ref bool __result)
        {
            if (!__runOriginal) return;

            if (ModState.IsNightVisionMode)
            {
                __result = true;
                __runOriginal = false;
            }
        }
    }

    [HarmonyPatch(typeof(BloomComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class PatchBloom
    {
        public static void Prefix(ref bool __runOriginal, ref bool __result)
        {
            if (!__runOriginal) return;

            if (ModState.IsNightVisionMode)
            {
                __result = true;
                __runOriginal = false;
            }            
        }
    }

    [HarmonyPatch(typeof(VignetteComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class PatchVignette
    {
        public static void Prefix(ref bool __runOriginal, ref bool __result)
        {
            if (!__runOriginal) return;

            if (ModState.IsNightVisionMode)
            {
                __result = true;
                __runOriginal = false;
            }            
        }
    }

    [HarmonyPatch(typeof(ChromaticAberrationComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class ChromaticAberration
    {
        public static void Prefix(ref bool __runOriginal, ref bool __result)
        {
            if (!__runOriginal) return;

            if (ModState.IsNightVisionMode)
            {
                __result = true;
                __runOriginal = false;
            }
        }
    }
}
