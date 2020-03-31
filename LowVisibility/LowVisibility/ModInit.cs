using Harmony;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Reflection;
using us.frostraptor.modUtils.logging;

namespace LowVisibility {

    public class Mod {

        public const string HarmonyPackage = "us.frostraptor.LowVisibility";

        public const string LogFilename = "low_visibility";
        public const string LogLabel = "LOWVIS";

        public static IntraModLogger Log;
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

            Log = new IntraModLogger(modDirectory, LogFilename, LogLabel, Config.Debug, Config.Trace);

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info($"Assembly version: {fvi.ProductVersion}");

            Log.Debug($"ModDir is:{modDirectory}");
            Log.Debug($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
