using BattleTech;
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
        

        public static void Initialize() {
            Mod.Log.Debug("== INITIALIZING MATERIALS ==");
            MaterialSensorStealthBubble = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxMatPrtl_shockwaveBlack_alpha");
            MaterialSensorStealthCapsule = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "envMatStct_darkMetal_generic");
            MaterialECMBubble = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxTxrPrtl_rainDot_alpha");
            Mod.Log.Debug("== MATERIALS INITIALIZED ==");
        }

        public static void EnableECMCarrierEffect(AbstractActor actor, EffectData effectData) {
            Mod.Log.Debug(" ADDING ECM CARRIER LOOP");

            // Calculate the range factor
            float vfxScaleFactor = effectData.targetingData.range / 100f;
            Mod.Log.Debug($" VFX scaling factor {vfxScaleFactor}");

            // Bubble
            ParticleSystem psECMLoop = actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform, 
                Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
            psECMLoop.Stop(true);

            foreach (Transform child in psECMLoop.transform) {
                if (child.gameObject.name == "sphere") {
                    //Mod.Log.Debug($"  - Found sphere");
                    child.gameObject.transform.localScale = new Vector3(vfxScaleFactor, vfxScaleFactor, vfxScaleFactor);
                    //ParticleSystemRenderer spherePSR = child.gameObject.transform.GetComponent<ParticleSystemRenderer>();

                    //Shader alphaShader = Shader.Find("BattleTech/VFX/Alpha");
                    //Mod.Log.Debug($"  - AlphaShader found: {alphaShader != null}");

                    //DataManager dm = UnityGameInstance.BattleTechGame.DataManager;
                    //Texture2D rainDropMat = (Texture2D)dm.Get(BattleTechResourceType.Texture2D, "vfxTxrPrtl_rainDot_alpha");
                    //Mod.Log.Debug($"  - Texture found: {rainDropMat != null}");

                    //Material tmpMat = new Material(alphaShader);
                    //tmpMat.mainTexture = rainDropMat;
                    //tmpMat.color = Color.clear;

                    //var mat = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "vfxMatPrtl_rainDrop_noFoW_alpha");
                    //Mod.Log.Debug($"  - STUPID SEARCH Material found: {mat != null}");
                    //var mat2 = new Material(mat);
                    //Mod.Log.Debug($"  - MAT Shader is: {mat2.shader.name}");
                    //Mod.Log.Debug($"  - MAT Texture is: {mat2.mainTexture.name}");
                    //Mod.Log.Debug($"  - MAT Color is: {mat2.color}");

                    /*
                        2019-06-08 04:04:12.752 -   - MAT Shader is: BattleTech/VFX/Alphawa
                        2019-06-08 04:04:12.752 -   - MAT Texture is: vfxTxrPrtl_rainDot_alpha
                        2019-06-08 04:04:12.752 -   - MAT Color is: RGBA(0.000, 0.000, 0.000, 0.000)
                        2019-06-08 04:04:12.752 -   - Material NEW shader name: BattleTech/VFX/Alpha
                    */

                    //spherePSR.material = mat2;
                    //Mod.Log.Debug($"  - Material NEW shader name: {spherePSR.material.shader.name}");
                } else {
                    Mod.Log.Debug($"  - Disabling GO: {child.gameObject.name}");
                    child.gameObject.SetActive(false);
                }
            }
            psECMLoop.Play(true);

            // AoE loop
            ParticleSystem psECMCarrier = actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform, 
                Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
            psECMCarrier.transform.localScale = new Vector3(vfxScaleFactor, vfxScaleFactor, vfxScaleFactor);

            //WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_ecm_start, __instance.GameRep.audioObject, null, null);

            Mod.Log.Debug(" DONE ECM CARRIER LOOP");
        }

        public static void DisableECMCarrierEffect(AbstractActor actor) {
            Mod.Log.Debug("DISABLING ECM CARRIER EFFECT");
            if (actor.GameRep != null) {
                actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMtargetRemove_burst", true, Vector3.zero, true, -1f);
                //WwiseManager.PostEvent<AudioEventList_ecm>(AudioEventList_ecm.ecm_exit, __instance.GameRep.audioObject, null, null);
                actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECM_loop");
                actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECM_opponent_loop");
                actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECMcarrierAura_loop");
                //WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_ecm_stop, __instance.GameRep.audioObject, null, null);
            }
        }

        public static void EnableSensorStealthEffect(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            Mod.Log.Debug("ENABLING SENSOR STEALTH EFFECT");
            ParticleSystem ps = actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform, 
                Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
            ps.Stop(true);

            foreach (Transform child in ps.transform) {
                if (child.gameObject.name == "sphere") {
                    Mod.Log.Debug($"  - Configuring sphere");
                    child.gameObject.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
                    ParticleSystemRenderer spherePSR = child.gameObject.transform.GetComponent<ParticleSystemRenderer>();
                    spherePSR.material = VfxHelper.MaterialSensorStealthBubble;
                } else {
                    Mod.Log.Debug($"  - Disabling GO: {child.gameObject.name}");
                    child.gameObject.SetActive(false);
                }
            }
            ps.Play(true);

        }

        public static void DisableSensorStealthEffect(AbstractActor actor) {
            Mod.Log.Debug("DISABLING SENSOR STEALTH EFFECT");
            actor.GameRep.StopManualPersistentVFX("vfxPrfPrtl_ECM_loop");
        }

        public static void EnableVisionStealthEffect(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            Mod.Log.Debug("ENABLING VISION STEALTH EFFECT");
            PilotableActorRepresentation par = actor.GameRep as PilotableActorRepresentation;
            par.BlipObjectGhostStrong.SetActive(false);
            par.BlipObjectGhostWeak.SetActive(true);
        }

        public static void DisableVisionStealthEffect(AbstractActor actor) {
            Mod.Log.Debug("DISABLING VISION STEALTH EFFECT");
            PilotableActorRepresentation par = actor.GameRep as PilotableActorRepresentation;
            par.BlipObjectGhostStrong.SetActive(false);
            par.BlipObjectGhostWeak.SetActive(false);
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
