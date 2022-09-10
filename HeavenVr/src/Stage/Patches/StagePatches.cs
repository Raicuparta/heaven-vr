using HarmonyLib;
using HeavenVr.ModSettings;
using UnityEngine;

namespace HeavenVr.Stage.Patches;

[HarmonyPatch]
public static class StagePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCamera), nameof(PlayerCamera.OnEnable))]
    private static void CreateStage(PlayerCamera __instance)
    {
        VrStage.Create(__instance.PlayerCam);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuCamera), nameof(MenuCamera.Start))]
    private static void CreateStage(MenuCamera __instance)
    {
        __instance.m_startColor = Color.black;
        VrStage.Create(__instance.cam);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MissionCompleteManager), nameof(MissionCompleteManager.Start))]
    private static void CreateStage(MissionCompleteManager __instance)
    {
        VrStage.Create(__instance.GetComponentInChildren<Camera>());
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShakePosition), nameof(ShakePosition.Start))]
    private static void CreateStage(ShakePosition __instance)
    {
        var camera = __instance.GetComponent<Camera>();
        if (camera)
        {
            VrStage.Create(camera);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CreditsPlayer), nameof(CreditsPlayer.PlayCreditsRoutine))]
    private static void CreateStage()
    {
        if (VrStage.Instance != null) return;

        if (Camera.allCameras.Length == 0)
        {
            Debug.LogError("Failed to find camera when creating stage for credits scene");
            return;
        }
        
        VrStage.Create(Camera.allCameras[0]);
    }
}