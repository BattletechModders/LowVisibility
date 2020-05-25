using BattleTech;
using BattleTech.Data;
using Harmony;
using SVGImporter;

namespace LowVisibility.Patch {

    // Pre-load our required icons, otherwise DM will unload them as they aren't necessary
    [HarmonyPatch(typeof(CombatGameState), "_Init")]
    public static class CombatGameState__Init {
        public static void Postfix(CombatGameState __instance) {
            Mod.Log.Trace("CGS:_I entered.");
            DataManager dm = UnityGameInstance.BattleTechGame.DataManager;
            LoadRequest loadRequest = dm.CreateLoadRequest();

            // Need to load each unique icon
            Mod.Log.Info("LOADING EFFECT ICONS...");
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.ElectronicWarfare, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.SensorsDisabled, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.VisionAndSensors, null);

            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetSensorsMark, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetVisualsMark, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetTaggedMark, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetNarcedMark, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetStealthMark, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetMimeticMark, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetECMShieldedMark, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.TargetActiveProbePingedMark, null);

            loadRequest.ProcessRequests();
            Mod.Log.Info("  ICON LOADING COMPLETE!");

            ModState.Combat = __instance;
        }

    }

    [HarmonyPatch(typeof(CombatGameState), "OnCombatGameDestroyed")]
    static class CombatGameState_OnCombatGameDestroyed
    {
        static void Postfix()
        {
            Mod.Log.Trace("CGS:OCGD - entered.");

            ModState.Reset();
        }
    }
}
