using System.ComponentModel;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;

namespace HeavenVr;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCamera), "Start")]
    private static void EnableCameraTracking(PlayerCamera __instance)
    {
        var trackedPoseDriver = __instance.gameObject.AddComponent<TrackedPoseDriver>();
        trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanarReflectionRenderFeature), nameof(PlanarReflectionRenderFeature.Create))]
    private static void DisableReflections(PlanarReflectionRenderFeature __instance)
    {
        __instance.m_Settings.m_ReflectLayers = 0;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MouseLook), "Start")]
    private static void DrawAimLaser(MouseLook __instance)
    {
        VrAimLaser.Create(__instance.transform.parent);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void ScaleCanvas(CanvasScaler __instance)
    {
        if (!__instance.transform.parent) return;
        __instance.transform.parent.localScale = Vector3.one * 1.8f;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Camera), nameof(Camera.ViewportPointToRay), typeof(Vector3))]
    private static bool UseVrAimingDirection(ref Ray __result, Camera __instance)
    {
        if (!__instance.CompareTag("MainCamera") || !VrAimLaser.Instance) return true;

        __result = new Ray(VrAimLaser.Instance.transform.position, VrAimLaser.Instance.transform.forward);
        return false;

    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FirstPersonDrifter), "Start")]
    private static void SetUpDummyRotation(FirstPersonDrifter __instance)
    {
        var dummy = new GameObject("VrCameraRotationDummy").transform;
        dummy.SetParent(__instance.m_cameraHolder.parent);
        __instance.m_cameraHolder = dummy;
    }
}