using System.Reflection;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace HeavenVr.Input.Patches;

[HarmonyPatch]
public static class FloatInputPatches {
    [HarmonyTargetMethod]
    private static MethodInfo TargetMethod() {
        return typeof(InputAction).GetAnyMethod(nameof(InputAction.ReadValue)).MakeGenericMethod(typeof(float));
    }

    [HarmonyPrefix]
    private static bool SetFloatInputs(ref float __result, InputAction __instance)
    {
        var binding = InputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.IsPressed ? 1 : 0;
    
        return false;
    }
}