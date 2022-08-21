using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeavenVr.Input.Patches;

[HarmonyPatch]
public static class Vector2InputPatches {
    [HarmonyTargetMethod]
    private static MethodInfo TargetMethod() {
        return typeof(InputAction).GetAnyMethod(nameof(InputAction.ReadValue)).MakeGenericMethod(typeof(Vector2));
    }

    [HarmonyPrefix]
    private static bool SetVector2Inputs(ref Vector2 __result, InputAction __instance)
    {   
        var binding = VrInputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.Position;
    
        return false;
    }
}