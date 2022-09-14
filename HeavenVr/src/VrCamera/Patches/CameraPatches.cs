using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HeavenVr.VrCamera.Patches;

[HarmonyPatch]
public static class CameraPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Application), nameof(Application.targetFrameRate), MethodType.Setter)]
    private static bool ForceDisableTargetFramerateSet(out int value)
    {
        value = -1;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Application), nameof(Application.targetFrameRate), MethodType.Getter)]
    private static bool ForceDisableTargetFramerateGet(out int __result)
    {
        __result = -1;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeadBob), nameof(HeadBob.Start))]
    private static bool DisableHeadBob(HeadBob __instance)
    {
        __instance.enabled = false;
        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCamera), nameof(PlayerCamera.OnEnable))]
    private static void DisableAntialiasing(PlayerCamera __instance)
    {
        var additionalData = __instance.GetComponent<UniversalAdditionalCameraData>();
        if (additionalData == null) return;

        additionalData.antialiasing = AntialiasingMode.None;
        additionalData.renderPostProcessing = false;
    }
}