using BattleTech;
using BattleTech.Rendering;
using BattleTech.UI;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace LowVisibility.Patch {
    [HarmonyPatch(typeof(VisRangeIndicator), "Init")]
    public static class VisRangeIndicator_Init {

        public static GameObject RangedScaledObjectClone;
        public static GameObject RadarRangeHolderClone;

        public static void Postfix(VisRangeIndicator __instance, CombatGameState Combat, CombatHUD HUD, int ___visRangeInt, int ___sensorRangeInt, GameObject ___radarRangeHolder) {
            Mod.Log.Log($"VisRangeIndicator::Init ");
            RadarRangeHolderClone = UnityEngine.Object.Instantiate(___radarRangeHolder, __instance.gameObject.transform);

            //GameObject radarRangeObject = (GameObject)Traverse.Create(__instance).Property("radarRangeScaledObject").GetValue(); 
            RangedScaledObjectClone = RadarRangeHolderClone.GetComponentInChildren<BTUIDecal>(true).gameObject;

            //LowVisibility.Logger.Log($"VisRangeIndicator::Init - VisRangeDecal is: {__instance.VisRangeDecal}");
            //LowVisibility.Logger.Log($"VisRangeIndicator::Init - visRangeInt:{___visRangeInt} sensorRangeInt:{___sensorRangeInt}");
        }
    }

    [HarmonyPatch(typeof(VisRangeIndicator), "UpdateIndicator")]
    public static class VisRangeIndicator_UpdateIndicator { 
        public static void Postfix(VisRangeIndicator __instance, Vector3 position, VisRangeIndicator.VisRangeIndicatorState ___state) {
            //LowVisibility.Logger.Log($"VisRangeIndicator::UpdateIndicator for position:{position}");
        }
    }

    [HarmonyPatch(typeof(VisRangeIndicator), "SetState")] 
    public static class VisRangeIndicator_SetState {
        public static void Postfix(VisRangeIndicator __instance, VisRangeIndicator.VisRangeIndicatorState newState) {
            //LowVisibility.Logger.Log($"VisRangeIndicator::SetState for newState:{newState}");
            MethodInfo sdv = AccessTools.Method("BattleTech.UI.VisRangeIndicator:SetDecalVisibility", new Type[] { typeof(bool) });
            if (newState == VisRangeIndicator.VisRangeIndicatorState.On) {
                //sdv.Invoke(__instance, new object[] { false });
                //VisRangeIndicator_Init.RangedScaledObjectClone.SetActive(true);
                //VisRangeIndicator_Init.RangedScaledObjectClone.transform.localScale = new Vector3(120f * 2f, 1f, 120f * 2f);                
            } if (newState == VisRangeIndicator.VisRangeIndicatorState.Off) {
                VisRangeIndicator_Init.RangedScaledObjectClone.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(VisRangeIndicator), "Refresh")]
    public static class VisRangeIndicator_Refresh {
        public static void Postfix(VisRangeIndicator __instance, AbstractActor actor) {
            //LowVisibility.Logger.Log($"VisRangeIndicator::Refresh for actor:{CombatantUtils.Label(actor)}");
        }
    }

    [HarmonyPatch(typeof(VisRangeIndicator), "Hide")]
    public static class VisRangeIndicator_Hide {
        public static void Postfix(VisRangeIndicator __instance) {
            //LowVisibility.Logger.Log($"VisRangeIndicator::Hide");
        }
    }

}
