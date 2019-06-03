using Harmony;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace LowVisibility {

    public class Mod {

        public const string HarmonyPackage = "us.frostraptor.LowVisibility";

        public static Logger Log;
        public static string ModDir;
        public static ModConfig Config;

        public static string CampaignSeed;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            ModDir = modDirectory;

            Exception settingsE;
            try {
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            } catch (Exception e) {
                settingsE = e;
                Mod.Config = new ModConfig();
            }

            Log = new Logger(modDirectory, "low_visibility");
            Log.LogIfDebug($"ModDir is:{modDirectory}");
            Log.LogIfDebug($"mod.json settings are:({settingsJSON})");
            Log.LogIfDebug($"mergedConfig is:{Mod.Config}");

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
