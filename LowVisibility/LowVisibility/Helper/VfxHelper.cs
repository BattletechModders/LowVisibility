using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.Data;
using BattleTech.Rendering;
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

        public const string ECMBubbleBaseVFX = "vfxPrfPrtl_ECM_loop";
        public const string ECMBubbleOpforBaseVFX = "vfxPrfPrtl_ECM_opponent_loop";
        public const string ECMBubbleRemovedBaseBFX = "vfxPrfPrtl_ECMtargetRemove_burst";
        public const string ECMCarrierBaseVFX = "vfxPrfPrtl_ECMcarrierAura_loop";

        public const string ECMBubbleVfxId = "lv_ecm_bubble_vfx";
        public const string ECMCarrierVfxId = "lv_ecm_carrier_vfx";
        public const string StealthEffectVfxId = "lv_stealth_vfx";
        public const string MimeticEffectVfxId = "lv_mimetic_vfx";

        public static Material StealthBubbleMaterial;
        public static Material DistortionMaterial;
        
        public static void Initialize(CombatGameState cgs) {
            Mod.Log.Debug("== INITIALIZING MATERIALS ==");

            Traverse abmT = Traverse.Create(cgs.DataManager).Property("AssetBundleManager");
            AssetBundleManager abm = abmT.GetValue<AssetBundleManager>();
            
            GameObject ppcImpactGO = abm.GetAssetFromBundle<GameObject>("vfxPrfPrtl_weaponPPCImpact_crit", "vfx");

            StealthBubbleMaterial = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxMatPrtl_shockwaveBlack_alpha");
            Mod.Log.Info($" == shockwaveBlack_alpha loaded? {StealthBubbleMaterial != null}");

            DistortionMaterial = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxMatPrtl_ECMdistortionStrong");
            Mod.Log.Info($" == vfxMatPrtl_ECMdistortionStrong loaded? {DistortionMaterial != null}");

            // vfxMatPrtl_distortion_distort_left or vfxMatPrtl_distortion_distort_right
            // vfxMatPrtl_ECMdistortionWeak or vfxMatPrtl_ECMdistortionStrong
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
                    Vector3.zero, ECMBubbleBaseVFX, true, Vector3.zero, false, -1f);
                psECMLoop.Stop(true);
                ParticleSystem.MainModule main = psECMLoop.main;

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
                    Vector3.zero, ECMCarrierBaseVFX, true, Vector3.zero, false, -1f);
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

                actor.GameRep.PlayVFXAt(actor.GameRep.thisTransform, Vector3.zero, ECMBubbleRemovedBaseBFX, true, Vector3.zero, true, -1f);
                actor.GameRep.StopManualPersistentVFX(ECMBubbleBaseVFX);
                actor.GameRep.StopManualPersistentVFX(ECMBubbleOpforBaseVFX);
                actor.GameRep.StopManualPersistentVFX(ECMCarrierBaseVFX);

                actor.StatCollection.RemoveStatistic(ModStats.ECMVFXEnabled);
            }
        }

        public static void EnableStealthVfx(AbstractActor actor) {

            if (!State.TurnDirectorStarted) { return; }

            if (!actor.StatCollection.ContainsStatistic(ModStats.StealthVFXEnabled)) {
                Mod.Log.Debug("ENABLING SENSOR STEALTH EFFECT");

                ParticleSystem ps = PlayVFXAt(actor.GameRep, actor.GameRep.thisTransform, Vector3.zero, ECMBubbleBaseVFX, StealthEffectVfxId, true, Vector3.zero, false, -1f); ;
                ps.Stop(true);
                Mod.Log.Debug($"Simulation speed is: {ps.main.simulationSpeed}");

                foreach (Transform child in ps.transform) {
                    if (child.gameObject.name == "sphere") {
                        Mod.Log.Debug($"  - Configuring sphere");

                        if (actor.UnitType == UnitType.Mech) {
                            // problate ellipsoid
                            child.gameObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                        } else if (actor.UnitType == UnitType.Vehicle) {
                            // oblong ellipsoid
                            child.gameObject.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
                        } else {
                            // Turrets and unknown get sphere
                            child.gameObject.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
                        }

                        // Center the sphere
                        if (actor.GameRep is MechRepresentation mr) {
                            Mod.Log.Debug($"Parent mech y positions: head: {mr.vfxHeadTransform.position.y} / " +
                                $"torso: {mr.vfxCenterTorsoTransform.position.y} / " +
                                $"leg: {mr.vfxLeftLegTransform.position.y}");
                            float headToTorso = mr.vfxHeadTransform.position.y - mr.vfxCenterTorsoTransform.position.y;
                            float torsoToLeg = mr.vfxCenterTorsoTransform.position.y - mr.vfxLeftLegTransform.position.y;
                            Mod.Log.Debug($"Parent mech headToTorso:{headToTorso} / torsoToLeg:{torsoToLeg}");

                            child.gameObject.transform.position = mr.vfxCenterTorsoTransform.position;
                            child.gameObject.transform.localPosition = new Vector3(0f, headToTorso * 2, 2f);
                            Mod.Log.Debug($"Centering sphere on mech torso at position: {mr.TorsoAttach.position}");
                        } else if (actor.GameRep is VehicleRepresentation vr) {
                            child.gameObject.transform.position = vr.transform.position;
                            Mod.Log.Debug($"Centering sphere on vehicle body at position: {vr.BodyAttach.position}");
                        } else if (actor.GameRep is TurretRepresentation tr) {
                            child.gameObject.transform.position = tr.transform.position;
                            Mod.Log.Debug($"Centering sphere on turret body at position: {tr.BodyAttach.position}");
                        }

                        ParticleSystemRenderer spherePSR = child.gameObject.transform.GetComponent<ParticleSystemRenderer>();
                        spherePSR.material = VfxHelper.StealthBubbleMaterial;
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

                actor.GameRep.StopManualPersistentVFX(StealthEffectVfxId);

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

        public static ParticleSystem PlayVFXAt(GameRepresentation gameRep, Transform parentTransform, Vector3 offset, string vfxName, string effectName, 
            bool attached, Vector3 lookAtPos, bool oneShot, float duration) {

            if (string.IsNullOrEmpty(vfxName)) {
                return null;
            }

            GameObject gameObject = gameRep.parentCombatant.Combat.DataManager.PooledInstantiate(vfxName, BattleTechResourceType.Prefab, null, null, null);
            if (gameObject == null) {
                GameRepresentation.initLogger.LogError("Error instantiating VFX " + vfxName, gameRep);
                return null;
            }
            ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
            component.Stop(true);
            component.Clear(true);
            Transform transform = gameObject.transform;
            transform.SetParent(null);

            BTWindZone componentInChildren = gameObject.GetComponentInChildren<BTWindZone>(true);
            if (componentInChildren != null && componentInChildren.enabled) {
                componentInChildren.ResetZero();
            }

            BTLightAnimator componentInChildren2 = gameObject.GetComponentInChildren<BTLightAnimator>(true);
            if (attached) {
                transform.SetParent(parentTransform, false);
                transform.localPosition = offset;
            } else {
                transform.localPosition = Vector3.zero;
                if (parentTransform != null) {
                    transform.position = parentTransform.position;
                }
                transform.position += offset;
            }

            if (lookAtPos != Vector3.zero) {
                transform.LookAt(lookAtPos);
            } else {
                transform.localRotation = Quaternion.identity;
            }
            transform.localScale = Vector3.one;

            if (oneShot) {
                AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
                if (autoPoolObject == null) {
                    autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
                }
                if (duration > 0f) {
                    autoPoolObject.Init(gameRep.parentCombatant.Combat.DataManager, vfxName, duration);
                } else {
                    autoPoolObject.Init(gameRep.parentCombatant.Combat.DataManager, vfxName, component);
                }
            } else {
                List<ParticleSystem> list = null;
                if (gameRep.persistentVFXParticles.TryGetValue(effectName, out list)) {
                    list.Add(component);
                    gameRep.persistentVFXParticles[effectName] = list;
                } else {
                    list = new List<ParticleSystem>();
                    list.Add(component);
                    gameRep.persistentVFXParticles[effectName] = list;
                }
            }

            BTCustomRenderer.SetVFXMultiplier(component);
            component.Play(true);
            if (componentInChildren != null) {
                componentInChildren.PlayAnimCurve();
            }
            if (componentInChildren2 != null) {
                componentInChildren2.PlayAnimation();
            }

            return component;
        }

    }
}
