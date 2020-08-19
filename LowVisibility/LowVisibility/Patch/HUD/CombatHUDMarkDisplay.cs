using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using SVGImporter;
using System;
using UnityEngine;
using UnityEngine.UI;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch.HUD
{

    // CombatHUDMarkDisplay displays any effect that createsa 

    static class CombatHUDMarkDisplayConsts
    {
        public const string SensorsMarkGOId = "lv_sensors_mark";
        public const string VisualsMarkGOId = "lv_visuals_mark";
        public const string TaggedMarkGOid = "lv_tagged_mark";
        public const string NarcedMarkGOId = "lv_narced_mark";
        public const string StealthMarkGOId = "lv_stealth_mark";
        public const string MimeticMarkGOId = "lv_mimetic_mark";
        public const string ECMShieldedMarkGOId = "lv_ecm_shielded_mark";
        public const string ActiveProbePingedMarkGOId = "lv_active_probe_pinged_mark";
    }

    [HarmonyPatch(typeof(CombatHUDMarkDisplay), "RefreshMark")]
    static class CombatHUDMarkDisplay_RefreshMark
    {
        static void Postfix(CombatHUDMarkDisplay __instance)
        {
            __instance.MarkTweens.SetState(ButtonState.Enabled, false);
        }
    }

    [HarmonyPatch(typeof(CombatHUDMarkDisplay), "RefreshInfo")]
    static class CombatHUDMarkDisplay_RefreshInfo
    {
        static void Prefix(CombatHUDMarkDisplay __instance)
        {
            Mod.Log.Trace?.Write("CHUDMD:RI - entered.");
            Mod.Log.Debug?.Write($"  instance is null: {__instance == null}  displayedActor: {CombatantUtils.Label(__instance?.DisplayedActor)}  " +
                $"lastPlayerActivated: {CombatantUtils.Label(ModState.LastPlayerActorActivated)}.");

            if (__instance != null && __instance.DisplayedActor != null && ModState.LastPlayerActorActivated != null)
            {
                // Cache these
                AbstractActor target = __instance.DisplayedActor;
                EWState targetState = new EWState(target);
                bool isPlayer = target != null && target.team != null && target.team.IsLocalPlayer;

                AbstractActor attacker = ModState.LastPlayerActorActivated;
                EWState attackerState = new EWState(attacker);

                // Sensor lock should only be enabled when the target is marked
                if (target.IsMarked) { __instance.gameObject.SetActive(true); }
                else { __instance.gameObject.SetActive(false); }

                // Make sure to set the layout group to handle the new icons
                VerticalLayoutGroup vlg = __instance.gameObject.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 6f;

                // Initialize or fetch all the gameObjects we create
                MarkGOContainer container;
                if (!ModState.MarkContainerRefs.ContainsKey(__instance)) container = InitializeContainer(__instance);
                else container = ModState.MarkContainerRefs[__instance];

                Mod.Log.Debug?.Write($"UPDATING COMBATHUDMARKDISPLAY FOR ACTOR: {CombatantUtils.Label(__instance.DisplayedActor)}");
                UpdateSensorAndVisualsIcons(container, __instance.DisplayedActor, ModState.LastPlayerActorActivated, isPlayer);

                // Tagged State
                if (targetState.IsTagged(attackerState)) UpdateIcon(container.TaggedMark, true, isPlayer);
                else UpdateIcon(container.TaggedMark, false, isPlayer);

                // Narced State
                if (targetState.IsNarced(attackerState)) UpdateIcon(container.NarcedMark, true, isPlayer);
                else UpdateIcon(container.NarcedMark, false, isPlayer);

                // ActiveProbePinged state
                if (targetState.PingedByProbeMod() != 0) UpdateIcon(container.ActiveProbePingedMark, true, isPlayer);
                else UpdateIcon(container.ActiveProbePingedMark, false, isPlayer);

                // Invert the isPlayer flag below here, since these effects are harmful when they are on opfor, and beneficial on the player
                // Stealth state
                if (targetState.HasStealth()) UpdateIcon(container.StealthMark, true, !isPlayer);
                else UpdateIcon(container.StealthMark, false, !isPlayer);

                // Mimetic State
                if (targetState.HasStealth()) UpdateIcon(container.MimeticMark, true, !isPlayer);
                else UpdateIcon(container.MimeticMark, false, !isPlayer);

                // ECMShielded State
                if (targetState.GetRawECMShield() != 0) UpdateIcon(container.ECMShieldedMark, true, !isPlayer);
                else UpdateIcon(container.ECMShieldedMark, false, !isPlayer);
                Mod.Log.Debug?.Write($"  -- DONE UPDATING COMBATHUDMARKDISPLAY FOR ACTOR: {CombatantUtils.Label(__instance.DisplayedActor)}");

                // DEBUG MODE
                //UpdateIcon(container.TaggedMark, true, isPlayer);
                //UpdateIcon(container.NarcedMark, true, isPlayer);
                //UpdateIcon(container.ActiveProbePingedMark, true, isPlayer);
                //UpdateIcon(container.StealthMark, true, !isPlayer);
                //UpdateIcon(container.MimeticMark, true, !isPlayer);
                //UpdateIcon(container.ECMShieldedMark, true, !isPlayer);
            }
        }

        static void UpdateIcon(GameObject icon, bool enable, bool isPlayerPositive)
        {
            if (enable)
            {
                SVGImage image = icon.GetComponent<SVGImage>();
                image.color = isPlayerPositive ? Mod.Config.Icons.PlayerPositiveMarkColor : Mod.Config.Icons.PlayerNegativeMarkColor;
            }

            icon.SetActive(enable);
        }

        static void UpdateSensorAndVisualsIcons(MarkGOContainer container, AbstractActor displayedActor, AbstractActor lastActivated, bool isPlayer)
        {
            // Sensors and Visuals are only shown for non-local players            
            if (!isPlayer)
            {
                // Check sensors
                bool hasSensorLock = SensorLockHelper.CalculateSharedLock(displayedActor, lastActivated) > SensorScanType.NoInfo;
                container.SensorsMark.SetActive(true);
                SVGImage sensorsImage = container.SensorsMark.GetComponent<SVGImage>();
                if (hasSensorLock)
                {
                    Mod.Log.Debug?.Write($" - Can sensors detect target, setting icon to green.");
                    sensorsImage.color = Color.green;
                }
                else
                {
                    Mod.Log.Debug?.Write($" - Can not sensors detect target, setting icon to red.");
                    sensorsImage.color = Color.red;
                }

                bool canSpotTarget = VisualLockHelper.CanSpotTarget(lastActivated, lastActivated.CurrentPosition, 
                    displayedActor, displayedActor.CurrentPosition, displayedActor.CurrentRotation, ModState.Combat.LOS);
                SVGImage visualsImage = container.VisualsMark.GetComponent<SVGImage>();
                container.VisualsMark.SetActive(true);
                if (canSpotTarget)
                {
                    Mod.Log.Debug?.Write($" - Can spot target, setting icon to green.");
                    visualsImage.color = Color.green;
                }
                else
                {
                    Mod.Log.Debug?.Write($" - Cannot spot target, setting icon to red.");
                    visualsImage.color = Color.red;

                }
            }
            else
            {
                container.SensorsMark.SetActive(false);
                container.VisualsMark.SetActive(false);
            }
        }

        static MarkGOContainer InitializeContainer(CombatHUDMarkDisplay markDisplay)
        {
            // We can't patch Init (probably compiled to a point we can't patch it) so create the objects here.
            Mod.Log.Debug?.Write($"CHUDMD:I invoked");
            GameObject sensorsMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetSensorsMark, CombatHUDMarkDisplayConsts.SensorsMarkGOId);
            GameObject visualsMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetVisualsMark, CombatHUDMarkDisplayConsts.VisualsMarkGOId);

            GameObject taggedMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetTaggedMark, CombatHUDMarkDisplayConsts.TaggedMarkGOid);
            GameObject narcedMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetNarcedMark, CombatHUDMarkDisplayConsts.NarcedMarkGOId);
            GameObject activeProbePingedMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetActiveProbePingedMark, CombatHUDMarkDisplayConsts.ActiveProbePingedMarkGOId);

            GameObject stealthMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetStealthMark, CombatHUDMarkDisplayConsts.StealthMarkGOId);
            GameObject mimeticMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetMimeticMark, CombatHUDMarkDisplayConsts.MimeticMarkGOId);
            GameObject ecmShieldedMark = CreateMark(markDisplay.transform.parent.gameObject, Mod.Config.Icons.TargetECMShieldedMark, CombatHUDMarkDisplayConsts.ECMShieldedMarkGOId);
            
            MarkGOContainer container = new MarkGOContainer
            {
                SensorsMark = sensorsMark,
                VisualsMark = visualsMark,
                TaggedMark = taggedMark,
                NarcedMark = narcedMark,
                StealthMark = stealthMark,
                MimeticMark = mimeticMark,
                ECMShieldedMark = ecmShieldedMark,
                ActiveProbePingedMark = activeProbePingedMark
            };

            ModState.MarkContainerRefs[markDisplay] = container;
            Mod.Log.Debug?.Write($"Created reference from instance {markDisplay} to container: {container}");

            // Set the parent's (Marks GO) scale to 300y
            GameObject marksGO = markDisplay.transform.parent.gameObject;

            RectTransform marksRT = marksGO.GetComponent<RectTransform>();
            Vector2 newSizeDelta = marksRT.sizeDelta;
            newSizeDelta.y = 300f;
            marksRT.sizeDelta = newSizeDelta;

            Vector3 newLocalPos = marksGO.transform.localPosition;
            newLocalPos.y += 100f;
            marksGO.transform.localPosition = newLocalPos;

            return container;
        }

        static GameObject CreateMark(GameObject parent, string iconId, string objectId)
        {
            Mod.Log.Debug?.Write($"Creating mark for iconId: {iconId}");
            try
            {
                SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(iconId, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"Icon: {iconId} was not loaded! Check the manifest load");

                GameObject imageGO = new GameObject();
                imageGO.name = objectId;
                imageGO.transform.parent = parent.transform;
                imageGO.transform.localScale = new Vector3(0.7f, 1f, 1f);

                SVGImage image = imageGO.AddComponent<SVGImage>();
                if (image == null) Mod.Log.Warn?.Write("Failed to create image for icon, load will fail!");

                image.vectorGraphics = icon;
                image.color = Color.white;
                image.enabled = true;

                LayoutElement le = imageGO.AddComponent<LayoutElement>();
                le.preferredHeight = 32f;
                le.preferredWidth = 32f;

                RectTransform rectTransform = imageGO.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(50f, 0f);

                return imageGO;
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Write(e, $"Failed to create mark image: {iconId}");
                return null;
            }
        }
    }
}
