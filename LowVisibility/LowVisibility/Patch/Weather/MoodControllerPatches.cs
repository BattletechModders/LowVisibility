using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using CustAmmoCategories;
using CustomUnits;
using Harmony;
using HBS.Collections;
using IRBTModUtils.Extension;
using LowVisibility.Helper;
using System;

namespace LowVisibility.Patch
{
	[HarmonyPatch(typeof(MoodController), "ApplyMoodSettings")]
	static class MoodController_ApplyMoodSettings
	{
		static bool Prepare() => Mod.Config.Weather.ModifyFog;

		static void Postfix(MoodController __instance, FogScattering ___mainFogScattering)
		{
			if (__instance == null || ___mainFogScattering == null) return;

			// Check for CU manual deploy
			if (ModState.Combat == null || ModState.Combat.ActiveContract == null) return;

			Mod.Log.Info?.Write($" -- CHECKING MOOD SETTINGS");
			Mod.Log.Info?.Write($"   currentMood: {__instance.CurrentMood?.GetFriendlyName()}");
			Mod.Log.Info?.Write($"   moodTags: {(String.Join(",", __instance.CurrentMood?.moodTags))}");

			// If we're past here, safe to set
			if (MoodHelper.IsFoggy(__instance))
			{
				bool isHeavyFog = MoodHelper.IsHeavyFog(__instance);
				Mod.Log.Info?.Write($"Fog detected => isHeavy: {isHeavyFog}");

				___mainFogScattering.fogSettings = __instance.CurrentMood.fogSettings;
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

				if (!LanceSpawnerGameLogic_OnEnterActive.isObjectivesReady(ModState.Combat.ActiveContract))
				{
					Mod.Log.Info?.Write("Deferring mood settings until after manual deployment. Disabling fog");
					___mainFogScattering.fogSettings.heightFogDensity = 100f;
					return;
				}
				else
                {
					___mainFogScattering.fogSettings.heightFogDensity = isHeavyFog ?
						Mod.Config.Weather.HeavyFogDensity : Mod.Config.Weather.LightFogDensity;
					Mod.Log.Info?.Write($"Updated heightFogDensity to: {___mainFogScattering.fogSettings.heightFogDensity}");
				}
			}
		}
	}

}
