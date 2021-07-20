using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using Harmony;
using HBS.Collections;
using System;

namespace LowVisibility.Patch
{
    [HarmonyPatch(typeof(MoodController), "ApplyMoodSettings")]
    static class MoodController_ApplyMoodSettings
    {
        static void Postfix(MoodController __instance, FogScattering ___mainFogScattering)
        {
			if (__instance == null || ___mainFogScattering == null) return;

			MoodSettings moodSettings = __instance?.CurrentMood;
			TagSet moodTags = moodSettings?.moodTags;
			bool isLightFog = false;
			bool isHeavyFog = false;
			foreach (string tag in moodTags)
			{
				if (tag.Equals("mood_fogLight", StringComparison.InvariantCultureIgnoreCase))
					isLightFog = true;
				if (tag.Equals("mood_fogHeavy", StringComparison.InvariantCultureIgnoreCase))
					isHeavyFog = true;
			}

			if (isLightFog || isHeavyFog)
            {
				Mod.Log.Info?.Write($"Fog isLight: {isLightFog} isHeavy: {isHeavyFog}");

				___mainFogScattering.fogSettings = __instance.currentMood.fogSettings;
				Mod.Log.Info?.Write($"Fog settings are: " +
					$" fogG: {___mainFogScattering.fogSettings.fogG}" +
					$" fogMieMultiplier: {___mainFogScattering.fogSettings.fogMieMultiplier}" +
					$" fogRayleighMultiplier: {___mainFogScattering.fogSettings.fogRayleighMultiplier}" +
					$" fogTintColor: {___mainFogScattering.fogSettings.fogTintColor}" +
					$" heightFogDensity: {___mainFogScattering.fogSettings.heightFogDensity}" +
					$" heightFogStart: {___mainFogScattering.fogSettings.heightFogStart}" +
					$" heightMieMultiplier: {___mainFogScattering.fogSettings.heightMieMultiplier}" +
					$" heightRayleighMultiplier: {___mainFogScattering.fogSettings.heightRayleighMultiplier}" +
					$" revealedIntensity: {___mainFogScattering.fogSettings.revealedIntensity}" +
					$" revealedMieMultiplier: {___mainFogScattering.fogSettings.revealedMieMultiplier}" +
					$" surveyedIntensity: {___mainFogScattering.fogSettings.surveyedIntensity}" +
					$" surveyedMieMultiplier: {___mainFogScattering.fogSettings.surveyedMieMultiplier}"
					);

				___mainFogScattering.fogSettings.heightFogDensity = isLightFog ? 
					Mod.Config.Weather.LightFogDensity : Mod.Config.Weather.HeavyFogDensity;
			}
		}
    }

}
