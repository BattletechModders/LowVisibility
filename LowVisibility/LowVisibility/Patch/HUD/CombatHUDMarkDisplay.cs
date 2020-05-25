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
    }

    [HarmonyPatch(typeof(CombatHUDMarkDisplay), "Init")]
    static class CombatHUDMarkDisplay_Init
    {

       

        static void Postfix(CombatHUDMarkDisplay __instance)
        {

        }
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
            Mod.Log.Info("CHUDMD:RI - entered.");
            Mod.Log.Info($"  instance is null: {__instance == null}  displayedActor: {CombatantUtils.Label(__instance?.DisplayedActor)}  " +
                $"lastPlayerActivated: {CombatantUtils.Label(ModState.LastPlayerActorActivated)}.");

            if (__instance != null && __instance.DisplayedActor != null && ModState.LastPlayerActorActivated != null)
            {
                bool isEnemy = ModState.Combat.HostilityMatrix.IsLocalPlayerEnemy(__instance.DisplayedActor.team);
                if (isEnemy)
                {

                    MarkGOContainer container = null;
                    if (!ModState.MarkContainerRefs.ContainsKey(__instance))
                    {
                        // We can't patch Init (probably compiled to a point we can't patch it) so create the objects here.
                        Mod.Log.Debug($"CHUDMD:I invoked");
                        GameObject sensorsMark = CreateMark(__instance.transform.parent.gameObject, Mod.Config.Icons.TargetSensorsMark, CombatHUDMarkDisplayConsts.SensorsMarkGOId);
                        GameObject visualsMark = CreateMark(__instance.transform.parent.gameObject, Mod.Config.Icons.TargetVisualsMark, CombatHUDMarkDisplayConsts.VisualsMarkGOId);
                        container = new MarkGOContainer
                        {
                            SensorsMark = sensorsMark,
                            VisualsMark = visualsMark
                        };
                        ModState.MarkContainerRefs[__instance] = container;
                        Mod.Log.Debug($"Created reference from instance {__instance} to container: {container}");
                    }
                    else
                    {
                        container = ModState.MarkContainerRefs[__instance];
                    }


                    // Check that enemy is visible (blip or better)
                    VerticalLayoutGroup vlg = __instance.gameObject.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>();
                    vlg.spacing = 6f;

                    Mod.Log.Info($"UPDATING COMBATHUDMARKDISPLAY FOR ACTOR: {CombatantUtils.Label(__instance.DisplayedActor)}");
                    // Cache these
                    AbstractActor target = __instance.DisplayedActor;
                    AbstractActor attacker = ModState.LastPlayerActorActivated;

                    // Sensor lock should only be enabled when the target is marked
                    if (target.IsMarked) { __instance.gameObject.SetActive(true); }
                    else { __instance.gameObject.SetActive(false);  }

                        // Check sensors
                        bool hasSensorAttack = SensorLockHelper.CalculateSharedLock(target, attacker) > SensorScanType.NoInfo;
                    if (hasSensorAttack)
                    {
                        Mod.Log.Debug($" - Can sensors detect target, setting icon to green.");
                        SVGImage image = container.SensorsMark.GetComponent<SVGImage>();
                        image.color = Color.green;
                        container.SensorsMark.SetActive(true);
                    }
                    else
                    {
                        Mod.Log.Debug($" - Can not sensors detect target, setting icon to red.");
                        SVGImage image = container.SensorsMark.GetComponent<SVGImage>();
                        image.color = Color.red;
                        container.SensorsMark.SetActive(true);
                    }

                    bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);
                    if (canSpotTarget)
                    {
                        Mod.Log.Debug($" - Can spot target, setting icon to green.");
                        SVGImage image = container.VisualsMark.GetComponent<SVGImage>();
                        image.color = Color.green;
                        container.VisualsMark.SetActive(true);
                    }
                    else
                    {
                        Mod.Log.Debug($" - Cannot spot target, setting icon to red.");
                        SVGImage image = container.VisualsMark.GetComponent<SVGImage>();
                        image.color = Color.red;
                        container.VisualsMark.SetActive(true);
                    }

                }
            }
        }

        static GameObject CreateMark(GameObject parent, string iconId, string objectId)
        {
            Mod.Log.Debug($"Creating mark for iconId: {iconId}");
            try
            {
                SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(iconId, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn($"Icon: {iconId} was not loaded! Check the manifest load");

                GameObject imageGO = new GameObject();
                imageGO.name = objectId;
                imageGO.transform.parent = parent.transform;
                imageGO.transform.localScale = new Vector3(0.7f, 1f, 1f);

                SVGImage image = imageGO.AddComponent<SVGImage>();
                if (image == null) Mod.Log.Warn("Failed to create image for icon, load will fail!");

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
                Mod.Log.Error($"Failed to create mark image: {iconId}", e);
                return null;
            }
        }
    }
}
