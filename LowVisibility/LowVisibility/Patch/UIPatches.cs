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
    public static class CombatHUDStatusPanel_RefreshDisplayedCombatant {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDStatusPanel), "RefreshDisplayedCombatant", new Type[] { });
        }

        public static void Postfix(CombatHUDStatusPanel __instance) {
            LowVisibility.Logger.LogIfDebug("CombatHUDStatusPanel:RefreshDisplayedCombatant:post - entered.");
            if (__instance != null && __instance.DisplayedCombatant != null) {
                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                IDState idState = CalculateTargetIDLevel(actor);
                if (idState == IDState.ProbeID) {
                    // Do nothing
                } else if (idState == IDState.SensorID) {
                    __instance.ClearAllStatuses();
                } else {
                    __instance.ClearAllStatuses();
                    Traverse hideEvasionIndicatorMethod = Traverse.Create(__instance).Method("HideEvasiveIndicator", new object[] { });
                    hideEvasionIndicatorMethod.GetValue();
                }
            }
        }
    }

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
                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {
                    RoundDetectRange detectRange = State.GetOrCreateRoundDetectResults(actor);

                    Type[] methodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                    if (detectRange == RoundDetectRange.VisualOnly) {
                        Traverse method = Traverse.Create(__instance).Method("ShowDebuff", methodParams);
                        method.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("VISUALS ONLY", new object[0]),
                            new Text($"Unit can only visually detect targets within {State.GetMapVisionRange()}m", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    } else if (detectRange >= RoundDetectRange.SensorsShort) {
                        Traverse method = Traverse.Create(__instance).Method("ShowBuff", methodParams);
                        ActorEWConfig ewConfig = State.GetOrCreateActorEWConfig(actor);
                        float sensorsDistance = CalculateSensorRange(actor);
                        // TODO: Change text/color for probe vs. sensors
                        method.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("SENSORS ACTIVE", new object[0]),
                            new Text($"Unit's sensors will detect targets out {sensorsDistance}m.", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    }
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
