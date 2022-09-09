using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine;

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
}