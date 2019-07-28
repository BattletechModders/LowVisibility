using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.UI;
using Harmony;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {
    public static class VfxHelper {

        public static Material MaterialSensorStealthBubble;
        public static Material MaterialSensorStealthCapsule;
        public static Material MaterialECMBubble;
        
        public static void Initialize(CombatGameState cgs) {
            Mod.Log.Debug("== INITIALIZING MATERIALS ==");

            // Try to find the asset bundles for each thing
            VersionManifestEntry[] allEntries = cgs.DataManager.ResourceLocator.AllEntries();
            foreach (VersionManifestEntry vme in allEntries) {
                if (vme.Id.ToLower() == "vfxPrfPrtl_orbitalPPC_oneshot".ToLower() ||
                    vme.Id.ToLower() == "envMatStct_darkMetal_generic".ToLower() ||
                    vme.Id.ToLower() == "vfxPrfPrtl_weatherCamRain".ToLower()) {
                    Mod.Log.Info($"Target material:{vme.Id} comes from bundle:{vme.AssetBundleName}");
                }
            }

            GameObject go = null;

            //go = cgs.DataManager.PooledInstantiate("vfxPrfPrtl_orbitalPPC_oneshot", BattleTechResourceType.Prefab, null, null, null);
            //if (go == null) { Mod.Log.Info("FAILED TO LOAD MATERIAL: vfxPrfPrtl_orbitalPPC_oneshot"); }

            go = cgs.DataManager.PooledInstantiate("vfxPrfPrtl_weaponPPCImpact_crit", BattleTechResourceType.Prefab, null, null, null);
            if (go == null) { Mod.Log.Info("FAILED TO LOAD MATERIAL: vfxPrfPrtl_weaponPPCImpact_crit"); }

            go = cgs.DataManager.PooledInstantiate("envMatStct_darkMetal_generic", BattleTechResourceType.Prefab, null, null, null);
            if (go == null) { Mod.Log.Info("FAILED TO LOAD MATERIAL: envMatStct_darkMetal_generic"); }

            go = cgs.DataManager.PooledInstantiate("vfxPrfPrtl_weatherCamRain", BattleTechResourceType.Prefab, null, null, null);
            if (go == null) { Mod.Log.Info("FAILED TO LOAD MATERIAL: vfxPrfPrtl_weatherCamRain"); }

            go = cgs.DataManager.PooledInstantiate("vfxTxrPrtl_rainDot_alpha", BattleTechResourceType.Prefab, null, null, null);
            if (go == null) { Mod.Log.Info("FAILED TO LOAD MATERIAL: vfxTxrPrtl_rainDot_alpha"); }

            // Comes from 
            //   assets/vfx/prefabs/environental/firepower/vfxprfprtl_orbitalppc_oneshot.prefab
            //   assets/vfx/prefabs/weapon/ppc/vfxprfprtl_weaponppcimpact_crit.prefab
            MaterialSensorStealthBubble = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxMatPrtl_shockwaveBlack_alpha");
            Mod.Log.Info($" == shockwaveBlack_alpha loaded? {MaterialSensorStealthBubble != null}");

            MaterialSensorStealthCapsule = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "envMatStct_darkMetal_generic");
            Mod.Log.Info($" == envMatStct_darkMetal_generic loaded? {MaterialSensorStealthCapsule != null}");

            // Comes from 
            //   assets/vfx/prefabs/weather/vfxprfprtl_weathercamrain.prefab
            //   assets/vfx/prefabs/weather/vfxprfprtl_weathercamrain_junglestorm.prefab
            //   assets/vfx/prefabs/weather/vfxprfprtl_weathercamrain_storm.prefab
            MaterialECMBubble = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxTxrPrtl_rainDot_alpha");
            Mod.Log.Info($" == vfxTxrPrtl_rainDot_alpha loaded? {MaterialECMBubble != null}");

            // vfxMatPrtl_zapArcFlip2_blue_alpha

            Mod.Log.Debug("== MATERIALS INITIALIZED ==");
        }

        public static void EnableECMCarrierVfx(AbstractActor actor, EffectData effectData) {

            if (!State.TurnDirectorStarted) { return; }

            if (!actor.StatCollection.ContainsStatistic(ModStats.ECMVFXEnabled)) {
                Mod.Log.Debug(" ENABLING ECM LOOP");
                
                // Calculate the range factor
                float vfxScaleFactor = effectData.targetingData.range / 100f;
                Mod.Log.Debug($" VFX scaling factor {vfxScaleFactor}");

                // Bubble
                ParticleSystem psECMLoop = actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform,
                    Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
                psECMLoop.Stop(true);
                ParticleSystem.MainModule main = psECMLoop.main;
                main.startColor = Color.red;

                foreach (Transform child in psECMLoop.transform) {
                    if (child.gameObject.name.StartsWith("sphere")) {
                        child.gameObject.transform.localScale = new Vector3(vfxScaleFactor, vfxScaleFactor, vfxScaleFactor);
                    } else {
                        child.gameObject.SetActive(false);
                    }
                }
                psECMLoop.Play(true);

                // AoE loop
                ParticleSystem psECMCarrier = actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform,
                    Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
                psECMCarrier.transform.localScale = new Vector3(vfxScaleFactor, vfxScaleFactor, vfxScaleFactor);

                actor.StatCollection.AddStatistic(ModStats.ECMVFXEnabled, true);
            } else {
                Mod.Log.Debug(" ECM LOOP ALREADY ENABLED, SKIPPING");
            }
        }

        public static void DisableECMCarrierVfx(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            if (actor.GameRep != null && actor.StatCollection.ContainsStatistic(ModStats.ECMVFXEnabled)) {
                Mod.Log.Debug("DISABLING ECM CARRIER EFFECT");

                actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMtargetRemove_burst", true, Vector3.zero, true, -1f);
                actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECM_loop");
                actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECM_opponent_loop");
                actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECMcarrierAura_loop");

                actor.StatCollection.RemoveStatistic(ModStats.ECMVFXEnabled);
            }
        }

        public static void EnableStealthVfx(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            if (!actor.StatCollection.ContainsStatistic(ModStats.StealthVFXEnabled)) {
                Mod.Log.Debug("ENABLING SENSOR STEALTH EFFECT");

                ParticleSystem ps = actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform,
                    Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
                ps.Stop(true);

                foreach (Transform child in ps.transform) {
                    if (child.gameObject.name == "sphere") {
                        Mod.Log.Debug($"  - Configuring sphere");

                        bool isVehicle = actor as Vehicle != null;
                        bool isTurret = actor as Turret != null;
                        if (actor.UnitType == UnitType.Mech) {
                            // problate ellipsoid
                            child.gameObject.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
                        } else if (actor.UnitType == UnitType.Vehicle) {
                            // oblong ellipsoid
                            child.gameObject.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
                        } else {
                            // Turrets and unknown get sphere
                            child.gameObject.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
                        }

                        ParticleSystemRenderer spherePSR = child.gameObject.transform.GetComponent<ParticleSystemRenderer>();
                        spherePSR.material = VfxHelper.MaterialSensorStealthBubble;
                    } else {
                        Mod.Log.Debug($"  - Disabling GO: {child.gameObject.name}");
                        child.gameObject.SetActive(false);
                    }
                }
                ps.Play(true);

                actor.StatCollection.AddStatistic(ModStats.StealthVFXEnabled, true);
            }
        }

        public static void DisableSensorStealthEffect(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            if (actor.StatCollection.ContainsStatistic(ModStats.StealthVFXEnabled)) {
                Mod.Log.Debug("DISABLING SENSOR STEALTH EFFECT");

                actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECM_loop");

                actor.StatCollection.RemoveStatistic(ModStats.StealthVFXEnabled);
            }
        }

        public static void EnableMimeticEffect(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            if (!actor.StatCollection.ContainsStatistic(ModStats.MimeticVFXEnabled)) {
                Mod.Log.Debug("ENABLING MIMETIC EFFECT");

                // Disabled due to bfix removing the ghost effect
                // TODO: FIX!
                //PilotableActorRepresentation par = actor.GameRep as PilotableActorRepresentation;
                //par.BlipObjectGhostStrong.SetActive(false);
                //par.BlipObjectGhostWeak.SetActive(true);

                actor.StatCollection.AddStatistic(ModStats.MimeticVFXEnabled, true);
            }
        }

        public static void DisableMimeticEffect(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            if (actor.StatCollection.ContainsStatistic(ModStats.MimeticVFXEnabled)) {
                Mod.Log.Debug("DISABLING MIMETIC EFFECT");

                // Disabled due to bfix removing the ghost effect
                // TODO: FIX!
                //PilotableActorRepresentation par = actor.GameRep as PilotableActorRepresentation;
                //par.BlipObjectGhostStrong.SetActive(false);
                //par.BlipObjectGhostWeak.SetActive(false);

                actor.StatCollection.RemoveStatistic(ModStats.MimeticVFXEnabled);
            }
        }

        public static void CalculateMimeticPips(CombatHUDStealthBarPips stealthDisplay, AbstractActor actor, Vector3 previewPos) {
            float distanceMoved = Vector3.Distance(previewPos, actor.CurrentPosition);
            CalculateMimeticPips(stealthDisplay, actor, distanceMoved);
        }

        public static void CalculateMimeticPips(CombatHUDStealthBarPips stealthDisplay, AbstractActor actor) {
            float distanceMoved = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            CalculateMimeticPips(stealthDisplay, actor, distanceMoved);

        }

        public static void CalculateMimeticPips(CombatHUDStealthBarPips stealthDisplay, AbstractActor actor, float distanceMoved) {
            EWState actorState = new EWState(actor);
            Mod.Log.Debug($"Calculating mimeticPips for Actor: {CombatantUtils.Label(actor)}");

            int stepsMoved = (int)Math.Ceiling(distanceMoved / 30f);
            Mod.Log.Debug($"  stepsMoved: {stepsMoved} = distanceMoved: {distanceMoved} / 30");

            // Update # of pips
            int maxPips = actorState.MaxMimeticPips();
            int currentPips = actorState.CurrentMimeticPips();
            stealthDisplay.ShowNewActorStealth(currentPips, maxPips);

            // Change colors to reflect maxmimums
            Traverse pipsT = Traverse.Create(stealthDisplay).Property("Pips");
            List<Graphic> pips = pipsT.GetValue<List<Graphic>>();
            for (int i = 0; i < pips.Count; i++) {
                Graphic g = pips[i];
                if (g.isActiveAndEnabled) {
                    Color pipColor = Color.grey;
                    UIHelpers.SetImageColor(g, pipColor);
                }
            }

            // Update number of pips
            //int maxSensorStealthPips = actorState.MaxSensorStealthPips();
            //int maxVisionStealthPips = actorState.MaxMimeticPips();
            //int maxPips = Math.Max(maxSensorStealthPips, maxVisionStealthPips);

            //int sensorStealthPips = actorState.CurrentSensorStealthPips();
            //int visionStealthPips = actorState.CurrentMimeticPips();
            //int currPips = Math.Max(sensorStealthPips, visionStealthPips);

            //stealthDisplay.ShowNewActorStealth(currPips, maxPips);

            //// Change colors to reflect maxmimums
            //Traverse pipsT = Traverse.Create(stealthDisplay).Property("Pips");
            //List<Graphic> pips = pipsT.GetValue<List<Graphic>>();
            //for (int i = 0; i < pips.Count; i++) {
            //    Graphic g = pips[i];
            //    if (g.isActiveAndEnabled) {
            //        Color pipColor = GetPipColor(i, sensorStealthPips, visionStealthPips);
            //        UIHelpers.SetImageColor(g, pipColor);
            //    }
            //}
        }

        private static Color GetPipColor(int idx, int sensorPips, int visionPips) {
            if (idx <= sensorPips && idx <= visionPips) {
                return Color.green;
            } else if (idx <= sensorPips && idx > visionPips) {
                return Color.blue;
            } else if (idx <= visionPips && idx > sensorPips) {
                return Color.red;
            } else {
                return Color.gray;
            }
        }

    }
}
