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
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch()]
    public static class CombatHUDStatusPanel_RefreshDisplayedCombatant {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDStatusPanel), "RefreshDisplayedCombatant", new Type[] { });
        }

        public static void Postfix(CombatHUDStatusPanel __instance, List<CombatHUDStatusIndicator> ___Buffs, List<CombatHUDStatusIndicator> ___Debuffs) {
            //LowVisibility.Logger.Debug("CombatHUDStatusPanel:RefreshDisplayedCombatant:post - entered.");
            if (__instance != null && __instance.DisplayedCombatant != null) {
                AbstractActor target = __instance.DisplayedCombatant as AbstractActor;
                // We can receive a building here, so 
                if (target != null) {
                    if (target.Combat.HostilityMatrix.IsLocalPlayerEnemy(target.team)) {                        
                        Locks lockState = State.LastActivatedLocksForTarget(target);

                        if (lockState.sensorLock < SensorScanType.Vector) {
                            //// Hide the evasive indicator, hide the buffs and debuffs
                            //Traverse hideEvasionIndicatorMethod = Traverse.Create(__instance).Method("HideEvasiveIndicator", new object[] { });
                            //hideEvasionIndicatorMethod.GetValue();
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                        } else if (lockState.sensorLock < SensorScanType.StructureAnalysis) {
                            // Hide the buffs and debuffs
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                        } else if (lockState.sensorLock >= SensorScanType.StructureAnalysis) {
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
            //LowVisibility.Logger.Debug("___ CombatHUDStatusPanel:ShowActorStatuses:post - entered.");

            if (__instance.DisplayedCombatant != null) {
                Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffIconMethod = Traverse.Create(__instance).Method("ShowDebuff", iconMethodParams);
                Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                Type[] stringMethodParams = new Type[] { typeof(string), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffStringMethod = Traverse.Create(__instance).Method("ShowDebuff", stringMethodParams);
                Traverse showBuffStringMethod = Traverse.Create(__instance).Method("ShowBuff", stringMethodParams);

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                EWState staticState = new EWState(actor);

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
            EWState ewState = new EWState(actor);
            Mod.Log.Debug($"EW State for actor:{CombatantUtils.Label(actor)} = {ewState}");

            List<string> details = new List<string>();
            float visualLockRange = VisualLockHelper.GetVisualLockRange(actor);
            float visualScanRange = VisualLockHelper.GetVisualScanRange(actor);
            float sensorsRange = SensorLockHelper.GetSensorsRange(actor);            
            details.Add($"Visual Lock:{visualLockRange:0}m Scan:{visualScanRange}m [{State.MapConfig.UILabel()}]\n");

            List<string> sensorDetails = new List<string>();
            sensorDetails.Add(" Sensor Check:");
            
            if (ewState.sensorsCheck >= 0) {
                sensorDetails.Add($"<color=#00FF00>{ewState.sensorsCheck:+0}</color>");                                        
            } else {
                sensorDetails.Add($"<color=#FF0000>{ewState.sensorsCheck:0}</color>");
            }

            sensorDetails.Add($" (Tactics: <color=#00FF00>{ewState.tacticsBonus:0}</color>)");

            if (ewState.probeMod > 0) {
                sensorDetails.Add($" (Probe:<color=#00FF00>{ewState.probeMod:0}</color>)");
            }

            // TODO: Should include equipment bonuses, etc
            float rangeMulti = (1f + ewState.SensorCheckRangeMultiplier());
            if (rangeMulti >= 1.0) {
                sensorDetails.Add($" = <color=#00FF00>x{rangeMulti:0.00}</color>");
            } else {
                sensorDetails.Add($" = <color=#FF0000>x{rangeMulti:0.00}</color>");
            }

            sensorDetails.Add("\n");

            // Sensor Info below
            int checkResult = ewState.sensorsCheck;

            sensorDetails.Add($" Info Roll:");            
            if (ewState.sensorsCheck >= 0) {
                sensorDetails.Add($"<color=#00FF00>{ewState.sensorsCheck:0}</color>");
            } else {
                sensorDetails.Add($"<color=#FF0000>{ewState.sensorsCheck:0}</color>");
            }
            checkResult += ewState.tacticsBonus;
            sensorDetails.Add($" + Tactics: <color=#00FF00>{ewState.tacticsBonus:0}</color>");                        

            if (ewState.probeMod > 0) {
                checkResult += ewState.probeMod;
                sensorDetails.Add($" + Probe: <color=#00FF00>{ewState.probeMod:0}</color>");
            }

            if (State.ECMJamming(actor) != 0) {
                checkResult -= State.ECMJamming(actor);
                sensorDetails.Add($" + Jammed: <color=#FF0000>{State.ECMJamming(actor):-0}</color>");
            }

            sensorDetails.Add(" = Result: ");
            if (checkResult >= 0) {
                sensorDetails.Add($"<color=#00FF00>{checkResult:0}</color>");
            } else {
                sensorDetails.Add($"<color=#FF0000>{checkResult:0}</color>");
            }
            sensorDetails.Add("\n");

            // Sensor range
            SensorScanType checkLevel = SensorScanType.NoInfo;
            if (checkLevel > SensorScanType.DentalRecords) {
                checkLevel = SensorScanType.DentalRecords;
            } else if (checkLevel < SensorScanType.NoInfo) {
                checkLevel = SensorScanType.NoInfo;
            } else {
                checkLevel = (SensorScanType)checkResult;
            }
            details.Add($"Sensors Lock:{sensorsRange:0}m Info:[{checkLevel.Label()}]\n");
            details.AddRange(sensorDetails);

            // Sensor check:(+/-0) SensorScanLevel:
            if (ewState.ecmMod != 0) {
                details.Add($"ECM => Enemy Modifier:<color=#FF0000>{ewState.ecmMod:-0}</color>");
            }
            if (ewState.stealthMod != 0) {
                details.Add($"STEALTH => Enemy Modifier:<color=#FF0000>{ewState.stealthMod:-0}</color>");
            }

            string tooltipText = String.Join("", details.ToArray());
            return tooltipText;
        }
    }

}
