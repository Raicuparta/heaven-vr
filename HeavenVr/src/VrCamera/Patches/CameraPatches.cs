using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine;
using UnityEngine.Animations;

namespace HeavenVr.VrCamera.Patches;

[HarmonyPatch]
public static class CameraPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FirstPersonDrifter), "Start")]
    private static void SetUpDummyRotation(FirstPersonDrifter __instance)
    {
        // There's a lot of code that depends on this "cameraHolder", but needs it to be afttached to the player body.
        // There's also code that tries to modify the cameraHolder transform.
        // Since in VR the camera can be far from the body and can't be moved / rotated manually,
        // we replace the cameraHolder with a dummy object that's always attached.
        var dummy = new GameObject("VrCameraRotationDummy").transform;
        dummy.SetParent(__instance.m_cameraHolder.parent, false);
        dummy.transform.localPosition = Vector3.zero;
        dummy.transform.localRotation = Quaternion.identity;
        __instance.m_cameraHolder = dummy;
    }

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
}