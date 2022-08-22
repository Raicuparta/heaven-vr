﻿using HarmonyLib;
using UnityEngine;

namespace HeavenVr.VrCamera.Patches;

[HarmonyPatch]
public static class CameraPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FirstPersonDrifter), "Start")]
    private static void SetUpDummyRotation(FirstPersonDrifter __instance)
    {
        // I forgot to write a comment for this so I don't remember what it's for.
        var dummy = new GameObject("VrCameraRotationDummy").transform;
        dummy.SetParent(__instance.m_cameraHolder.parent);
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
}