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
            Mod.Log.Trace("CHUDSP:RDC - entered.");
            if (__instance != null && __instance.DisplayedCombatant != null) {
                AbstractActor target = __instance.DisplayedCombatant as AbstractActor;
                // We can receive a building here, so 
                if (target != null) {
                    if (target.Combat.HostilityMatrix.IsLocalPlayerEnemy(target.team)) {

                        SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, ModState.LastPlayerActorActivated);

                        if (scanType < SensorScanType.Vector) {
                            //// Hide the evasive indicator, hide the buffs and debuffs
                            //Traverse hideEvasionIndicatorMethod = Traverse.Create(__instance).Method("HideEvasiveIndicator", new object[] { });
                            //hideEvasionIndicatorMethod.GetValue();
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                        } else if (scanType < SensorScanType.StructureAnalysis) {
                            // Hide the buffs and debuffs
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                        } else if (scanType >= SensorScanType.StructureAnalysis) {
                            // Do nothing; normal state
                        }
                    }

                    // Calculate stealth pips
                    Traverse stealthDisplayT = Traverse.Create(__instance).Field("stealthDisplay");
                    CombatHUDStealthBarPips stealthDisplay = stealthDisplayT.GetValue<CombatHUDStealthBarPips>();
                    VfxHelper.CalculateMimeticPips(stealthDisplay, target);
                }
            }
        }
    }


    [HarmonyPatch(typeof(CombatHUDStatusPanel), "HideStealthIndicator")]
    public static class CombatHUDStatusPanel_HideStealthIndicator {
        public static void Postfix(CombatHUDStatusPanel __instance) {
            Mod.Log.Trace("CHUDSP:HSI - entered.");
        }
    }

    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowStealthIndicators")]
    [HarmonyPatch(new Type[] {  typeof(AbstractActor), typeof(Vector3) })]
    public static class CombatHUDStatusPanel_ShowStealthIndicators_Vector3 {
        public static void Postfix(CombatHUDStatusPanel __instance, AbstractActor target, Vector3 previewPos, CombatHUDStealthBarPips ___stealthDisplay) {
            if (___stealthDisplay == null) { return; }
            Mod.Log.Trace("CHUDSP:SSI:Vector3 - entered.");

            VfxHelper.CalculateMimeticPips(___stealthDisplay, target, previewPos);
        }
    }

    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowStealthIndicators")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(float) })]
    public static class CombatHUDStatusPanel_ShowStealthIndicators_float {
        public static void Postfix(CombatHUDStatusPanel __instance, AbstractActor target, float previewStealth, CombatHUDStealthBarPips ___stealthDisplay) {
            if (___stealthDisplay == null) { return; }
            Mod.Log.Trace("CHUDSP:SSI:float - entered.");

            VfxHelper.CalculateMimeticPips(___stealthDisplay, target);
        }
    }


    [HarmonyPatch()]
    public static class CombatHUDStatusPanel_ShowActorStatuses {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDStatusPanel), "ShowActorStatuses", new Type[] { typeof(AbstractActor) });
        }

        public static void Postfix(CombatHUDStatusPanel __instance) {
            Mod.Log.Trace("CHUDSP:SAS - entered.");

            if (__instance.DisplayedCombatant != null) {
                Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffIconMethod = Traverse.Create(__instance).Method("ShowDebuff", iconMethodParams);
                Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                EWState actorState = new EWState(actor);

                Traverse svgAssetT = Traverse.Create(__instance.DisplayedCombatant.Combat.DataManager).Property("SVGCache");
                object svgCache = svgAssetT.GetValue();
                Traverse svgCacheT = Traverse.Create(svgCache).Method("GetAsset", new Type[] { typeof(string) });

                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {

                    SVGAsset icon = svgCacheT.GetValue<SVGAsset>(new object[] { ModIcons.VisionAndSensors });
                    Text title = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TITLE_VISION_AND_SENSORS]);
                    showBuffIconMethod.GetValue(new object[] { icon, title, new Text(BuildToolTip(actor)), __instance.effectIconScale, false });

                    // Disable the sensors
                    if (actor.Combat.TurnDirector.CurrentRound == 1) {
                        SVGAsset sensorsDisabledIcon = svgCacheT.GetValue<SVGAsset>(new object[] { ModIcons.SensorsDisabled });
                        Text sensorsDisabledTitle = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TITLE_SENSORS_DISABLED]);
                        Text sensorsDisabledText = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TEXT_SENSORS_DISABLED]);
                        showDebuffIconMethod.GetValue(new object[] { 
                            sensorsDisabledIcon, sensorsDisabledTitle, sensorsDisabledText, __instance.effectIconScale, false });
                    }
                }

                if (actorState.HasECMShield()) {
                    SVGAsset icon = svgCacheT.GetValue<SVGAsset>(new object[] { ModIcons.ECMShielded });
                    Text title = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TITLE_ECM_SHIELD]);
                    Text text = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TEXT_ECM_SHIELD]);
                    showBuffIconMethod.GetValue(new object[] { icon, title, text, __instance.effectIconScale, false });
                }

                if (actorState.ECMJammedMod() != 0) {
                    SVGAsset icon = svgCacheT.GetValue<SVGAsset>(new object[] { ModIcons.ECMJammed });
                    Text title = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TITLE_ECM_JAMMING]);
                    Text text = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TEXT_ECM_JAMMING]);
                    showDebuffIconMethod.GetValue(new object[] { icon, title, text, __instance.effectIconScale, false });
                }

                if (actorState.HasStealth()) {
                    SVGAsset icon = svgCacheT.GetValue<SVGAsset>(new object[] { ModIcons.Stealth });
                    Text title = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TITLE_STEALTH]);
                    Text text = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TEXT_STEALTH]);
                    showBuffIconMethod.GetValue(new object[] { icon, title, text, __instance.effectIconScale, false });
                }

                if (actorState.HasMimetic()) {
                    SVGAsset icon = svgCacheT.GetValue<SVGAsset>(new object[] { ModIcons.Mimetic });
                    Text title = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TITLE_MIMETIC]);
                    Text text = new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TEXT_MIMETIC]);
                    showBuffIconMethod.GetValue(new object[] { icon, title, text, __instance.effectIconScale, false });
                }
            }
        }

        private static string BuildToolTip(AbstractActor actor) {
            EWState ewState = new EWState(actor);
            //Mod.Log.Debug($"EW State for actor:{CombatantUtils.Label(actor)} = {ewState}");

            List<string> details = new List<string>();

            float visualLockRange = VisualLockHelper.GetVisualLockRange(actor);
            float visualScanRange = VisualLockHelper.GetVisualScanRange(actor);
            details.Add(
                new Text(Mod.Config.LocalizedText[ModConfig.LT_PANEL_VISUAL_RANGE], 
                    new object[] { visualLockRange, visualScanRange, ModState.GetMapConfig().UILabel() })
                    .ToString()
                );
            
            SensorScanType checkLevel;
            if (ewState.GetCurrentEWCheck() > (int)SensorScanType.DentalRecords) { checkLevel = SensorScanType.DentalRecords; } 
            else if (ewState.GetCurrentEWCheck() < (int)SensorScanType.NoInfo) { checkLevel = SensorScanType.NoInfo; } 
            else { checkLevel = (SensorScanType)ewState.GetCurrentEWCheck(); }
            float sensorsRange = SensorLockHelper.GetSensorsRange(actor);
            string sensorColor = ewState.GetCurrentEWCheck() >= 0 ? "00FF00" : "FF0000";
            details.Add(
                new Text(Mod.Config.LocalizedText[ModConfig.LT_PANEL_SENSOR_RANGE], 
                    new object[] { sensorColor, sensorsRange, sensorColor, ewState.GetSensorsRangeMulti(), checkLevel.Label() })
                    .ToString()
                );

            // Sensor details
            ewState.BuildCheckTooltip(details);

            // TODO: FIX ME!

            //if (ewState.probeMod > 0) {
            //    sensorDetails.Add($" (Probe:<color=#00FF00>{ewState.probeMod:0}</color>)");
            //}              

            //if (ewState.probeMod > 0) {
            //    checkResult += ewState.probeMod;
            //    sensorDetails.Add($" + Probe: <color=#00FF00>{ewState.probeMod:0}</color>");
            //}

            //if (State.ECMJamming(actor) != 0) {
            //    checkResult -= State.ECMJamming(actor);
            //    sensorDetails.Add($" + Jammed: <color=#FF0000>{State.ECMJamming(actor):-0}</color>");
            //}

            //// Sensor check:(+/-0) SensorScanLevel:
            //if (ewState.ecmMod != 0) {
            //    details.Add($"ECM => Enemy Modifier:<color=#FF0000>{ewState.ecmMod:-0}</color>");
            //}

            string tooltipText = String.Join("", details.ToArray());
            return tooltipText;
        }
    }

}
