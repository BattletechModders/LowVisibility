using BattleTech.UI;
using Harmony;
using IRBTModUtils.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace LowVisibility
{

    public class Mod
    {

        public const string HarmonyPackage = "us.frostraptor.LowVisibility";

        public const string LogFilename = "low_visibility";
        public const string LogLabel = "LOWVIS";

        public static DeferringLogger Log;
        public static string ModDir;
        public static ModConfig Config;
        public static ModText LocalizedText;

        public static string CampaignSeed;
        public static bool CustomUnitsAPIDetected { get; private set; } = false;
        public static readonly Random Random = new Random();
        public delegate void d_SetArmorDisplayActive(CombatHUDTargetingComputer __instance, bool active);
        private static d_SetArmorDisplayActive i_SetArmorDisplayActive = null;
        public static void detectCU() {
          try { 
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
              CustomUnitsAPIDetected = true;
              if (assembly.FullName.StartsWith("CustomUnits")) {
                {
                  Type helperType = assembly.GetType("CustomUnits.LowVisibilityAPIHelper");
                  MethodInfo method = helperType.GetMethod("SetArmorDisplayActive", BindingFlags.Public | BindingFlags.Static);
                  var dm = new DynamicMethod("CU_SetArmorDisplayActive", null, new Type[] { typeof(CombatHUDTargetingComputer), typeof(bool) });
                  var gen = dm.GetILGenerator();
                  gen.Emit(OpCodes.Ldarg_0);
                  gen.Emit(OpCodes.Ldarg_1);
                  gen.Emit(OpCodes.Call, method);
                  gen.Emit(OpCodes.Ret);
                  i_SetArmorDisplayActive = (d_SetArmorDisplayActive)dm.CreateDelegate(typeof(d_SetArmorDisplayActive));
                }
              }
            }
          } catch (Exception e) {
            CustomUnitsAPIDetected = false;
            Log.Error?.Write(e.ToString());
          }
        }
        public static void CU_SetArmorDisplayActive(CombatHUDTargetingComputer __instance, bool active) {
           i_SetArmorDisplayActive?.Invoke(__instance, active);
        }
        public static void FinishedLoading(List<string> loadOrder) {
          detectCU();
          LowVisibility.Patch.ToHit_GetAllModifiers.registerCACToHitModifiers();
        }
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
        }

    }
}
