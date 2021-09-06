using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using HBS;
using IRBTModUtils.Extension;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(CombatHUDStatusPanel), "RefreshDisplayedCombatant")]
    public static class CombatHUDStatusPanel_RefreshDisplayedCombatant {

        public static void Postfix(CombatHUDStatusPanel __instance, List<CombatHUDStatusIndicator> ___Buffs, List<CombatHUDStatusIndicator> ___Debuffs) {
            Mod.UILog.Trace?.Write("CHUDSP:RDC - entered.");
            if (__instance != null && __instance.DisplayedCombatant != null) {
                AbstractActor target = __instance.DisplayedCombatant as AbstractActor;
                // We can receive a building here, so 
                if (target != null) {
                    if (target.Combat.HostilityMatrix.IsLocalPlayerEnemy(target.team)) {

                        SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, ModState.LastPlayerActorActivated);

                        // Hide the buffs and debuffs if the current scanType is less than allInfo
                        if (scanType < SensorScanType.AllInformation) {
                            //// Hide the buffs and debuffs
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
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


    // This patch only exists to try to isolate the bug being reported in RT where the HUD 'vanishes'. See
    //   https://github.com/BattletechModders/LowVisibility/issues/39 for details. I have no code that interferes in this logic
    //   before the postfix above on RefreshDisplayedCombatant, so we're replicating ShowEffectStatuses faithfully here
    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowEffectStatuses")]
    static class CombatHUDStatusPanel_ShowEffectStatuses
    {
        static bool Prefix(CombatHUDStatusPanel __instance, AbstractActor actor, AbilityDef.SpecialRules specialRulesFilter, Vector3 worldPos, Dictionary<string, CombatHUDStatusIndicator> ___effectDict)
        {
            Mod.UILog.Debug?.Write($"Updating StatusEffect Panel for actor: {CombatantUtils.Label(actor)}");

            try
            {
                List<EffectData> effectsOnActor = new List<EffectData>();

                foreach (Effect effect in ModState.Combat.EffectManager.GetAllEffectsTargeting(actor))
                {

                    if (effect == null || effect.EffectData == null) 
                    {
                        Mod.UILog.Warn?.Write($"Effect with id: {effect?.id} has no effectData! Effect is from creatorGUID: {effect?.creatorGUID} creatorID: {effect?.creatorID} " +
                            $"with targetId: {effect?.targetID}");
                        continue;
                    }

                    if (effect.EffectData.targetingData.specialRules != AbilityDef.SpecialRules.Aura && 
                        (effect.EffectData.targetingData.effectTriggerType != EffectTriggerType.OnDamaged || effect.triggerCount != 0)
                        )
                    {
                        Mod.UILog.Debug?.Write($"Adding effectId: {effect?.EffectData?.Description?.Id} with name: {effect?.EffectData?.Description?.Name}");
                        effectsOnActor.Add(effect.EffectData);
                    }
                }

                if (specialRulesFilter == AbilityDef.SpecialRules.Aura)
                {
                    if (actor.AuraCache == null)
                    {
                        Mod.UILog.Warn?.Write($"Actor: {CombatantUtils.Label(actor)} has a null aura cache.  This should not happen!");
                    }
                    else
                    {
                        Dictionary<string, List<EffectData>> dictionary = actor.AuraCache.PreviewAurasAffectingMe(actor, worldPos, null);
                        foreach (string key in dictionary.Keys)
                        {
                            List<EffectData> collection = dictionary[key];
                            Mod.UILog.Debug?.Write("Adding collection from aura.");
                            effectsOnActor.AddRange(collection);
                        }
                    }
                }


                Traverse shouldShowEffectT = Traverse.Create(__instance).Method("ShouldShowEffect", new Type[] { typeof(EffectData), typeof(AbilityDef.SpecialRules)});
                Traverse showDebuffT = Traverse.Create(__instance).Method("ShowBuff", new Type[] { typeof(string), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) });
                Traverse showBuffT = Traverse.Create(__instance).Method("ShowDebuff", new Type[] { typeof(string), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) });
                if (shouldShowEffectT == null || showDebuffT == null || showBuffT == null)
                {
                    Mod.UILog.Error?.Write("Failed to traverse necessary methods! Notify FrostRaptor - this should not happen!");
                    return false;
                }

                ___effectDict.Clear();
                for (int i = 0; i < effectsOnActor.Count; i++)
                {
                    EffectData effectData = effectsOnActor[i];

                    if (effectData == null || effectData.Description == null || 
                        effectData.Description.Id == null || effectData.Description.Name == null)
                    {
                        Mod.UILog.Error?.Write($"EffectData {effectData?.Description?.Name} has no description, id, or name! Cannot process, skipping!");
                        continue;
                    }

                    if (string.IsNullOrEmpty(effectData?.Description?.Icon)) continue; // No icon to display, skip.

                    bool shouldShowEffect = shouldShowEffectT.GetValue<bool>(new object[] { effectData, specialRulesFilter });
                    bool alreadyShown = ___effectDict.ContainsKey(effectData.Description.Id);
                    Mod.UILog.Debug?.Write($" -- Effect with name: {effectData?.Description?.Name} and Id: {effectData?.Description?.Id} has shouldShowEffect: {shouldShowEffect} and alreadyShown: {alreadyShown}");

                    string effectId = effectData.Description.Id;
                    if (shouldShowEffect && !alreadyShown)
                    {
                        Mod.UILog.Debug?.Write($" -- Adding effect with name: {effectData?.Description?.Name} and Id: {effectData?.Description?.Id} to buff list.");
                        int num = effectsOnActor.FindAll((EffectData x) => x.Description.Id == effectId).Count;
                        if (effectData.statisticData != null && 
                            effectData.statisticData.targetCollection == StatisticEffectData.TargetCollection.Weapon && 
                            effectData.statisticData.targetWeaponSubType != WeaponSubType.Melee)
                        {
                            num = ((num > 1) ? (num / actor.Weapons.Count) : num);
                        }
                        Text text = CombatHUDStatusPanel.ProcessDetailString(effectData, (num > 0) ? num : 1);

                        CombatHUDStatusIndicator combatHUDStatusIndicator;
                        if (effectsOnActor[i].nature == EffectNature.Debuff)
                        {
                            combatHUDStatusIndicator = showDebuffT.GetValue<CombatHUDStatusIndicator>(new object[] {
                                effectData.Description.Icon, new Text(effectData.Description.Name, Array.Empty<object>()), text, __instance.effectIconScale, false
                            });
                        }
                        else
                        {
                            combatHUDStatusIndicator = showBuffT.GetValue<CombatHUDStatusIndicator>(new object[] {
                                effectData.Description.Icon, new Text(effectData.Description.Name, Array.Empty<object>()), text, __instance.effectIconScale, false
                            });
                        }

                        if (combatHUDStatusIndicator != null)
                        {
                            combatHUDStatusIndicator.AddTooltipString(text, effectsOnActor[i].nature);
                            ___effectDict[effectId] = combatHUDStatusIndicator;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Mod.UILog.Error?.Write(e, $"Failed to log status effects for actor: {CombatantUtils.Label(actor)} at position: {worldPos}");
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowStealthIndicators")]
    [HarmonyPatch(new Type[] {  typeof(AbstractActor), typeof(Vector3) })]
    public static class CombatHUDStatusPanel_ShowStealthIndicators_Vector3 {
        public static void Postfix(CombatHUDStatusPanel __instance, AbstractActor target, Vector3 previewPos, CombatHUDStealthBarPips ___stealthDisplay) {
            if (___stealthDisplay == null) { return; }
            Mod.UILog.Trace?.Write("CHUDSP:SSI:Vector3 - entered.");

            VfxHelper.CalculateMimeticPips(___stealthDisplay, target, previewPos);
        }
    }

    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowStealthIndicators")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(float) })]
    public static class CombatHUDStatusPanel_ShowStealthIndicators_float {
        public static void Postfix(CombatHUDStatusPanel __instance, AbstractActor target, float previewStealth, CombatHUDStealthBarPips ___stealthDisplay) {
            if (___stealthDisplay == null) { return; }
            Mod.UILog.Trace?.Write("CHUDSP:SSI:float - entered.");

            VfxHelper.CalculateMimeticPips(___stealthDisplay, target);
        }
    }

    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowActorStatuses")]
    public static class CombatHUDStatusPanel_ShowActorStatuses {

        public static void Postfix(CombatHUDStatusPanel __instance) {
            Mod.UILog.Trace?.Write("CHUDSP:SAS - entered.");

            if (__instance.DisplayedCombatant != null) {
                Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffIconMethod = Traverse.Create(__instance).Method("ShowDebuff", iconMethodParams);
                Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                EWState actorState = new EWState(actor);

                DataManager dm = __instance.DisplayedCombatant.Combat.DataManager;

                Mod.UILog.Info?.Write($"Updating icon tooltips for actor: {actor.DistinctId()}");
                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                Mod.UILog.Info?.Write($"  -- actor isPlayer: {isPlayer}");
                if (isPlayer) {

                    SVGAsset icon = dm.GetObjectOfType<SVGAsset>(Mod.Config.Icons.VisionAndSensors, BattleTechResourceType.SVGAsset);
                    Text title = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TITLE_VISION_AND_SENSORS]);
                    string tooltipText = BuildToolTip(actor);
                    Mod.UILog.Info?.Write($"  -- visionAndSensors tooltip text is: {tooltipText}");
                    showBuffIconMethod.GetValue(new object[] { icon, title, new Text(tooltipText), __instance.effectIconScale, false });

                    // Disable the sensors
                    if (actor.Combat.TurnDirector.CurrentRound == 1) {
                        SVGAsset sensorsDisabledIcon = dm.GetObjectOfType<SVGAsset>(Mod.Config.Icons.SensorsDisabled, BattleTechResourceType.SVGAsset);
                        Text sensorsDisabledTitle = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TITLE_SENSORS_DISABLED]);
                        Text sensorsDisabledText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_SENSORS_DISABLED]);
                        showDebuffIconMethod.GetValue(new object[] { 
                            sensorsDisabledIcon, sensorsDisabledTitle, sensorsDisabledText, __instance.effectIconScale, false });
                    }
                }

                if (actorState.GetRawECMShield() != 0|| actorState.GetRawECMJammed() != 0 || actorState.ProbeCarrierMod() != 0 || actorState.PingedByProbeMod() != 0 ||
                    actorState.GetRawStealth() != null || actorState.GetRawMimetic() != null || actorState.GetRawNarcEffect() != null || actorState.GetRawTagEffect() != null) {
                    // Build out the detailed string
                    StringBuilder sb = new StringBuilder();

                    if (actorState.GetRawECMShield() != 0) {
                        // A positive is good, a negative is bad
                        string color = actorState.GetRawECMShield() >= 0 ? "00FF00" : "FF0000";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_ECM_SHIELD], 
                            new object[] { color, actorState.GetRawECMShield() }
                            ).ToString();
                        sb.Append(localText);
                    }

                    if (actorState.GetRawECMJammed() != 0) {
                        // A positive (after normalization) is good, a negative is bad
                        string color = -1 * actorState.GetRawECMJammed() >= 0 ? "00FF00" : "FF0000";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_ECM_JAMMING],
                            new object[] { color, -1 * actorState.GetRawECMJammed() }
                            ).ToString();
                        sb.Append(localText);
                    }

                    if (actorState.ProbeCarrierMod() != 0) {
                        // A positive is good, a negative is bad
                        string color = actorState.ProbeCarrierMod() >= 0 ? "00FF00" : "FF0000";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_PROBE_CARRIER],
                            new object[] { color, -1 * actorState.ProbeCarrierMod() }
                            ).ToString();
                        sb.Append(localText);
                    }

                    // Armor
                    if (actorState.GetRawStealth() != null) {
                        string color = "00FF00";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_STEALTH],
                            new object[] { color, actorState.GetRawStealth().MediumRangeAttackMod, actorState.GetRawStealth().LongRangeAttackMod, actorState.GetRawStealth().ExtremeRangeAttackMod, }
                            ).ToString();
                        sb.Append(localText);
                    }

                    if (actorState.GetRawMimetic() != null) {
                        // A positive is good (harder to hit), should be no negative?
                        string color = "00FF00";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_MIMETIC],
                            new object[] { color, actorState.CurrentMimeticPips() }
                            ).ToString();
                        sb.Append(localText);
                    }

                    // Transient effects
                    if (actorState.PingedByProbeMod() != 0) {
                        // A positive (after normalization) is good, a negative is bad
                        string color = -1 * actorState.PingedByProbeMod() >= 0 ? "00FF00" : "FF0000";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_PROBE_EFFECT],
                            new object[] { color, -1 * actorState.PingedByProbeMod() }
                            ).ToString();
                        sb.Append(localText);
                    }

                    if (actorState.GetRawNarcEffect() != null) {
                        // A positive (after normalization) is good, a negative is bad
                        string color = -1 * actorState.GetRawNarcEffect().AttackMod >= 0 ? "00FF00" : "FF0000";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_NARC_EFFECT],
                            new object[] { color, -1 * actorState.GetRawNarcEffect().AttackMod }
                            ).ToString();
                        sb.Append(localText);
                    }

                    if (actorState.GetRawTagEffect() != null) {
                        // A positive (after normalization) is good, a negative is bad
                        string color = -1 * actorState.GetRawTagEffect().AttackMod >= 0 ? "00FF00" : "FF0000";
                        string localText = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TEXT_EW_TAG_EFFECT],
                            new object[] { color, -1 * actorState.GetRawTagEffect().AttackMod }
                            ).ToString();
                        sb.Append(localText);
                    }

                    SVGAsset icon = dm.GetObjectOfType<SVGAsset>(Mod.Config.Icons.ElectronicWarfare, BattleTechResourceType.SVGAsset);
                    Text title = new Text(Mod.LocalizedText.Tooltips[ModText.LT_TT_TITLE_EW]);
                    Mod.Log.Info?.Write($"  -- effects tooltip text is: {sb}");
                    showBuffIconMethod.GetValue(new object[] { icon, title, new Text(sb.ToString()), __instance.effectIconScale, false });
                }

            }
        }

        private static string BuildToolTip(AbstractActor actor) {
            //Mod.Log.Debug?.Write($"EW State for actor:{CombatantUtils.Label(actor)} = {ewState}");

            List<string> details = new List<string>();

            // Visuals check
            float visualLockRange = VisualLockHelper.GetVisualLockRange(actor);
            float visualScanRange = VisualLockHelper.GetVisualScanRange(actor);
            details.Add(
                new Text(Mod.LocalizedText.StatusPanel[ModText.LT_PANEL_VISUALS], 
                    new object[] { visualLockRange, visualScanRange, ModState.GetMapConfig().UILabel() })
                    .ToString()
                );

            // Sensors check
            EWState ewState = new EWState(actor);

            int totalDetails = ewState.GetCurrentEWCheck() + ewState.AdvancedSensorsMod();
            SensorScanType checkLevel = SensorScanTypeHelper.DetectionLevelForCheck(totalDetails);
            
            float rawRangeMulti = SensorLockHelper.GetAllSensorRangeMultipliers(actor);
            float rangeMulti = rawRangeMulti + ewState.GetSensorsRangeMulti();
            
            float sensorsRange = SensorLockHelper.GetSensorsRange(actor);
            string sensorColor = ewState.GetCurrentEWCheck() >= 0 ? "00FF00" : "FF0000";
            details.Add(
                new Text(Mod.LocalizedText.StatusPanel[ModText.LT_PANEL_SENSORS], 
                    new object[] { sensorColor, sensorsRange, sensorColor, rangeMulti, checkLevel.Label() })
                    .ToString()
                );

            // Details
            //{ LT_PANEL_DETAILS, "  Total: <color=#{0}>{1:+0;-#}</color><size=90%> Roll: <color=#{2}>{3:+0;-#}</color> Tactics: <color=#00FF00>{4:+0;-#}</color> AdvSen: <color=#{5}>{6:+0;-#}</color>\n"
            string totalColor = totalDetails >= 0 ? "00FF00" : "FF0000";
            string checkColor = ewState.GetRawCheck() >= 0 ? "00FF00" : "FF0000";
            string advSenColor = ewState.AdvancedSensorsMod() >= 0 ? "00FF00" : "FF0000";
            details.Add(
                new Text(Mod.LocalizedText.StatusPanel[ModText.LT_PANEL_DETAILS],
                    new object[] { totalColor, totalDetails, checkColor, ewState.GetRawCheck(), ewState.GetRawTactics(), advSenColor, ewState.AdvancedSensorsMod() })
                    .ToString()
                );

            //  Heat Vision
            if (ewState.GetRawHeatVision() != null) {
                // { LT_PANEL_HEAT, "<b>Thermals</b><size=90%> Mod:<color=#{0}>{1:+0;-#}</color> / {2} heat Range:{3}m\n" },
                HeatVision heatVis = ewState.GetRawHeatVision();
                // Positive is bad, negative is good
                string modColor = heatVis.AttackMod >= 0 ? "FF0000" : "00FF00";
                details.Add(
                    new Text(Mod.LocalizedText.StatusPanel[ModText.LT_PANEL_HEAT],
                        new object[] { modColor, heatVis.AttackMod, heatVis.HeatDivisor, heatVis.MaximumRange })
                        .ToString()
                    );
            }

            //  Zoom Vision
            if (ewState.GetRawZoomVision() != null) {
                // { LT_PANEL_ZOOM, "<b>Zoom</b><size=90%> Mod:<color=#{0}>{1:+0;-#}</color? Cap:<color=#{2}>{3:+0;-#}</color> Range:{4}m\n" },
                ZoomVision zoomVis = ewState.GetRawZoomVision();
                // Positive is bad, negative is good
                string modColor = zoomVis.AttackMod >= 0 ? "FF0000" : "00FF00";
                string capColor = zoomVis.AttackCap >= 0 ? "FF0000" : "00FF00";
                details.Add(
                    new Text(Mod.LocalizedText.StatusPanel[ModText.LT_PANEL_ZOOM],
                        new object[] { modColor, zoomVis.AttackMod, capColor, zoomVis.AttackCap, zoomVis.MaximumRange })
                        .ToString()
                    );
            }

            string tooltipText = String.Join("", details.ToArray());
            return tooltipText;
        }
    }

}
