using LowVisibility.Object;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{
    [HarmonyPatch(typeof(MultiAttack), "GetExpectedDamageForMultiTargetWeapon")]
    public static class MultiAttack_GetExpectedDamageForMultiTargetWeapon
    {

        public static void Postfix(ref float __result, ICombatant targetUnit)
        {
            Mod.Log.Trace?.Write("MA:GEDFMTW entered");

            if (targetUnit is AbstractActor targetActor)
            {
                EWState targetState = new EWState(targetActor);
                if (targetState.HasStealth() || targetState.HasMimetic())
                {
                    Mod.Log.Debug?.Write($"Target {CombatantUtils.Label(targetUnit)} has stealth, AI cannot multi-attack!");
                    __result = 0f;
                }
            }

        }
    }
}
