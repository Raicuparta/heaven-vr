﻿using System.Reflection;
using HarmonyLib;
using HeavenVr.Helpers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeavenVr.VrInput.Patches;

[HarmonyPatch]
public static class Vector2InputPatches {
    // ReSharper disable once UnusedMember.Local
    [HarmonyTargetMethod]
    private static MethodInfo TargetMethod() {
        return typeof(InputAction).GetAnyMethod(nameof(InputAction.ReadValue)).MakeGenericMethod(typeof(Vector2));
    }

    [HarmonyPatch]
    [HarmonyPrefix]
    private static bool SetVector2Inputs(ref Vector2 __result, InputAction __instance)
    {   
        var binding = InputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.Position;
    
        return false;
    }
}