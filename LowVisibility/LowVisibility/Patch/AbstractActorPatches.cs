using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.Data;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

    // Initializes any custom status effects. Without this, they wont' be read.
    [HarmonyPatch(typeof(AbstractActor), "InitEffectStats")]
    public static class AbstractActor_InitEffectStats {
        private static void Postfix(AbstractActor __instance) {
            Mod.Log.Trace("AA:IES entered");

            __instance.StatCollection.AddStatistic<int>(ModStats.SensorCheck, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.TacticsMod, 0);

            __instance.StatCollection.AddStatistic<bool>(ModStats.ECMCarrier, false);
            __instance.StatCollection.AddStatistic<int>(ModStats.ECMShield, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.ECMJammed, 0);

            __instance.StatCollection.AddStatistic<int>(ModStats.Jammer, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.Probe, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.Stealth, 0);

            __instance.StatCollection.AddStatistic<bool>(ModStats.SharesSensors, false);

            __instance.StatCollection.AddStatistic<int>(ModStats.StealthMoveMod, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.VismodeZoom, 0);
            __instance.StatCollection.AddStatistic<int>(ModStats.VismodeHeat, 0);
        }
    } 

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {

        public static void Prefix(AbstractActor __instance, int stackItemID) {
            if (stackItemID == -1 || __instance == null || __instance.HasBegunActivation) {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DO ES NOTHING. GAH!
                return;
            }
            
            //ECMHelper.UpdateECMState(__instance);

            //VisibilityHelper.UpdateVisibilityForAllTeams(__instance.Combat);

            Mod.Log.Debug($"=== AbstractActor:OnActivationBegin:pre - processing {CombatantUtils.Label(__instance)}");
            if (__instance.team == __instance.Combat.LocalPlayerTeam) {
                State.LastPlayerActor = __instance.GUID;
            }

            List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance)
                .FindAll((Effect x) => x.EffectData.effectType == EffectType.StatisticEffect && x.EffectData.statisticData.statName == ModStats.ECMCarrier);
            bool HasECM = list.Count > 0;
            Mod.Log.Debug($" ACTOR HAS ECM: Actor: {CombatantUtils.Label(__instance)} hasECM: {__instance.HasECMAbilityInstalled}");
            if (HasECM) {
                Mod.Log.Debug(" ADDING ECM CARRIER LOOP"); 
                // Bubble
                ParticleSystem psECMLoop = __instance.GameRep.PlayVFXAt(__instance.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
                psECMLoop.Stop(true);

                foreach (Transform child in psECMLoop.transform) {
                    Mod.Log.Debug($"  - Found GO: {child.gameObject.name}");
                    if (child.gameObject.name == "electric dome circumference slow") {
                        //Mod.Log.Debug($"  - Found dome circumference");
                        //child.gameObject.SetActive(true);
                        //child.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                        child.gameObject.SetActive(false);
                    } else if (child.gameObject.name == "sphere") {
                        Mod.Log.Debug($"  - Found sphere");
                        child.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                        ParticleSystemRenderer spherePSR = child.gameObject.transform.GetComponent<ParticleSystemRenderer>();

                        Mod.Log.Debug($"  - Material OLD shader name: {spherePSR.material.shader.name}");
                        //Shader rainDropShader = Shader.Find("vfxMatPrtl_rainDrop_noFoW_alpha");
                        //Mod.Log.Debug($"  - Shader found? {rainDropShader != null} ?? {rainDropShader?.name}");
                        //Material rainDropMat = new Material(rainDropMat);

                        DataManager dm = UnityGameInstance.BattleTechGame.DataManager;
                        var rainDropMat = dm.Get(BattleTechResourceType.AssetBundle, "vfxMatPrtl_rainDrop_noFoW_alpha");
                        Mod.Log.Debug($"  - Material found: {rainDropMat != null}");

                        var mat = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "vfxMatPrtl_rainDrop_noFoW_alpha");
                        Mod.Log.Debug($"  - STUPID SEARCH Material found: {mat != null}");
                        var mat2 = new Material(mat);
                        mat2.color = new Color(15, 82, 186, 100);

                        spherePSR.material = mat2;
                        Mod.Log.Debug($"  - Material NEW shader name: {spherePSR.material.shader.name}");
                    } else {
                        Mod.Log.Debug($"  - Disabling GO: {child.gameObject.name}");
                        child.gameObject.SetActive(false);
                    }
                }
                
                //ParticleSystem.MainModule psECMLoopMM = psECMLoop.main;
                //psECMLoopMM.scalingMode = ParticleSystemScalingMode.Hierarchy;
                //psECMLoopMM.startSizeMultiplier = 3.0f;
                //ParticleSystem.ShapeModule psECMLoopSM = psECMLoop.shape;
                //psECMLoopSM.meshScale = 3f;
                //psECMLoop.transform.localScale = new Vector3(3f, 3f, 3f);

                psECMLoop.Play(true);

                // AoE loop
                ParticleSystem psECMCarrier = __instance.GameRep.PlayVFXAt(__instance.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
                //psECMCarrier.transform.localScale = new Vector3(1f, 1f, 1f);

                WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_ecm_start, __instance.GameRep.audioObject, null, null);

                Mod.Log.Debug(" DONE ECM CARRIER LOOP");
            }

        }
    }

    [HarmonyPatch(typeof(AbstractActor), "UpdateLOSPositions")]
    public static class AbstractActor_UpdateLOSPositions {
        public static void Prefix(AbstractActor __instance) {
            // Check for teamID; if it's not present, unit hasn't spawned yet. Defer to UnitSpawnPointGameLogic::SpawnUnit for these updates
            if (State.TurnDirectorStarted && __instance.TeamId != null) {
                Mod.Log.Debug($"AbstractActor_UpdateLOSPositions:pre - entered for {CombatantUtils.Label(__instance)}.");
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "HasLOSToTargetUnit")]
    public static class AbstractActor_HasLOSToTargetUnit {
        public static void Postfix(AbstractActor __instance, ref bool __result, ICombatant targetUnit) {
            //LowVisibility.Logger.Debug("AbstractActor:HasLOSToTargetUnit:post - entered.");

            // Forces you to be able to see targets that are only blips
            __result = __instance.VisibilityToTargetUnit(targetUnit) >= VisibilityLevel.Blip0Minimum;
            Mod.Log.Trace($"Actor{CombatantUtils.Label(__instance)} has LOSToTargetUnit? {__result} " +
                $"to target:{CombatantUtils.Label(targetUnit as AbstractActor)}");
            //LowVisibility.Logger.Trace($"Called from:{new StackTrace(true)}");
        }
    }


    [HarmonyPatch(typeof(AbstractActor), "CanDetectPositionNonCached")]
    public static class AbstractActor_CanDetectPositionNonCached {
        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CDPNC: source{CombatantUtils.Label(__instance)} checking detection " +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CanSeeTargetAtPositionNonCached")]
    public static class AbstractActor_CanSeeTargetAtPositionNonCached {
        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CSTAPNC: source{__instance} checking vision" +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    //[HarmonyPatch(typeof(AbstractActor), "HasECMAbilityInstalled", MethodType.Getter)]
    //public static class AbstractActor_HasECMAbilityInstalled_Getter {
    //    public static void Postfix(AbstractActor __instance, ref bool __result) {
    //        Mod.Log.Trace("AA:HECMAI:GET entered");

    //        List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance)
    //            .FindAll((Effect x) => x.EffectData.effectType == EffectType.StatisticEffect && 
    //            (x.EffectData.Description.Id == "ECMStealth_GhostEffect" || x.EffectData.statisticData.statName == ModStats.ECMCarrier));

    //        Mod.Log.Debug($"  Found {list.Count} effects for actor: {CombatantUtils.Label(__instance)}");
    //        __result = list.Count > 0;
    //    }
    //}

    // ParentECM Carrier is the unit carrying the ECM... duh
    //[HarmonyPatch(typeof(AbstractActor), "ParentECMCarrier", MethodType.Getter)]
    //public static class AbstractActor_ParentECMCarrier_Getter {
    //    public static void Postfix(AbstractActor __instance, ref bool __result) {
    //        Mod.Log.Trace("AA:PECMC:GET entered");

    //        List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance)
    //            .FindAll((Effect x) => x.EffectData.effectType == EffectType.StatisticEffect &&
    //            (x.EffectData.Description.Id == "ECMStealth_GhostEffect" || x.EffectData.statisticData.statName == ModStats.ECMCarrier));
    //        __result = list.Count > 0;
    //    }
    //}

    [HarmonyPatch(typeof(AbstractActor), "CreateEffect")]
    [HarmonyPatch(new Type[] { typeof(EffectData), typeof(Ability), typeof(string), typeof(int), typeof(AbstractActor), typeof(bool) })]
    public static class AbstractActor_CreateEffect {
        public static void Postfix(AbstractActor __instance, EffectData effect, Ability fromAbility, string effectId, int stackItemUID, AbstractActor creator, bool skipLogging) {
            Mod.Log.Trace("AA:CE entered");


        }
    }

    //[HarmonyPatch(typeof(AbstractActor), "OnAuraAdded")]
    //public static class AbstractActor_OnAuraAdded {
    //    public static void Postfix(AbstractActor __instance, MessageCenterMessage message) {
    //        Mod.Log.Debug("AA:OAA entered");

    //        AuraAddedMessage auraAddedMessage = message as AuraAddedMessage;
    //        if (auraAddedMessage.targetID == __instance.GUID) {
    //            Mod.Log.Debug($"Self aura found for actor:{CombatantUtils.Label(__instance)} with ID: {auraAddedMessage.effectData.Description.Id}");
    //            if (auraAddedMessage.effectData.statisticData.statName == ModStats.ECMCarrier) {
    //                if (__instance.Combat.TurnDirector.IsInterleaved) {
    //                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(auraAddedMessage.creatorID, auraAddedMessage.targetID, new Text("ECM PROTECTED", new object[0]), FloatieMessage.MessageNature.Buff));
    //                }
    //                if (__instance.GameRep != null) {
    //                    __instance.GameRep.PlayVFXAt(__instance.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMtargetAdd_burst", true, Vector3.zero, true, -1f);
    //                    WwiseManager.PostEvent<AudioEventList_ecm>(AudioEventList_ecm.ecm_enter, __instance.GameRep.audioObject, null, null);
    //                    if (__instance.HasECMAbilityInstalled) {
    //                        string vfxName = "vfxPrfPrtl_ECM_loop";
    //                        __instance.GameRep.PlayVFXAt(__instance.GameRep.thisTransform, Vector3.zero, vfxName, true, Vector3.zero, false, -1f);
    //                        __instance.GameRep.PlayVFXAt(__instance.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
    //                        WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_ecm_start, __instance.GameRep.audioObject, null, null);
    //                    }
    //                }
    //                __instance.Combat.MessageCenter.PublishMessage(new StealthChangedMessage(__instance.GUID, __instance.StealthPipsCurrent));
    //            }
    //        }

    //        // TODO: Add else if conditional?
    //    }
    //}
}
