using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using Localize;
using SVGImporter;
using System;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch()]
    public static class CombatHUDStatusPanel_ShowActorStatuses {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDStatusPanel), "ShowActorStatuses", new Type[] { typeof(AbstractActor) });
        }

        public static void Postfix(CombatHUDStatusPanel __instance) {
            LowVisibility.Logger.LogIfDebug("CombatHUDStatusPanel:ShowActorStatuses:post - entered.");

            if (__instance.DisplayedCombatant != null) {
                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                RoundDetectRange detectRange = State.GetOrCreateRoundDetectResults(actor);

                Type[] methodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };                               
                if (detectRange == RoundDetectRange.VisualOnly) {
                    Traverse method = Traverse.Create(__instance).Method("ShowDebuff", methodParams);
                    method.GetValue(new object[] {
                        LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                        new Text("VISUALS ONLY", new object[0]),
                        new Text($"This unit failed a sensors check. It will only detect visually within {State.GetMapVisionRange()}", new object[0]),
                        __instance.effectIconScale,
                        false
                    });
                } else if (detectRange >= RoundDetectRange.SensorsShort) {
                    Traverse method = Traverse.Create(__instance).Method("ShowBuff", methodParams);
                    ActorEWConfig ewConfig = State.GetOrCreateActorEWConfig(actor);
                    float sensorsDistance = ewConfig.sensorsRange * (int)detectRange;
                    method.GetValue(new object[] {
                        LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                        new Text("SENSORS ACTIVE", new object[0]),
                        new Text($"This unit pass a sensors check. It will detect units out to {sensorsDistance}.", new object[0]),
                        __instance.effectIconScale,
                        false
                    });
                }
                
            }

            /*
                private void ShowSensorLockIndicator() {
			        this.ShowDebuff(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon, new Text("SENSOR LOCKED", new object[0]), new Text("This unit is Sensor Locked. It is visible to both sides.", new object[0]), this.effectIconScale, false);
		        }
             */
        }
    }


}
