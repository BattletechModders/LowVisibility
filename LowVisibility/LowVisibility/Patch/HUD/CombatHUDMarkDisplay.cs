using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch.HUD
{

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

                    // Check that enemy is visible (blip or better)
                    Vector3 newPos = __instance.gameObject.transform.position;

                    VerticalLayoutGroup vlg = __instance.gameObject.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>();
                    vlg.spacing = 4f;

                    Mod.Log.Info($"UPDATING COMBATHUDMARKDISPLAY FOR ACTOR: {CombatantUtils.Label(__instance.DisplayedActor)}");
                    // Cache these
                    AbstractActor target = __instance.DisplayedActor;
                    AbstractActor attacker = ModState.LastPlayerActorActivated;

                    SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.TargetVisualsMark, BattleTechResourceType.SVGAsset);
                    if (icon == null) Mod.Log.Error("FAILED TO LOAD ICON!");
                    GameObject imageGO = new GameObject();
                    imageGO.name = "lv_icon_foo";
                    imageGO.transform.parent = __instance.gameObject.transform.parent;
                    //newPos.x += 30f;
                    //imageGO.transform.position = newPos;
                    //imageGO.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                    imageGO.transform.localScale = new Vector3(0.35f, 1f, 1f);

                    SVGImage image = imageGO.AddComponent<SVGImage>();
                    if (image == null) Mod.Log.Error("FAILED TO CREATE IMAGE!");
                    image.vectorGraphics = icon;
                    image.color = Color.green;
                    image.enabled = true;
                    imageGO.SetActive(true);
                    Mod.Log.Info("Set objects active!");

                    LayoutElement le = imageGO.AddComponent<LayoutElement>();
                    le.preferredHeight = 16f;
                    le.preferredWidth = 16f;

                    RectTransform rectTransform = imageGO.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(50f, 0f);
                    Mod.Log.Info("Set SIZE DELTA");

                    //SVGAsset icon2 = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.TargetSensorsMark, BattleTechResourceType.SVGAsset);
                    //if (icon2 == null) Mod.Log.Error("FAILED TO LOAD ICON 2!");
                    //GameObject imageGO2 = new GameObject();
                    //imageGO2.name = "lv_icon_foo2";
                    //imageGO2.transform.parent = __instance.gameObject.transform.parent;
                    //newPos.x += 30f;
                    //imageGO.transform.position = newPos;
                    ////imageGO.transform.localScale = new Vector3(1f, 1f, 1f);

                    //SVGImage image2 = imageGO2.AddComponent<SVGImage>();
                    //if (image2 == null) Mod.Log.Error("FAILED TO CREATE IMAGE 2!");
                    //image2.vectorGraphics = icon;
                    //image2.color = Color.green;
                    //image.enabled = true;
                    //imageGO.SetActive(true);
                    //Mod.Log.Info("Set objects active 2!");

                    //RectTransform rectTransform2 = imageGO.GetComponent<RectTransform>();
                    //rectTransform2.sizeDelta = new Vector2(28f, 28f);
                    //Mod.Log.Info("Set SIZE DELTA");

                    //bool hasSensorAttack = SensorLockHelper.CalculateSharedLock(target, attacker) > SensorScanType.NoInfo;
                    //if (hasSensorAttack)
                    //{
                    //    SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.HasSensorsOnTarget, BattleTechResourceType.SVGAsset);
                    //    SVGImage image = __instance.gameObject.AddComponent<SVGImage>();
                    //    image.vectorGraphics = icon;

                    //    if (!__instance.MarkIcons.Contains(image))
                    //    {
                    //        Mod.Log.Debug("ADDING IMAGE FOR HAS_SENSORS");
                    //        __instance.MarkIcons.Add(image);
                    //    }
                    //}
                    //else
                    //{
                    //    SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.NoSensorsOnTarget, BattleTechResourceType.SVGAsset);
                    //    SVGImage image = new SVGImage();
                    //    image.transform.parent = __instance.transform.parent;
                    //    image.vectorGraphics = icon;

                    //    if (!__instance.MarkIcons.Contains(image))
                    //    {
                    //        Mod.Log.Debug("ADDING IMAGE FOR NO_SENSORS");
                    //        __instance.MarkIcons.Add(image);
                    //    }
                    //}

                    //bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);
                    //if (canSpotTarget)
                    //{
                    //    SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.HasVisualsOnTarget, BattleTechResourceType.SVGAsset);
                    //    SVGImage image = new SVGImage();
                    //    image.transform.parent = __instance.transform.parent;
                    //    image.vectorGraphics = icon;

                    //    if (!__instance.MarkIcons.Contains(image))
                    //    {
                    //        Mod.Log.Debug("ADDING IMAGE FOR HAS VISUALS");
                    //        __instance.MarkIcons.Add(image);
                    //    }
                    //}
                    //else
                    //{
                    //    SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.NoVisualsOnTarget, BattleTechResourceType.SVGAsset);
                    //    SVGImage image = new SVGImage();
                    //    image.transform.parent = __instance.transform.parent;
                    //    image.vectorGraphics = icon;

                    //    if (!__instance.MarkIcons.Contains(image))
                    //    {
                    //        Mod.Log.Debug("ADDING IMAGE FOR NO VISUALS");
                    //        __instance.MarkIcons.Add(image);
                    //    }
                    //}


                }
            }
        }
    }
}
