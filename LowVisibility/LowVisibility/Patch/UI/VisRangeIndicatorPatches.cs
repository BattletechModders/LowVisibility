using BattleTech.UI;
using UnityEngine;

namespace LowVisibility.Patch.UI
{
    static class VisRangeState
    {
        public static GameObject VisRangeProjector;
    }

    // VisRangeProjector 
    [HarmonyPatch(typeof(VisRangeIndicator), "SetDecalVisibility")]
    public static class VisRangeIndicator_SetDecalVisibility
    {
        public static void Postfix(VisRangeIndicator __instance, bool visible)
        {
            Mod.Log.Trace?.Write("VRI:SDV - invoked!");
            VisRangeState.VisRangeProjector.SetActive(visible);
        }
    }

    [HarmonyPatch(typeof(VisRangeIndicator), "Init")]
    public static class VisRangeIndicator_Init
    {
        public static void Postfix(VisRangeIndicator __instance)
        {
            Mod.Log.Trace?.Write("VRI:I - invoked!");
            Transform projectorTransform = __instance.gameObject.transform.Find("VisRangeProjector");
            if (projectorTransform != null) VisRangeState.VisRangeProjector = projectorTransform.gameObject;
            else Mod.Log.Warn?.Write("FAILED TO FIND VisRangeProjector transform!");
        }
    }

    [HarmonyPatch(typeof(VisRangeIndicator), "OnDestroy")]
    public static class VisRangeIndicator_OnDestroy
    {
        public static void Postfix(VisRangeIndicator __instance)
        {
            Mod.Log.Trace?.Write("VRI:OD - invoked!");
            VisRangeState.VisRangeProjector = null;
        }
    }
}
