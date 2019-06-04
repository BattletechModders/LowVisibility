using BattleTech;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure.Messages;
using Harmony;
using System;
using System.Reflection;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(GameInstance), "Load")]
    [HarmonyPatch(new Type[] { typeof(GameInstanceSave) })]
    public static class GameInstance_Load {

        public static void Postfix(GameInstance __instance, GameInstanceSave save) {
            Mod.Log.Debug($"Loading  saveGame with fileId:{save.FileID}");
            State.LoadStateData(save.FileID);
        }
    }

    [HarmonyPatch()]
    public static class GameInstance_SaveComplete {

        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(GameInstance), "SaveComplete", new Type[] { typeof(StructureSaveComplete), typeof(bool) });
        }

        public static void Postfix(GameInstance __instance, StructureSaveComplete message, bool quit) {
            if (message.Slot.HasCombatData) {
                Mod.Log.Debug($"Creating combat saveGame with fileId:{message.Slot.FileID}");
                State.SaveStateData(message.Slot.FileID);
            }
        }
    }

}