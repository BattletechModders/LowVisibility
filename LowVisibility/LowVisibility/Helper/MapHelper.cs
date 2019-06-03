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
            Mod.Log.Log("MH:PCM Parsing current map.");

            // This is a VERY slow call, that can add 30-40ms just to execute. Cache it!
            MoodController moodController = UnityEngine.Object.FindObjectOfType<MoodController>();

            MoodSettings moodSettings = moodController?.CurrentMood;
            TagSet moodTags = moodSettings?.moodTags;
            if (moodTags == null || moodTags.IsEmpty) {
                return new MapConfig {
                    visionRange = Mod.Config.VisionRangeBaseDaylight,
                    scanRange = Mod.Config.VisualScanRange
                };
            }


            Mod.Log.LogIfDebug($"  - Parsing current map for mod config");

            MapConfig mapConfig = new MapConfig();
            
            String allTags = String.Join(", ", moodTags.ToArray());
            Mod.Log.LogIfDebug($"  - All mood tags are: {allTags}");

            float baseVision = Mod.Config.VisionRangeBaseDaylight;
            float visionMulti = 1.0f;
            foreach (string tag in moodTags) {
                switch (tag) {
                    case "mood_timeMorning":
                    case "mood_timeNoon":
                    case "mood_timeAfternoon":
                    case "mood_timeDay":
                        Mod.Log.LogIfDebug($"  - {tag}");
                        mapConfig.isDay = true;
                        mapConfig.isDim = false;
                        mapConfig.isDark = false;
                        break;
                    case "mood_timeSunrise":
                    case "mood_timeSunset":
                    case "mood_timeTwilight":
                        Mod.Log.LogIfDebug($"  - {tag}");
                        if (baseVision > Mod.Config.VisionRangeBaseDimlight) {
                            baseVision = Mod.Config.VisionRangeBaseDimlight;
                            mapConfig.isDay = false;
                            mapConfig.isDim = true;
                            mapConfig.isDark = false;
                        }
                        break;
                    case "mood_timeNight":
                        Mod.Log.LogIfDebug($"  - {tag}");
                        if (baseVision > Mod.Config.VisionRangeBaseNight) {
                            baseVision = Mod.Config.VisionRangeBaseNight;
                            mapConfig.isDay = false;
                            mapConfig.isDim = false;
                            mapConfig.isDark = true;
                        }
                        break;
                    case "mood_weatherRain":
                        Mod.Log.LogIfDebug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiRainSnow) {
                            visionMulti = Mod.Config.VisionRangeMultiRainSnow;
                            mapConfig.hasRain = true;
                        }
                        break;
                    case "mood_weatherSnow":
                        Mod.Log.LogIfDebug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiRainSnow) {
                            visionMulti = Mod.Config.VisionRangeMultiRainSnow;
                            mapConfig.hasSnow = true;
                        }
                        break;
                    case "mood_fogLight":
                        Mod.Log.LogIfDebug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiLightFog) {
                            visionMulti = Mod.Config.VisionRangeMultiLightFog;
                            mapConfig.hasLightFog = true;
                        }
                        break;
                    case "mood_fogHeavy":
                        Mod.Log.LogIfDebug($"  - {tag}");
                        if (visionMulti > Mod.Config.VisionRangeMultiHeavyFog) {
                            visionMulti = Mod.Config.VisionRangeMultiHeavyFog;
                            mapConfig.hasHeavyFog = true;
                        }
                        break;
                    default:
                        break;
                }
            }            
            float visRange = (float)Math.Ceiling(baseVision * 30f * visionMulti);
            Mod.Log.Log($"  Calculating vision range as Math.Ceil(baseVision:{baseVision} * 30.0 * visionMulti:{visionMulti}) = visRange:{visRange}.");
            if (visRange < Mod.Config.MinimumVisionRange()) {
                visRange = Mod.Config.MinimumVisionRange();
            }
            
            float normalizedVisionRange = MathHelper.CountHexes(visRange, false) * 30f;
            Mod.Log.Log($"MapHelper: Vision range for map will be ==> {normalizedVisionRange}m (normalized from {visRange}m)");
            mapConfig.visionRange = normalizedVisionRange;
            mapConfig.scanRange = Math.Min(normalizedVisionRange, Mod.Config.VisualScanRange * 30.0f);

            Mod.Log.Log($"Map vision range = visual:{normalizedVisionRange} / visualScan:{mapConfig.scanRange}");

            return mapConfig;
        }
    }
}
