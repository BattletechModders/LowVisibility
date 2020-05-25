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
                    Mod.Log.Info($"UPDATING COMBATHUDMARKDISPLAY FOR ACTOR: {CombatantUtils.Label(__instance.DisplayedActor)}");
                    // Cache these
                    AbstractActor target = __instance.DisplayedActor;
                    AbstractActor attacker = ModState.LastPlayerActorActivated;

                    SVGAsset icon = ModState.Combat.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.TargetVisualsMark, BattleTechResourceType.SVGAsset);
                    if (icon == null) Mod.Log.Error("FAILED TO LOAD ICON!");
                    GameObject imageGO = new GameObject();
                    imageGO.name = "lv_icon_foo";
                    imageGO.transform.parent = __instance.gameObject.transform;
                    imageGO.transform.position = __instance.gameObject.transform.position;
                    //imageGO.transform.localScale = new Vector3(1f, 1f, 1f);

                    SVGImage image = imageGO.AddComponent<SVGImage>();
                    if (image == null) Mod.Log.Error("FAILED TO CREATE IMAGE!");
                    image.vectorGraphics = icon;
                    image.color = Color.green;
                    image.gameObject.SetActive(true);

                    RectTransform rectTransform = imageGO.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(28f, 28f);                    

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
