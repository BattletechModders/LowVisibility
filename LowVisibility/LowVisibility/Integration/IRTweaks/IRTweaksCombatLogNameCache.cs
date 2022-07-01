using BattleTech;
using Harmony;
using LowVisibility.Integration.IRTweaks;
using LowVisibility.Object;
using System.Collections.Generic;

namespace LowVisibility.Integration
{


    public static class CombatLogNameCacheHelper
    {

        public static void Add(string key, VisibilityLevel visibilityLevel, SensorScanType sensorScanType, string name)
        {
            IRTweaksHelper.LogIfEnabled($"Adding new entry for key: {key} with VisLevel: {visibilityLevel}, Sensors: {sensorScanType}, Name: {name}");
            ModState.CombatLogIntegrationNameCache[key] = new CombatLogNameCacheEntry(visibilityLevel, sensorScanType, name);
        }

        public static string Get(string GUID)
        {
            ModState.CombatLogIntegrationNameCache.TryGetValue(GUID, out CombatLogNameCacheEntry entry);
            return entry?.name ?? string.Empty;
        }

        public static bool ContainsEqualOrBetterName(string key, VisibilityLevel visibilityLevel, SensorScanType sensorScanType)
        {
            IRTweaksHelper.LogIfEnabled($"Contains check for key: {key}, VisLevel: {visibilityLevel}, Sensors: {sensorScanType}");
            ModState.CombatLogIntegrationNameCache.TryGetValue(key, out CombatLogNameCacheEntry cacheEntry);
            if (cacheEntry == null)
            {
                Mod.Log.Trace?.Write($"No cache entry for key: {key}");
                return false;
            }

            // Check if stored entry has higher visibility or same visibility and at least equal sensors
            bool cacheHasHigherVisibility = visibilityLevel < cacheEntry.visibilityLevel;
            bool cacheHasHigherSensors = (visibilityLevel == cacheEntry.visibilityLevel && sensorScanType <= cacheEntry.sensorScanType);
            IRTweaksHelper.LogIfEnabled($"Cache Entry for key {key} is VisLevel: {cacheEntry.visibilityLevel}, Sensors: {cacheEntry.sensorScanType}");
            bool cacheEntryIsHigher = cacheHasHigherVisibility || cacheHasHigherSensors;
            IRTweaksHelper.LogIfEnabled($"Cache Entry for key {key} is {(cacheEntryIsHigher ? "better" : "worse")}");
            return cacheEntryIsHigher;
        }

        public static void Clear()
        {
            ModState.CombatLogIntegrationNameCache.Clear();
        }

    }
    public class CombatLogNameCacheEntry
    {
        public VisibilityLevel visibilityLevel;
        public SensorScanType sensorScanType;
        public string name;

        public CombatLogNameCacheEntry(VisibilityLevel visibilityLevel, SensorScanType sensorScanType, string name)
        {
            this.name = name;
            this.visibilityLevel = visibilityLevel;
            this.sensorScanType = sensorScanType;
        }
    }

    [HarmonyPatch(typeof(CombatGameState), "OnCombatGameDestroyed")]
    static class CombatGameState_OnCombatGameDestroyed
    {
        static void Postfix()
        {
            if (Mod.Config.Integrations.IRTweaks.CombatLogNames == CombatLogIntegration.REMEMBER)
            {

                IRTweaksHelper.LogIfEnabled("Destroying CombatLogNameCache.");
                CombatLogNameCacheHelper.Clear();
            }
        }
    }

}
