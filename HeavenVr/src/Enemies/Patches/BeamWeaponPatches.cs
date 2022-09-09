using HarmonyLib;
using UnityEngine;

namespace HeavenVr.Enemies.Patches;

[HarmonyPatch]
public static class BeamWeaponPatches
{
    private static Vector3 _playerPosition;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BeamWeapon), nameof(BeamWeapon.WeaponStart))]
    private static void FixBeamOffset(BeamWeapon __instance)
    {
        __instance._playerCamTrackingOffset = Vector3.up;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BeamWeapon), nameof(BeamWeapon.TrackingRoutine), MethodType.Enumerator)]
    private static void MakeBeamAimAtPlayerSetUp()
    {
        _playerPosition = RM.mechController.playerCamera.transform.position;
        RM.mechController.playerCamera.transform.position = RM.mechController.transform.position;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BeamWeapon), nameof(BeamWeapon.TrackingRoutine), MethodType.Enumerator)]
    private static void MakeBeamAimAtPlayerReset()
    {
        RM.mechController.playerCamera.transform.position = _playerPosition;
    }
    
}