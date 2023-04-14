using BattleTech.Assetbundles;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using BattleTech.UI;
using CustAmmoCategories;
using FogOfWar;
using HBS;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper
{
    public static class VfxHelper
    {

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

        public static void Initialize(CombatGameState cgs)
        {
            Mod.Log.Debug?.Write("== INITIALIZING MATERIALS ==");

            AssetBundleManager abm = cgs.DataManager.AssetBundleManager;
            GameObject ppcImpactGO = abm.GetAssetFromBundle<GameObject>("vfxPrfPrtl_weaponPPCImpact_crit", "vfx");

            StealthBubbleMaterial = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxMatPrtl_shockwaveBlack_alpha");
            Mod.Log.Info?.Write($" == shockwaveBlack_alpha loaded? {StealthBubbleMaterial != null}");

            DistortionMaterial = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "vfxMatPrtl_ECMdistortionStrong");
            Mod.Log.Info?.Write($" == vfxMatPrtl_ECMdistortionStrong loaded? {DistortionMaterial != null}");

            // vfxMatPrtl_distortion_distort_left or vfxMatPrtl_distortion_distort_right
            // vfxMatPrtl_ECMdistortionWeak or vfxMatPrtl_ECMdistortionStrong
        }

        public static void EnableStealthVfx(AbstractActor actor)
        {

            if (!ModState.TurnDirectorStarted) { return; }

            if (!actor.StatCollection.ContainsStatistic(ModStats.StealthVFXEnabled))
            {
                Mod.Log.Debug?.Write("ENABLING SENSOR STEALTH EFFECT");

                ParticleSystem ps = PlayVFXAt(actor.GameRep, actor.GameRep.thisTransform, Vector3.zero, ECMBubbleBaseVFX, StealthEffectVfxId, true, Vector3.zero, false, -1f); ;
                ps.Stop(true);

                foreach (Transform child in ps.transform)
                {
                    if (child.gameObject.name == "sphere")
                    {
                        Mod.Log.Debug?.Write($"  - Configuring sphere");

                        if (actor is Mech)
                        {
                            if (actor.TrooperSquad())
                                // problate ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
                            else if (actor.NavalUnit())
                                // oblong ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.26f, 0.13f, 0.26f);
                            else if (actor.FakeVehicle())
                                // oblong ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.26f, 0.13f, 0.26f);
                            else
                                // problate ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
                        }
                        else if (actor is Vehicle)
                            // oblong ellipsoid
                            child.gameObject.transform.localScale = new Vector3(0.26f, 0.13f, 0.26f);
                        else
                            // Turrets and unknown get sphere
                            child.gameObject.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);

                        // Center the sphere
                        if (actor.GameRep is MechRepresentation mr)
                        {
                            Mod.Log.Debug?.Write($"Parent mech y positions: head: {mr.vfxHeadTransform.position.y} / " +
                                $"torso: {mr.vfxCenterTorsoTransform.position.y} / " +
                                $"leg: {mr.vfxLeftLegTransform.position.y}");
                            float headToTorso = mr.vfxHeadTransform.position.y - mr.vfxCenterTorsoTransform.position.y;
                            float torsoToLeg = mr.vfxCenterTorsoTransform.position.y - mr.vfxLeftLegTransform.position.y;
                            Mod.Log.Debug?.Write($"Parent mech headToTorso:{headToTorso} / torsoToLeg:{torsoToLeg}");

                            child.gameObject.transform.position = mr.vfxCenterTorsoTransform.position;
                            child.gameObject.transform.localPosition = new Vector3(0f, headToTorso * 2, 2f);
                            Mod.Log.Debug?.Write($"Centering sphere on mech torso at position: {mr.TorsoAttach.position}");
                        }
                        else if (actor.GameRep is VehicleRepresentation vr)
                        {
                            child.gameObject.transform.position = vr.BodyAttach.position;
                            child.gameObject.transform.localPosition = new Vector3(0f, 0f, 2f);
                            Mod.Log.Debug?.Write($"Centering sphere on vehicle body at position: {vr.BodyAttach.position}");
                        }
                        else if (actor.GameRep is TurretRepresentation tr)
                        {
                            child.gameObject.transform.position = tr.BodyAttach.position;
                            child.gameObject.transform.localPosition = new Vector3(0f, 0f, 2f);
                            Mod.Log.Debug?.Write($"Centering sphere on turret body at position: {tr.BodyAttach.position}");
                        }

                        ParticleSystemRenderer spherePSR = child.gameObject.transform.GetComponent<ParticleSystemRenderer>();
                        spherePSR.material = VfxHelper.StealthBubbleMaterial;
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                ps.Play(true);

                actor.StatCollection.AddStatistic(ModStats.StealthVFXEnabled, true);
            }
        }

        public static void DisableSensorStealthEffect(AbstractActor actor)
        {

            if (!ModState.TurnDirectorStarted) { return; }

            if (actor.StatCollection.ContainsStatistic(ModStats.StealthVFXEnabled))
            {
                Mod.Log.Debug?.Write("DISABLING SENSOR STEALTH EFFECT");

                actor.GameRep.StopManualPersistentVFX(StealthEffectVfxId);

                actor.StatCollection.RemoveStatistic(ModStats.StealthVFXEnabled);
            }
        }

        public static void EnableMimeticEffect(AbstractActor actor)
        {

            if (!ModState.TurnDirectorStarted) { return; }

            if (!actor.StatCollection.ContainsStatistic(ModStats.MimeticVFXEnabled))
            {
                Mod.Log.Debug?.Write("ENABLING MIMETIC EFFECT");

                ParticleSystem ps = PlayVFXAt(actor.GameRep, actor.GameRep.thisTransform, Vector3.zero, ECMBubbleBaseVFX, MimeticEffectVfxId, true, Vector3.zero, false, -1f); ;
                ps.Stop(true);

                foreach (Transform child in ps.transform)
                {
                    if (child.gameObject.name == "sphere rumble")
                    {
                        Mod.Log.Trace?.Write($"  - Configuring sphere rumble");


                        if (actor is Mech)
                        {
                            if (actor.TrooperSquad())
                                // problate ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                            else if (actor.NavalUnit())
                                // oblong ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
                            else if (actor.FakeVehicle())
                                // oblong ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
                            else
                                // problate ellipsoid
                                child.gameObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                        }
                        else if (actor is Vehicle)
                            // oblong ellipsoid
                            child.gameObject.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
                        else
                            // Turrets and unknown get sphere
                            child.gameObject.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);

                        // Try to manipulate the animation speed
                        ParticleSystem[] childPS = child.gameObject.GetComponentsInChildren<ParticleSystem>();
                        if (childPS != null && childPS.Length != 0)
                        {
                            foreach (ParticleSystem cPS in childPS)
                            {
                                var main = cPS.main;
                                main.duration = 4f;
                            }
                        }

                        // Center the sphere
                        if (actor.GameRep is MechRepresentation mr)
                        {
                            float headToTorso = mr.vfxHeadTransform.position.y - mr.vfxCenterTorsoTransform.position.y;
                            float torsoToLeg = mr.vfxCenterTorsoTransform.position.y - mr.vfxLeftLegTransform.position.y;
                            Mod.Log.Trace?.Write($"Parent mech headToTorso:{headToTorso} / torsoToLeg:{torsoToLeg}");

                            child.gameObject.transform.position = mr.vfxCenterTorsoTransform.position;
                            child.gameObject.transform.localPosition = new Vector3(0f, headToTorso * 2, 2f);
                            Mod.Log.Debug?.Write($"Centering sphere on mech torso at position: {mr.TorsoAttach.position}");
                        }
                        else if (actor.GameRep is VehicleRepresentation vr)
                        {
                            child.gameObject.transform.position = vr.BodyAttach.position;
                            child.gameObject.transform.localPosition = new Vector3(0f, 0f, 2f);
                            Mod.Log.Debug?.Write($"Centering sphere on vehicle body at position: {vr.BodyAttach.position}");
                        }
                        else if (actor.GameRep is TurretRepresentation tr)
                        {
                            child.gameObject.transform.position = tr.BodyAttach.position;
                            child.gameObject.transform.localPosition = new Vector3(0f, 0f, 2f);
                            Mod.Log.Debug?.Write($"Centering sphere on turret body at position: {tr.BodyAttach.position}");
                        }


                        ParticleSystemRenderer spherePSR = child.gameObject.transform.GetComponent<ParticleSystemRenderer>();
                        spherePSR.material = VfxHelper.DistortionMaterial;
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                ps.Play(true);

                if (Mod.Config.Toggles.MimeticUsesGhost)
                {
                    Mod.Log.Debug?.Write($"Enabling GhostWeak VFX on actor: {CombatantUtils.Label(actor)}");
                    PilotableActorRepresentation par = actor.GameRep as PilotableActorRepresentation;
                    par.BlipObjectGhostStrong.SetActive(false);
                    par.BlipObjectGhostWeak.SetActive(true);
                }

                actor.StatCollection.AddStatistic(ModStats.MimeticVFXEnabled, true);
            }
        }

        public static void DisableMimeticEffect(AbstractActor actor)
        {

            if (!ModState.TurnDirectorStarted) { return; }

            if (actor.StatCollection.ContainsStatistic(ModStats.MimeticVFXEnabled))
            {

                Mod.Log.Debug?.Write("DISABLING MIMETIC EFFECT");

                actor.GameRep.StopManualPersistentVFX(MimeticEffectVfxId);

                if (Mod.Config.Toggles.MimeticUsesGhost)
                {
                    Mod.Log.Debug?.Write($"Disabling GhostWeak VFX on actor: {CombatantUtils.Label(actor)}");
                    PilotableActorRepresentation par = actor.GameRep as PilotableActorRepresentation;
                    par.BlipObjectGhostStrong.SetActive(false);
                    par.BlipObjectGhostWeak.SetActive(false);
                }

                actor.StatCollection.RemoveStatistic(ModStats.MimeticVFXEnabled);
            }
        }

        public static void EnableNightVisionEffect(AbstractActor source)
        {
            // Skip if the green effect is disabled
            if (!Mod.Config.Toggles.ShowNightVision) { return; }

            ModState.IsNightVisionMode = true;

            MoodController mc = MoodController.Instance;

            PostProcessingBehaviour ppb = mc.unityPostProcess;

            // Enable grain and set the intensity
            GrainComponent gc = ppb.m_Grain;
            GrainModel.Settings gms = gc.model.settings;
            gms.intensity = 0.8f;
            gms.size = 1.0f;
            gc.model.settings = gms;

            BTSunlight sunlightBT = mc.sunlightBT;

            // Disable shadows from sunlight
            //BTSunlight.SunlightSettings sunlightS = sunlightBT.sunSettings;
            //sunlightS.castShadows = false;
            //sunlightBT.sunSettings = sunlightS;
            Light sunlight = sunlightBT.sunLight;
            sunlight.shadows = LightShadows.None;

            // Set the sunlight color
            Color lightVision = Color.green;
            lightVision.a = 0.8f;
            Shader.SetGlobalColor(Shader.PropertyToID("_BT_SunlightColor"), lightVision);

            // Disable opacity from the clouds
            Shader.SetGlobalFloat(Shader.PropertyToID("_BT_CloudOpacity"), 0f);

            // Make the sunlight point straight down
            Shader.SetGlobalVector(Shader.PropertyToID("_BT_SunlightDirection"), sunlightBT.transform.up);
        }

        public static void DisableNightVisionEffect()
        {
            // Skip if the green effect is disabled
            if (!Mod.Config.Toggles.ShowNightVision) { return; }

            ModState.IsNightVisionMode = false;

            MoodController mc = BattleTech.Rendering.Mood.MoodController.Instance;

            // Grain will disable automatically

            // Re-enable shadows
            BTSunlight sunlightBT = mc.sunlightBT;

            // Re-enable shadows from sunlight
            BTSunlight.SunlightSettings sunlightS = sunlightBT.sunSettings;
            //sunlightS.castShadows = false;
            //sunlightBT.sunSettings = sunlightS;
            Light sunlight = sunlightBT.sunLight;
            sunlight.shadows = (sunlightS.castShadows) ? LightShadows.None : LightShadows.Soft;

            // Reset the sunlight color
            Shader.SetGlobalColor(Shader.PropertyToID("_BT_SunlightColor"), mc.currentMood.sunlight.sunColor);

            // Re-enable opacity from the clouds
            Shader.SetGlobalFloat(Shader.PropertyToID("_BT_CloudOpacity"), mc.currentMood.sunlight.cloudOpacity);

            // Point sunlight forward
            Shader.SetGlobalVector(Shader.PropertyToID("_BT_SunlightDirection"), sunlightBT.transform.forward);
        }

        public static void RedrawFogOfWar(AbstractActor activeActor)
        {

            if (!Mod.Config.FogOfWar.RedrawFogOfWarOnActivation) return;

            FogOfWarSystem fowSystem = LazySingletonBehavior<FogOfWarView>.Instance.FowSystem;
            if (fowSystem == null)
            {
                Mod.Log.Error?.Write("FogOfWarSystem could not be found - this should never happen!");
                return;
            }

            Mod.Log.Debug?.Write($"Redrawing FOW for actor: {CombatantUtils.Label(activeActor)}");

            List<AbstractActor> viewers = fowSystem.viewers;
            viewers.Clear();

            // Reset FoW to being unseen
            fowSystem.WipeToValue(Mod.Config.FogOfWar.ShowTerrainThroughFogOfWar ?
                FogOfWarState.Surveyed : FogOfWarState.Unknown);

            // Add the actor as a viewer
            fowSystem.AddViewer(activeActor);

            // Check lancemates; if they have vision sharing add them as well
            foreach (string lanceGuid in activeActor?.lance?.unitGuids)
            {
                if (!lanceGuid.Equals(activeActor.GUID))
                {
                    ICombatant lanceMateC = activeActor.Combat.FindCombatantByGUID(lanceGuid);
                    if (lanceMateC is AbstractActor lanceActor)
                    {
                        EWState lanceState = new EWState(lanceActor);
                        if (lanceState.SharesVision())
                        {
                            fowSystem.AddViewer(lanceActor);
                        }
                    }
                }
            }

        }

        public static void CalculateMimeticPips(CombatHUDStealthBarPips stealthDisplay, AbstractActor actor, Vector3 previewPos)
        {
            float distanceMoved = Vector3.Distance(previewPos, actor.CurrentPosition);
            CalculateMimeticPips(stealthDisplay, actor, distanceMoved);
        }

        public static void CalculateMimeticPips(CombatHUDStealthBarPips stealthDisplay, AbstractActor actor)
        {
            float distanceMoved = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            CalculateMimeticPips(stealthDisplay, actor, distanceMoved);

        }

        public static void CalculateMimeticPips(CombatHUDStealthBarPips stealthDisplay, AbstractActor actor, float distanceMoved)
        {
            EWState actorState = new EWState(actor);
            Mod.Log.Trace?.Write($"Calculating mimeticPips for Actor: {CombatantUtils.Label(actor)}");

            int stepsMoved = (int)Math.Ceiling(distanceMoved / 30f);
            Mod.Log.Trace?.Write($"  stepsMoved: {stepsMoved} = distanceMoved: {distanceMoved} / 30");

            // Update # of pips
            int maxPips = actorState.MaxMimeticPips();
            int currentPips = actorState.CurrentMimeticPips(distanceMoved);
            stealthDisplay.ShowNewActorStealth(currentPips, maxPips);

            // Change colors to reflect maxmimums
            List<Graphic> pips = stealthDisplay.Pips;
            for (int i = 0; i < pips.Count; i++)
            {
                Graphic g = pips[i];
                if (g.isActiveAndEnabled)
                {
                    //Color pipColor = Color.white;
                    Color pipColor = new Color(50f, 206f, 230f);
                    UIHelpers.SetImageColor(g, pipColor);
                }
            }
        }

        public static ParticleSystem PlayVFXAt(GameRepresentation gameRep, Transform parentTransform, Vector3 offset, string vfxName, string effectName,
            bool attached, Vector3 lookAtPos, bool oneShot, float duration)
        {

            if (string.IsNullOrEmpty(vfxName))
            {
                return null;
            }

            GameObject gameObject = gameRep.parentCombatant.Combat.DataManager.PooledInstantiate(vfxName, BattleTechResourceType.Prefab, null, null, null);
            if (gameObject == null)
            {
                GameRepresentation.initLogger.LogError("Error instantiating VFX " + vfxName, gameRep);
                return null;
            }
            ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
            component.Stop(true);
            component.Clear(true);
            Transform transform = gameObject.transform;
            transform.SetParent(null);

            BTWindZone componentInChildren = gameObject.GetComponentInChildren<BTWindZone>(true);
            if (componentInChildren != null && componentInChildren.enabled)
            {
                componentInChildren.ResetZero();
            }

            BTLightAnimator componentInChildren2 = gameObject.GetComponentInChildren<BTLightAnimator>(true);
            if (attached)
            {
                transform.SetParent(parentTransform, false);
                transform.localPosition = offset;
            }
            else
            {
                transform.localPosition = Vector3.zero;
                if (parentTransform != null)
                {
                    transform.position = parentTransform.position;
                }
                transform.position += offset;
            }

            if (lookAtPos != Vector3.zero)
            {
                transform.LookAt(lookAtPos);
            }
            else
            {
                transform.localRotation = Quaternion.identity;
            }
            transform.localScale = Vector3.one;

            if (oneShot)
            {
                AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
                if (autoPoolObject == null)
                {
                    autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
                }
                if (duration > 0f)
                {
                    autoPoolObject.Init(gameRep.parentCombatant.Combat.DataManager, vfxName, duration);
                }
                else
                {
                    autoPoolObject.Init(gameRep.parentCombatant.Combat.DataManager, vfxName, component);
                }
            }
            else
            {
                List<ParticleSystem> list = null;
                if (gameRep.persistentVFXParticles.TryGetValue(effectName, out list))
                {
                    list.Add(component);
                    gameRep.persistentVFXParticles[effectName] = list;
                }
                else
                {
                    list = new List<ParticleSystem>();
                    list.Add(component);
                    gameRep.persistentVFXParticles[effectName] = list;
                }
            }

            BTCustomRenderer.SetVFXMultiplier(component);
            component.Play(true);
            if (componentInChildren != null)
            {
                componentInChildren.PlayAnimCurve();
            }
            if (componentInChildren2 != null)
            {
                componentInChildren2.PlayAnimation();
            }

            return component;
        }

    }
}
