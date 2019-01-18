using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;
using static LowVisibility.Helper.VisibilityHelper;

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
                AbstractActor target = __instance.DisplayedCombatant as AbstractActor;
                // We can receive a building here, so 
                if (target != null) {
                    bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
                    if (!isPlayer) {                        
                        LockState lockState = State.GetLockStateForLastActivatedAgainstTarget(target);

                        if (lockState.sensorLockLevel < DetectionLevel.Vector) {
                            // Hide the evasive indicator, hide the buffs and debuffs
                            Traverse hideEvasionIndicatorMethod = Traverse.Create(__instance).Method("HideEvasiveIndicator", new object[] { });
                            hideEvasionIndicatorMethod.GetValue();

                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                        } else if (lockState.sensorLockLevel < DetectionLevel.StructureAnalysis) {
                            // Hide the buffs and debuffs
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                        } else if (lockState.sensorLockLevel >= DetectionLevel.StructureAnalysis) {
                            // Do nothing; normal state
                        }
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
            //LowVisibility.Logger.LogIfDebug("___ CombatHUDStatusPanel:ShowActorStatuses:post - entered.");

            if (__instance.DisplayedCombatant != null) {
                Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffIconMethod = Traverse.Create(__instance).Method("ShowDebuff", iconMethodParams);
                Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                Type[] stringMethodParams = new Type[] { typeof(string), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffStringMethod = Traverse.Create(__instance).Method("ShowDebuff", stringMethodParams);
                Traverse showBuffStringMethod = Traverse.Create(__instance).Method("ShowBuff", stringMethodParams);

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                StaticEWState staticState = State.GetStaticState(actor);
                DynamicEWState dynamicState = State.GetDynamicState(actor);

                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {
                    showBuffIconMethod.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("VISION AND SENSORS", new object[0]),
                            new Text(BuildToolTip(actor)),
                            __instance.effectIconScale,
                            false
                        });

                    if (actor.Combat.TurnDirector.CurrentRound == 1) {
                        showDebuffStringMethod.GetValue(new object[] {
                            "uixSvgIcon_status_sensorsImpaired",
                            new Text("SENSORS OFFLINE", new object[0]),
                            new Text($"Sensors offline during the first round of the battle.", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    }
                }

                if (State.ECMProtection(actor) != 0) {
                    showDebuffStringMethod.GetValue(new object[] {
                        "uixSvgIcon_status_sensorsImpaired",
                        new Text("ECM PROTECTION", new object[0]),
                        new Text($"Unit is protected by friendly ECM and will be harder to detect by enemy units."),
                        __instance.effectIconScale,
                        false
                    });
                }

                if (State.ECMJamming(actor) != 0) {
                    showDebuffStringMethod.GetValue(new object[] {
                        "uixSvgIcon_status_sensorsImpaired",
                        new Text("ECM JAMMING", new object[0]),
                        new Text($"Unit is jammed by enemy ECM which makes enemy units harder to detect."),
                        __instance.effectIconScale,
                        false
                    });
                }
            }
        }

        private static string BuildToolTip(AbstractActor actor) {
            StaticEWState staticState = State.GetStaticState(actor);
            DynamicEWState dynamicState = State.GetDynamicState(actor);

            List<string> details = new List<string>();
            float visualLockRange = ActorHelper.GetVisualLockRange(actor);
            float sensorsRange = ActorHelper.GetSensorsRange(actor);
            details.Add($"RANGE => Visual:{visualLockRange:0}m Sensors:{sensorsRange:0}m\n");

            // TODO: Let players know what effect is impacting their vision

            details.Add($"  Range: Roll: ");
            float rangeMulti = 1.0f + ((dynamicState.rangeCheck + staticState.tacticsBonus) / 10.0f);
            if (dynamicState.rangeCheck >= 0) {
                details.Add($"<color=#00FF00>{dynamicState.rangeCheck:+0}</color> + " +
                    $"Tactics: <color=#00FF00>{staticState.tacticsBonus:0}</color> = " +
                    $"Multi: <color=#00FF00>x{rangeMulti:0.00}</color>");
            } else {
                details.Add($"<color=#FF0000>{dynamicState.rangeCheck:0}</color> + " +
                    $"Tactics: <color=#00FF00>{staticState.tacticsBonus:0}</color> = " +
                    $"Multi: <color=#FF0000>x{rangeMulti:0.00}</color>");                
            }
            details.Add("\n");

            details.Add($" Info: => ");
            int checkResult = dynamicState.detailCheck;
            if (dynamicState.detailCheck >= 0) {
                details.Add($"Roll: <color=#00FF00>{dynamicState.detailCheck:0}</color>");
            } else {
                details.Add($"Roll: <color=#FF0000>{dynamicState.detailCheck:0}</color>");
            }
            checkResult += staticState.tacticsBonus;
            details.Add($" + Tactics: <color=#00FF00>{staticState.tacticsBonus:0}</color>");                        

            if (staticState.probeMod > 0) {
                checkResult += staticState.probeMod;
                details.Add($" + Probe: <color=#00FF00>{staticState.probeMod:0}</color>");
            }

            if (State.ECMJamming(actor) != 0) {
                checkResult -= State.ECMJamming(actor);
                details.Add($" + Jammed: <color=#FF0000>{State.ECMJamming(actor):-0}</color>");
            }

            details.Add(" = Result: ");
            if (checkResult >= 0) {
                details.Add($"<color=#00FF00>{checkResult:0}</color>");
            } else {
                details.Add($"<color=#FF0000>{checkResult:0}</color>");
            }
            details.Add("\n");

            // Sensor check:(+/-0) SensorScanLevel:
            if (staticState.ecmMod != 0) {
                details.Add($"ECM => Range:{staticState.ecmRange}m Enemy Modifier:<color=#FF0000>{staticState.ecmMod:-0}</color>");
            }
            if (staticState.stealthMod != 0) {
                details.Add($"STEALTH => Enemy Modifier:<color=#FF0000>{staticState.stealthMod:-0}</color>");
            }

            string tooltipText = String.Join("", details.ToArray());
            return tooltipText;
        }
    }

}
