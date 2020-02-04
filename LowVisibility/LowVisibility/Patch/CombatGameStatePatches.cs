using BattleTech;
using BattleTech.Data;
using Harmony;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            loadRequest.ProcessRequests();
            Mod.Log.Info("  ICON LOADING COMPLETE!");
        }

    }
}
