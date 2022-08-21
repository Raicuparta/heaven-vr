using HarmonyLib;
using UnityEngine;

namespace HeavenVr.Stage.Patches;

[HarmonyPatch]
public static class StagePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCamera), nameof(PlayerCamera.Start))]
    private static void EnableCameraTracking(PlayerCamera __instance)
    {
        VrStage.Create(__instance.PlayerCam);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuCamera), nameof(MenuCamera.Start))]
    private static void EnableCameraTracking(MenuCamera __instance)
    {
        __instance.m_startColor = Color.black;
        VrStage.Create(__instance.cam);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MissionCompleteManager), nameof(MissionCompleteManager.Start))]
    private static void EnableCameraTracking(MissionCompleteManager __instance)
    {
        VrStage.Create(__instance.GetComponentInChildren<Camera>());
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShakePosition), nameof(ShakePosition.Start))]
    private static void EnableCameraTracking(ShakePosition __instance)
    {
        var camera = __instance.GetComponent<Camera>();
        if (camera)
        {
            VrStage.Create(camera);
        }
    }
}