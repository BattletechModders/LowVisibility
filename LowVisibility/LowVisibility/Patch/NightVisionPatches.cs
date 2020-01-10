using Harmony;
using UnityEngine.PostProcessing;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(GrainComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class PatchGrainComp {
        public static bool Prefix(ref bool __result) {
            if (ModState.IsNightVisionMode) {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BloomComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class PatchBloom {
        public static bool Prefix(ref bool __result) {
            if (ModState.IsNightVisionMode) {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(VignetteComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class PatchVignette {
        public static bool Prefix(ref bool __result) {
            if (ModState.IsNightVisionMode) {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ChromaticAberrationComponent), "active", MethodType.Getter)]
    [HarmonyBefore(new string[] { "ca.gnivler.CrystalClear" })]
    public static class ChromaticAberration {
        public static bool Prefix(ref bool __result) {
            if (ModState.IsNightVisionMode) {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
