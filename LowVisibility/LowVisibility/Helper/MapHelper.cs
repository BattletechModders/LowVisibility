using BattleTech.Rendering.Mood;
using HBS.Collections;
using System;
using us.frostraptor.modUtils.math;

namespace LowVisibility.Helper {
    public static class MapHelper {

        public class MapConfig {

            public float spotterRange = 0.0f;
            public float visualIDRange = 0.0f;
            public float nightVisionSpotterRange = 0.0f;
            public float nightVisionVisualIDRange = 0.0f;

            public bool isDay;
            public bool isDim;
            public bool isDark;

            public bool hasLightFog;
            public bool hasHeavyFog;
            public bool hasSnow;
            public bool hasRain;

            public string UILabel() {
                // Parse light
                string lightLabel;
                if (isDay) { lightLabel = Mod.Config.LocalizedText[ModConfig.LT_MAP_LIGHT_BRIGHT]; }
                else if (isDim) { lightLabel = Mod.Config.LocalizedText[ModConfig.LT_MAP_LIGHT_DIM]; }
                else { lightLabel = Mod.Config.LocalizedText[ModConfig.LT_MAP_LIGHT_DARK]; }
                lightLabel = new Localize.Text(lightLabel).ToString();

                // Parse weather
                string weatherLabel = null;
                if (hasHeavyFog) { weatherLabel = Mod.Config.LocalizedText[ModConfig.LT_MAP_FOG_HEAVY]; }
                else if (hasLightFog) { weatherLabel = Mod.Config.LocalizedText[ModConfig.LT_MAP_FOG_LIGHT]; }
                else if (hasSnow) { weatherLabel = Mod.Config.LocalizedText[ModConfig.LT_MAP_SNOW]; }
                else if (hasRain) { weatherLabel = Mod.Config.LocalizedText[ModConfig.LT_MAP_RAIN]; }
                if (weatherLabel != null) { weatherLabel = new Localize.Text(weatherLabel).ToString(); }

                return weatherLabel == null ? lightLabel : lightLabel + ", " + weatherLabel;
            }
        }

        public static MapConfig ParseCurrentMap() {
            Mod.Log.Info("MH:PCM Parsing current map.");

            MoodController moodController = ModState.GetMoodController();

            MoodSettings moodSettings = moodController?.CurrentMood;
            TagSet moodTags = moodSettings?.moodTags;
            if (moodTags == null || moodTags.IsEmpty) {
                return new MapConfig {
                    spotterRange = Mod.Config.VisionRangeBaseDaylight,
                    visualIDRange = Mod.Config.VisualScanRange
                };
            }

            Mod.Log.Debug($"  - Parsing current map for mod config");
            MapConfig mapConfig = new MapConfig();
            
            String allTags = String.Join(", ", moodTags.ToArray());
            Mod.Log.Debug($"  - All mood tags are: {allTags}");

            float baseVision = Mod.Config.VisionRangeBaseDaylight;
            float visionMulti = 1.0f;
            foreach (string tag in moodTags) {
                switch (tag) {
                    case "mood_timeMorning":
                    case "mood_timeNoon":
                    case "mood_timeAfternoon":
                    case "mood_timeDay":
                        Mod.Log.Debug($"  - {tag}");
                        mapConfig.isDay = true;
                        mapConfig.isDim = false;
                        mapConfig.isDark = false;
                        break;
                    case "mood_timeSunrise":
                    case "mood_timeSunset":
                    case "mood_timeTwilight":
                        Mod.Log.Debug($"  - {tag}");
                        if (baseVision > Mod.Config.VisionRangeBaseDimlight) {
                            baseVision = Mod.Config.VisionRangeBaseDimlight;
                            mapConfig.isDay = false;
                            mapConfig.isDim = true;
                            mapConfig.isDark = false;
                        }
                        break;
                    case "mood_timeNight":
                        Mod.Log.Debug($"  - {tag}");
                        if (baseVision > Mod.Config.VisionRangeBaseNight) {
                            baseVision = Mod.Config.VisionRangeBaseNight;
                            mapConfig.isDay = false;
                            mapConfig.isDim = false;
                            mapConfig.isDark = true;
                        }
                        break;
                    case "mood_weatherRain":
                        Mod.Log.Debug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiRainSnow) {
                            visionMulti = Mod.Config.VisionRangeMultiRainSnow;
                            mapConfig.hasRain = true;
                        }
                        break;
                    case "mood_weatherSnow":
                        Mod.Log.Debug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiRainSnow) {
                            visionMulti = Mod.Config.VisionRangeMultiRainSnow;
                            mapConfig.hasSnow = true;
                        }
                        break;
                    case "mood_fogLight":
                        Mod.Log.Debug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiLightFog) {
                            visionMulti = Mod.Config.VisionRangeMultiLightFog;
                            mapConfig.hasLightFog = true;
                        }
                        break;
                    case "mood_fogHeavy":
                        Mod.Log.Debug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiHeavyFog) {
                            visionMulti = Mod.Config.VisionRangeMultiHeavyFog;
                            mapConfig.hasHeavyFog = true;
                        }
                        break;
                    default:
                        break;
                }
            }            

            // Calculate normal vision range
            float visRange = (float)Math.Ceiling(baseVision * 30f * visionMulti);
            Mod.Log.Info($"  Calculating vision range as Math.Ceil(baseVision:{baseVision} * 30.0 * visionMulti:{visionMulti}) = visRange:{visRange}.");
            if (visRange < Mod.Config.MinimumVisionRange()) {
                visRange = Mod.Config.MinimumVisionRange();
            }
            
            float roundedVisRange = HexUtils.CountHexes(visRange, false) * 30f;
            Mod.Log.Info($"MapHelper: Vision range for map will be ==> {roundedVisRange}m (normalized from {visRange}m)");
            mapConfig.spotterRange = roundedVisRange;
            mapConfig.visualIDRange = Math.Min(roundedVisRange, Mod.Config.VisualScanRange * 30.0f);

            Mod.Log.Info($"Map vision range = visual:{roundedVisRange} / visualScan:{mapConfig.visualIDRange}");

            // Calculate night vision range
            if (mapConfig.isDark) {
                float nightVisRange = (float)Math.Ceiling(Mod.Config.VisionRangeBaseDaylight * 30f * visionMulti);
                if (nightVisRange < Mod.Config.MinimumVisionRange()) {
                    nightVisRange = Mod.Config.MinimumVisionRange();
                }

                float roundedNightVisRange = HexUtils.CountHexes(nightVisRange, false) * 30f;
                Mod.Log.Info($"MapHelper: Night vision range for map will be ==> {roundedNightVisRange}m (normalized from {nightVisRange}m)");
                mapConfig.nightVisionSpotterRange = roundedNightVisRange;
                mapConfig.nightVisionVisualIDRange = Math.Min(roundedNightVisRange, Mod.Config.VisualScanRange * 30.0f);
            }

            return mapConfig;
        }
    }
}
