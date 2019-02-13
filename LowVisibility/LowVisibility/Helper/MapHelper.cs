using BattleTech.Rendering.Mood;
using HBS.Collections;
using System;

namespace LowVisibility.Helper {
    public static class MapHelper {

        public class MapConfig {

            public float visionRange = 0.0f;
            public float scanRange = 0.0f;

            public bool isDay;
            public bool isDim;
            public bool isDark;

            public bool hasLightFog;
            public bool hasHeavyFog;
            public bool hasSnow;
            public bool hasRain;

            public string UILabel() {
                string label;

                // Parse light
                if (isDay) { label = "Day";  }
                else if (isDim) { label = "Dim"; }
                else { label = "Dark";  }

                // Parse weather
                if (hasHeavyFog) { label += ", Dense Fog"; }
                else if (hasLightFog) { label += ", Fog"; }
                else if (hasSnow) { label += ", Snow"; }
                else if (hasRain) { label += ", Rain"; }

                return label;
            }
        }

        public static MapConfig ParseCurrentMap() {
            LowVisibility.Logger.Log("MH:PCM Parsing current map.");

            // This is a VERY slow call, that can add 30-40ms just to execute. Cache it!
            MoodController moodController = UnityEngine.Object.FindObjectOfType<MoodController>();

            MoodSettings moodSettings = moodController?.CurrentMood;
            TagSet moodTags = moodSettings?.moodTags;
            if (moodTags == null || moodTags.IsEmpty) {
                return new MapConfig {
                    visionRange = LowVisibility.Config.VisionRangeBaseDaylight,
                    scanRange = LowVisibility.Config.VisualScanRange
                };
            }


            LowVisibility.Logger.LogIfDebug($"  - Parsing current map for mod config");

            MapConfig mapConfig = new MapConfig();
            
            String allTags = String.Join(", ", moodTags.ToArray());
            LowVisibility.Logger.LogIfDebug($"  - All mood tags are: {allTags}");

            float baseVision = LowVisibility.Config.VisionRangeBaseDaylight;
            float visionMulti = 1.0f;
            foreach (string tag in moodTags) {
                switch (tag) {
                    case "mood_timeMorning":
                    case "mood_timeNoon":
                    case "mood_timeAfternoon":
                    case "mood_timeDay":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        mapConfig.isDay = true;
                        mapConfig.isDim = false;
                        mapConfig.isDark = false;
                        break;
                    case "mood_timeSunrise":
                    case "mood_timeSunset":
                    case "mood_timeTwilight":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (baseVision > LowVisibility.Config.VisionRangeBaseDimlight) {
                            baseVision = LowVisibility.Config.VisionRangeBaseDimlight;
                            mapConfig.isDay = false;
                            mapConfig.isDim = true;
                            mapConfig.isDark = false;
                        }
                        break;
                    case "mood_timeNight":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (baseVision > LowVisibility.Config.VisionRangeBaseNight) {
                            baseVision = LowVisibility.Config.VisionRangeBaseNight;
                            mapConfig.isDay = false;
                            mapConfig.isDim = false;
                            mapConfig.isDark = true;
                        }
                        break;
                    case "mood_weatherRain":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (visionMulti > LowVisibility.Config.VisionRangeMultiRainSnow) {
                            visionMulti = LowVisibility.Config.VisionRangeMultiRainSnow;
                            mapConfig.hasRain = true;
                        }
                        break;
                    case "mood_weatherSnow":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (visionMulti > LowVisibility.Config.VisionRangeMultiRainSnow) {
                            visionMulti = LowVisibility.Config.VisionRangeMultiRainSnow;
                            mapConfig.hasSnow = true;
                        }
                        break;
                    case "mood_fogLight":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (visionMulti > LowVisibility.Config.VisionRangeMultiLightFog) {
                            visionMulti = LowVisibility.Config.VisionRangeMultiLightFog;
                            mapConfig.hasLightFog = true;
                        }
                        break;
                    case "mood_fogHeavy":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (visionMulti > LowVisibility.Config.VisionRangeMultiHeavyFog) {
                            visionMulti = LowVisibility.Config.VisionRangeMultiHeavyFog;
                            mapConfig.hasHeavyFog = true;
                        }
                        break;
                    default:
                        break;
                }
            }            
            float visRange = (float)Math.Ceiling(baseVision * 30f * visionMulti);
            LowVisibility.Logger.Log($"  Calculating vision range as Math.Ceil(baseVision:{baseVision} * 30.0 * visionMulti:{visionMulti}) = visRange:{visRange}.");
            if (visRange < LowVisibility.Config.MinimumVisionRange()) {
                visRange = LowVisibility.Config.MinimumVisionRange();
            }
            
            float normalizedVisionRange = MathHelper.CountHexes(visRange, false) * 30f;
            LowVisibility.Logger.Log($"MapHelper: Vision range for map will be ==> {normalizedVisionRange}m (normalized from {visRange}m)");
            mapConfig.visionRange = normalizedVisionRange;
            mapConfig.scanRange = Math.Min(normalizedVisionRange, LowVisibility.Config.VisualScanRange * 30.0f);

            LowVisibility.Logger.Log($"Map vision range = visual:{normalizedVisionRange} / visualScan:{mapConfig.scanRange}");

            return mapConfig;
        }
    }
}
