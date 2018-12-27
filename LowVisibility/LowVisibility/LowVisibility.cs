using Harmony;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace LowVisibility {
    public class LowVisibility {

        public const string HarmonyPackage = "us.frostraptor.LowVisibility";

        public static Logger Logger;
        public static string ModDir;
        public static ModConfig Config;

        public static string CampaignSeed;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            ModDir = modDirectory;

            Exception settingsE;
            try {
                LowVisibility.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            } catch (Exception e) {
                settingsE = e;
                LowVisibility.Config = new ModConfig();
            }

            Logger = new Logger(modDirectory, "low_visibility", Config.Debug);
            Logger.LogIfDebug($"Mod settings are:({settingsJSON})");

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
