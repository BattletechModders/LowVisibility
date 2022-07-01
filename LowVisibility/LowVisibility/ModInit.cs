using BattleTech.UI;
using Harmony;
using IRBTModUtils.Logging;
using IRTweaks.Modules.UI;
using LowVisibility.Integration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LowVisibility
{

    public class Mod
    {
        public const string HarmonyPackage = "us.frostraptor.LowVisibility";
        public const string LogFilename = "low_visibility";
        public const string LogLabel = "LOWVIS";

        public static DeferringLogger Log;
        public static DeferringLogger ActorStateLog;
        public static DeferringLogger EffectsLog;
        public static DeferringLogger ToHitLog;
        public static DeferringLogger UILog;


        public static string ModDir;
        public static ModConfig Config;
        public static ModText LocalizedText;

        public static string CampaignSeed;
        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON)
        {
            ModDir = modDirectory;

            Exception settingsE = null;
            try
            {
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            }
            catch (Exception e)
            {
                settingsE = e;
                Mod.Config = new ModConfig();
            }
            Mod.Config.Init();

            Log = new DeferringLogger(modDirectory, LogFilename, LogLabel, Config.Debug, Config.Trace);
            if (settingsE != null)
                Log.Info?.Write($"ERROR reading settings file! Error was: {settingsE}");
            else
                Log.Info?.Write($"INFO: No errors reading settings file.");
            ActorStateLog = new DeferringLogger(modDirectory, $"{LogFilename}.actorstate", LogLabel, Config.Debug, Config.Trace);
            EffectsLog = new DeferringLogger(modDirectory, $"{LogFilename}.effects", LogLabel, Config.Debug, Config.Trace);
            ToHitLog = new DeferringLogger(modDirectory, $"{LogFilename}.tohit", LogLabel, Config.Debug, Config.Trace);
            UILog = new DeferringLogger(modDirectory, $"{LogFilename}.ui", LogLabel, Config.Debug, Config.Trace);


            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info?.Write($"Assembly version: {fvi.ProductVersion}");

            // Read config
            Log.Debug?.Write($"ModDir is:{modDirectory}");
            Log.Debug?.Write($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();

            // Read localization
            string localizationPath = Path.Combine(ModDir, "./mod_localized_text.json");
            try
            {
                string jsonS = File.ReadAllText(localizationPath);
                Mod.LocalizedText = JsonConvert.DeserializeObject<ModText>(jsonS);
            }
            catch (Exception e)
            {
                Mod.LocalizedText = new ModText();
                Log.Error?.Write(e, $"Failed to read localizations from: {localizationPath} due to error!");
            }

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (Mod.Config.Integrations.IRTweaks.CombatLogNames != CombatLogIntegration.NONE)
            {
                CombatLog.RegisterUnitNameModifier(CombatLogIntegration.LowVisibilityCombatLogUnitNameModifier);
                CombatLog.RegisterPilotNameModifier(CombatLogIntegration.LowVisibilityCombatLogPilotNameModifier);
            }
        }

        public static void FinishedLoading(List<string> loadOrder)
        {
            // Hook all toHit modifiers
            CACToHitHooks.RegisterToHitModifiers();
        }

    }
}
