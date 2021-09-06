using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using Harmony;
using HBS.Collections;
using LowVisibility.Helper;
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
			if (MoodHelper.IsFoggy(__instance))
            {
				bool isHeavyFog = MoodHelper.IsHeavyFog(__instance);
				Mod.Log.Info?.Write($"Fog detected => isHeavy: {isHeavyFog}");

				___mainFogScattering.fogSettings = __instance.currentMood.fogSettings;
				Mod.Log.Info?.Write($"Mood settings for fog are: " +
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

				if ()
				___mainFogScattering.fogSettings.heightFogDensity = isHeavyFog ? 
					Mod.Config.Weather.HeavyFogDensity : Mod.Config.Weather.LightFogDensity;
				Mod.Log.Info?.Write($"Updated heightFogDensity to: {___mainFogScattering.fogSettings.heightFogDensity}");
			}
		}
    }

	[HarmonyPatch(typeof(LanceSpawnerGameLogic), "SpawnUnits")]
	[HarmonyPatch(new Type[] { typeof(bool) })]
	public static class LanceSpawnerGameLogic_SpawnUnits
	{

		public static void Postfix()
		{
			
		}
	}

}
