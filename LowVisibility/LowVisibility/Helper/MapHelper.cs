using BattleTech.Rendering.Mood;
using HBS.Collections;
using System;
using us.frostraptor.modUtils.math;

namespace LowVisibility.Helper {

    public class MapConfig
    {
        public float spotterRange = 0.0f;
        public float visualIDRange = 0.0f;
        public float nightVisionSpotterRange = 0.0f;
        public float nightVisionVisualIDRange = 0.0f;

        public float visionMulti = 1.0f;

        public bool isDay;
        public bool isDim;
        public bool isDark;

        public bool hasLightFog;
        public bool hasHeavyFog;
        public bool hasSnow;
        public bool hasRain;

        public string UILabel()
        {
            // Parse light
            string lightLabel;
            if (isDay) { lightLabel = Mod.LocalizedText.MapEffects[ModText.LT_MAP_LIGHT_BRIGHT]; }
            else if (isDim) { lightLabel = Mod.LocalizedText.MapEffects[ModText.LT_MAP_LIGHT_DIM]; }
            else { lightLabel = Mod.LocalizedText.MapEffects[ModText.LT_MAP_LIGHT_DARK]; }
            lightLabel = new Localize.Text(lightLabel).ToString();

            // Parse weather
            string weatherLabel = null;
            if (hasHeavyFog) { weatherLabel = Mod.LocalizedText.MapEffects[ModText.LT_MAP_FOG_HEAVY]; }
            else if (hasLightFog) { weatherLabel = Mod.LocalizedText.MapEffects[ModText.LT_MAP_FOG_LIGHT]; }
            else if (hasSnow) { weatherLabel = Mod.LocalizedText.MapEffects[ModText.LT_MAP_SNOW]; }
            else if (hasRain) { weatherLabel = Mod.LocalizedText.MapEffects[ModText.LT_MAP_RAIN]; }
            if (weatherLabel != null) { weatherLabel = new Localize.Text(weatherLabel).ToString(); }

            return weatherLabel == null ? lightLabel : lightLabel + ", " + weatherLabel;
        }
    }

    public static class MapHelper {

        public static MapConfig ParseCurrentMap() {
            Mod.Log.Info?.Write(" -- PARSING CURRENT MAP");

            MoodController moodController = MoodController.Instance;
            MoodSettings moodSettings = moodController?.CurrentMood;
            Mod.Log.Info?.Write($"   currentMood: {moodSettings.GetFriendlyName()}");

            TagSet moodTags = moodSettings?.moodTags;
            if (moodTags == null || moodTags.IsEmpty) {
                return new MapConfig {
                    spotterRange = Mod.Config.Vision.RangeBright,
                    visualIDRange = Mod.Config.Vision.ScanRange
                };
            }

            Mod.Log.Debug?.Write($"  - Parsing current map for mod config");
            MapConfig mapConfig = new MapConfig();
            
            String allTags = String.Join(", ", moodTags.ToArray());
            Mod.Log.Debug?.Write($"  - All mood tags are: {allTags}");

            float baseVision = Mod.Config.Vision.RangeBright;

            foreach (string tag in moodTags) {
                switch (tag) {
                    case "mood_timeMorning":
                    case "mood_timeNoon":
                    case "mood_timeAfternoon":
                    case "mood_timeDay":
                        Mod.Log.Debug?.Write($"  - {tag}");
                        mapConfig.isDay = true;
                        mapConfig.isDim = false;
                        mapConfig.isDark = false;
                        break;
                    case "mood_timeSunrise":
                    case "mood_timeSunset":
                    case "mood_timeTwilight":
                        Mod.Log.Debug?.Write($"  - {tag}");
                        if (baseVision > Mod.Config.Vision.RangeDim) {
                            baseVision = Mod.Config.Vision.RangeDim;
                            mapConfig.isDay = false;
                            mapConfig.isDim = true;
                            mapConfig.isDark = false;
                        }
                        break;
                    case "mood_timeNight":
                        Mod.Log.Debug?.Write($"  - {tag}");
                        if (baseVision > Mod.Config.Vision.RangeDark) {
                            baseVision = Mod.Config.Vision.RangeDark;
                            mapConfig.isDay = false;
                            mapConfig.isDim = false;
                            mapConfig.isDark = true;
                        }
                        break;
                    case "mood_weatherRain":
                        Mod.Log.Debug?.Write($"  - {tag}");
                        if (mapConfig.visionMulti > Mod.Config.Vision.RangeMultiRainSnow) {
                            mapConfig.visionMulti = Mod.Config.Vision.RangeMultiRainSnow;
                            mapConfig.hasRain = true;
                        }
                        break;
                    case "mood_weatherSnow":
                        Mod.Log.Debug?.Write($"  - {tag}");
                        if (mapConfig.visionMulti > Mod.Config.Vision.RangeMultiRainSnow) {
                            mapConfig.visionMulti = Mod.Config.Vision.RangeMultiRainSnow;
                            mapConfig.hasSnow = true;
                        }
                        break;
                    case "mood_fogLight":
                        Mod.Log.Debug?.Write($"  - {tag}");
                        if (mapConfig.visionMulti > Mod.Config.Vision.RangeMultiLightFog) {
                            mapConfig.visionMulti = Mod.Config.Vision.RangeMultiLightFog;
                            mapConfig.hasLightFog = true;
                        }
                        break;
                    case "mood_fogHeavy":
                        Mod.Log.Debug?.Write($"  - {tag}");
                        if (mapConfig.visionMulti > Mod.Config.Vision.RangeMultiHeavyFog) {
                            mapConfig.visionMulti = Mod.Config.Vision.RangeMultiHeavyFog;
                            mapConfig.hasHeavyFog = true;
                        }
                        break;
                    default:
                        break;
                }
            }            

            // Calculate normal vision range
            float visRange = (float)Math.Ceiling(baseVision * mapConfig.visionMulti);
            Mod.Log.Info?.Write($"  Calculating vision range as Math.Ceil(baseVision:{baseVision} * visionMulti:{mapConfig.visionMulti}) = visRange:{visRange}.");
            if (visRange < Mod.Config.Vision.MinimumRange) {
                visRange = Mod.Config.Vision.MinimumRange;
            }
            mapConfig.spotterRange = visRange;
            mapConfig.visualIDRange = Math.Min(visRange, Mod.Config.Vision.ScanRange);
            Mod.Log.Info?.Write($"Map vision range = visual:{mapConfig.spotterRange} / visualScan:{mapConfig.visualIDRange}");

            // Calculate night vision range
            if (mapConfig.isDark) {
                float nightVisRange = (float)Math.Ceiling(Mod.Config.Vision.RangeBright * mapConfig.visionMulti);
                if (nightVisRange < Mod.Config.Vision.MinimumRange) {
                    nightVisRange = Mod.Config.Vision.MinimumRange;
                }
                mapConfig.nightVisionSpotterRange = nightVisRange;
                mapConfig.nightVisionVisualIDRange = Math.Min(nightVisRange, Mod.Config.Vision.ScanRange);
                Mod.Log.Info?.Write($"Map night vision range = visual:{mapConfig.nightVisionSpotterRange} / visualScan:{mapConfig.nightVisionVisualIDRange}");
            }

            return mapConfig;
        }
    }
}
