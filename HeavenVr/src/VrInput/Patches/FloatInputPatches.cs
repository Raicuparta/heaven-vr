using System.Reflection;
using HarmonyLib;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using UnityEngine.InputSystem;

namespace HeavenVr.VrInput.Patches;

[HarmonyPatch]
public static class FloatInputPatches
{
    // ReSharper disable once UnusedMember.Local
    [HarmonyTargetMethod]
    private static MethodInfo TargetMethod()
    {
        return typeof(InputAction).GetAnyMethod(nameof(InputAction.ReadValue)).MakeGenericMethod(typeof(float));
    }

    [HarmonyPatch]
    [HarmonyPrefix]
    private static bool SetFloatInputs(ref float __result, InputAction __instance)
    {
        if (VrSettings.TurningMode.Value != VrSettings.TurningModeValue.Smooth &&
            __instance.name == "Look")
            return false;

        var binding = InputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.IsPressed ? 1 : 0;

        return false;
    }
}