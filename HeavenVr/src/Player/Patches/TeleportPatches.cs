using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine;

namespace HeavenVr.Player.Patches;

[HarmonyPatch]
public static class TeleportPatches
{
    private static float _rotationX;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerTeleport), nameof(PlayerTeleport.Teleport))]
    private static void StoreMouseLookX(PlayerTeleport __instance)
    {
        // Need to store mouseLookX before Teleport, because Teleport modifies it and I want to reset it after.
        _rotationX = RM.drifter.mouseLookX.rotationX;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTeleport), nameof(PlayerTeleport.Teleport))]
    private static void FixTeleportRotation(PlayerTeleport __instance)
    {
        if (VrStage.Instance == null) return;

        // Reset mouseLookX to the value stored in the prefix patch.
        RM.drifter.mouseLookX.SetRotationX(_rotationX);

        // Make the player face forward upon exiting a portal.
        VrStage.Instance.RecenterRotation();
        VrStage.Instance.transform.Rotate(Vector3.up, __instance.transform.eulerAngles.y - _rotationX);
    }
}