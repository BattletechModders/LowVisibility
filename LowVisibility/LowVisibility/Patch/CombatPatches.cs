using BattleTech.UI;
using System.Collections.Generic;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{

    [HarmonyPatch(typeof(CombatSelectionHandler), "TrySelectActor")]
    public static class CombatSelectionHandler_TrySelectActor
    {
        public static void Postfix(CombatSelectionHandler __instance, bool __result, AbstractActor actor, bool manualSelection)
        {
            Mod.Log.Debug?.Write($"=== CombatSelectionHandler:TrySelectActor:post - entered for {CombatantUtils.Label(actor)}.");
            if (__instance != null && actor != null && __result == true && actor.IsAvailableThisPhase)
            {

                // Do this to force a refresh during a combat save
                if (TurnDirector_OnEncounterBegin.IsFromSave)
                {
                    DEBUG_ToggleForcedVisibility(false, actor.Combat);
                    TurnDirector_OnEncounterBegin.IsFromSave = false;
                }
            }
        }

        public static void DEBUG_ToggleForcedVisibility(bool forceVisible, CombatGameState Combat)
        {
            List<ICombatant> list = Combat.AllActors.ConvertAll<ICombatant>((AbstractActor x) => x);
            for (int i = 0; i < list.Count; i++)
            {
                PilotableActorRepresentation pilotableActorRepresentation = list[i].GameRep as PilotableActorRepresentation;
                if (pilotableActorRepresentation != null)
                {
                    if (forceVisible)
                    {
                        pilotableActorRepresentation.SetForcedPlayerVisibilityLevel(VisibilityLevel.LOSFull, true);
                    }
                    else
                    {
                        pilotableActorRepresentation.ClearForcedPlayerVisibilityLevel(list);
                    }
                }
            }
        }
    }


}
