using BattleTech;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.Save.SaveGameStructure.Messages;
using Harmony;
using System;
using System.Reflection;

namespace LowVisibility.Patch {

    //[HarmonyPatch(typeof(TurnDirector), "Dehydrate")]
    //public static class TurnDirector_Dehydrate {
    //    public static void Postfix(TurnDirector __instance) {
    //        LowVisibility.Logger.LogIfDebug($"TurnDirector:Dehydrate:post - Combat.GUID:{__instance.Combat.GUID} Combat.FileID:{__instance.Combat.FileID}");
    //    }
    //}


    //[HarmonyPatch(typeof(TurnDirector), "Hydrate")]
    //public static class TurnDirector_Hydrate {
    //    public static void Postfix(TurnDirector __instance, CombatGameState combat) {
    //        LowVisibility.Logger.LogIfDebug($"TurnDirector:Hydrate:post - Combat.GUID:{combat.GUID} Combat.FileID:{combat.FileID} Combat.WasFromSave:{combat.WasFromSave}");
    //    }
    //}


    //[HarmonyPatch(typeof(CombatGameState), "Dehydrate")]
    //public static class CombatGameState_Dehydrate {
    //    public static void Postfix(CombatGameState __instance) {
    //        LowVisibility.Logger.LogIfDebug($"CombatGameState:Dehydrate:post - contract.GUID:{__instance.ActiveContract.GUID}");
    //    }
    //}


    //[HarmonyPatch(typeof(CombatGameState), "Hydrate")]
    //public static class CombatGameState_Hydrate {
    //    public static void Postfix(CombatGameState __instance) {
    //        LowVisibility.Logger.LogIfDebug($"CombatGameState:Hydrate:post - contract.GUID:{__instance.ActiveContract.GUID}");
    //    }
    //}

    //[HarmonyPatch()]
    //public static class GameInstanceSave_PreSerialization {

    //    // Private method can't be patched by annotations, so use MethodInfo
    //    public static MethodInfo TargetMethod() {
    //        return AccessTools.Method(typeof(GameInstanceSave), "PreSerialization", new Type[] { });
    //    }

    //    public static void Postfix(GameInstanceSave __instance) {
    //        LowVisibility.Logger.LogIfDebug($"GameInstanceSave:PreSerialization:post - " +
    //            $"timestamp is:{__instance.SaveTime} battlename:{__instance.BattleName} FileID:{__instance.FileID} " +
    //            $"InstanceGUID:{__instance.InstanceGUID} SaveNameOverride:{__instance.SaveNameOverride} Version:{__instance.Version}");
    //    }
    //}

    //[HarmonyPatch()]    
    //public static class GameInstanceSave_PostDeserialization {

    //    // Private method can't be patched by annotations, so use MethodInfo
    //    public static MethodInfo TargetMethod() {
    //        return AccessTools.Method(typeof(GameInstanceSave), "PostDeserialization", new Type[] { });
    //    }

    //    public static void Postfix(GameInstanceSave __instance) {
    //        LowVisibility.Logger.LogIfDebug($"GameInstanceSave:PostDeserialization:post - " +
    //            $"timestamp is:{__instance.SaveTime} battlename:{__instance.BattleName} FileID:{__instance.FileID} " +
    //            $"InstanceGUID:{__instance.InstanceGUID} SaveNameOverride:{__instance.SaveNameOverride} Version:{__instance.Version}");

    //    }
    //}


    [HarmonyPatch(typeof(GameInstance), "Load")]
    [HarmonyPatch(new Type[] { typeof(GameInstanceSave) })]
    public static class GameInstance_Load {
        public static void Postfix(GameInstance __instance, GameInstanceSave save) {
            LowVisibility.Logger.LogIfDebug($"GameInstance:Load:post - entered - save " +
                $"instanceGUID is:{save.InstanceGUID} fileId:{save.FileID} " +
                $"isSkirmish:{save.IsSkirmish} saveTime:{save.SaveTime}");
            State.LoadStateData(save.FileID);
        }
    }


    [HarmonyPatch()]
    public static class GameInstance_SaveComplete {
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(GameInstance), "SaveComplete", new Type[] { typeof(StructureSaveComplete), typeof(bool) });
        }

        public static void Postfix(GameInstance __instance, StructureSaveComplete message, bool quit) {
            LowVisibility.Logger.LogIfDebug($"GameInstance:SaveComplete:post - entered - save " +
                $"instanceGUID is:{message.Slot.InstanceGUID} fileId:{message.Slot.FileID} " +
                $"isSkirmish:{message.Slot.IsSkirmish} saveTime:{message.Slot.SaveTime}");
            State.SaveStateData(message.Slot.FileID);
        }
    }

    //[HarmonyPatch(typeof(TurnDirector), "InitFromSave")]
    //public static class TurnDirector_InitFromSave {
    //    public static void Postfix(TurnDirector __instance) {
    //        LowVisibility.Logger.LogIfDebug("TurnDirector:InitFromSave:post - entered.");

    //        // Do a pre-encounter populate 
    //        if (__instance != null && __instance.Combat != null && __instance.Combat.AllActors != null) {
    //            Helper.MakeSensorChecksOnLoad(__instance.Combat);
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(SaveGameStructure), "Load")]
    //[HarmonyPatch(new Type[] { typeof(string) })]
    //public static class SaveGameStructure_Load {
    //    public static void Postfix(SaveGameStructure __instance, string slotFileID) {
    //        LowVisibility.Logger.LogIfDebug($"SaveGameStructure:Save:post - entered - slotFileID:{slotFileID} ");
    //    }
    //}

    //[HarmonyPatch(typeof(SaveGameStructure), "Save")]
    //[HarmonyPatch(new Type[] { typeof(GameInstance), typeof(SaveReason), typeof(string) })]
    //public static class SaveGameStructure_Save {
    //    public static void Postfix(SaveGameStructure __instance, GameInstance gameInstance, SaveReason saveReason, string saveNameOverride) {
    //        LowVisibility.Logger.LogIfDebug($"SaveGameStructure:Save:post - entered: ");
    //        Traverse GetSaveFileIDMethod = Traverse.Create(__instance).Method("GetSaveFileID", new object[] { });
    //        LowVisibility.Logger.LogIfDebug($"SaveGameStructure:Save:post - saveFile name is: {GetSaveFileIDMethod.GetValue()}");
    //    }
    //}

}
