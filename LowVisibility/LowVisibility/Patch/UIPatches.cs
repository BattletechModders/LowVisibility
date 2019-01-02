using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using Localize;
using SVGImporter;
using System;
using System.Collections.Generic;
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

        public static void Postfix(CombatHUDStatusPanel __instance, List<CombatHUDStatusIndicator> ___Buffs, List<CombatHUDStatusIndicator> ___Debuffs) {
            //LowVisibility.Logger.LogIfDebug("CombatHUDStatusPanel:RefreshDisplayedCombatant:post - entered.");
            if (__instance != null && __instance.DisplayedCombatant != null) {
                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (!isPlayer) {
                    IDState idState = CalculateTargetIDLevel(actor);
                    if (idState == IDState.ProbeID) {
                        // Do nothing
                    } else if (idState == IDState.SensorID) {
                        ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                        ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                    } else {
                        ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                        ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                        Traverse hideEvasionIndicatorMethod = Traverse.Create(__instance).Method("HideEvasiveIndicator", new object[] { });
                        hideEvasionIndicatorMethod.GetValue();
                    }
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
                Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffIconMethod = Traverse.Create(__instance).Method("ShowDebuff", iconMethodParams);
                Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                Type[] stringMethodParams = new Type[] { typeof(string), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffStringMethod = Traverse.Create(__instance).Method("ShowDebuff", stringMethodParams);
                Traverse showBuffStringMethod = Traverse.Create(__instance).Method("ShowBuff", stringMethodParams);

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                ActorEWConfig ewConfig = State.GetOrCreateActorEWConfig(actor);
                RoundDetectRange detectRange = State.GetOrCreateRoundDetectResults(actor);

                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {                                        
                    if (detectRange == RoundDetectRange.VisualOnly) {                        
                        showDebuffIconMethod.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("VISUALS ONLY", new object[0]),
                            new Text($"Unit can only visually detect targets within {State.GetMapVisionRange()}m", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    } else if (detectRange >= RoundDetectRange.SensorsShort) {                                                
                        float sensorsDistance = CalculateSensorRange(actor);
                        // TODO: Change text/color for probe vs. sensors
                        showBuffIconMethod.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("SENSORS ACTIVE", new object[0]),
                            new Text($"Unit's sensors will detect targets out {sensorsDistance}m.", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    }

                    // TODO: Better icon
                    if (ewConfig.ecmTier > -1) {
                        showBuffStringMethod.GetValue(new object[] {
                            "uixSvgIcon_status_sensorsImpaired",
                            new Text("ECM JAMMING", new object[0]),
                            new Text($"Unit has an ECM jammer and will hide allies with {ewConfig.ecmRange * 30}m.", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    }

                    // TODO: Better icon
                    if (ewConfig.probeTier > -1) {
                        showDebuffIconMethod.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("ACTIVE PROBE", new object[0]),
                            new Text($"Unit has an active probe which adds {ewConfig.probeRange * 30}m to detection range and allows it to detect component details.", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    }
                }
                
                if (State.IsJammed(actor)) {
                    showDebuffStringMethod.GetValue(new object[] {
                        "uixSvgIcon_status_sensorsImpaired",
                        new Text("ECM JAMMING", new object[0]),
                        new Text($"Unit is within an ECM jamming bubble that will make it difficult to detect other units.", new object[0]),
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
