using HarmonyLib;
using HeavenVr.ModSettings;
using HeavenVr.Stage;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeavenVr.VrInput.Patches;

[HarmonyPatch]
public static class InputPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.IsPressed))]
    private static bool SetInputIsPressed(ref bool __result, InputAction __instance)
    {
        var binding = InputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.IsPressed;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasPressedThisFrame))]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasPerformedThisFrame))]
    private static bool SetInputWasPressed(ref bool __result, InputAction __instance)
    {
        var binding = InputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.WasPressedThisFrame;

        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasReleasedThisFrame))]
    private static bool SetPreviousBoolInputsReleased(ref bool __result, InputAction __instance)
    {
        var binding = InputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.WasReleasedThisFrame;
        
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirstPersonDrifter), nameof(FirstPersonDrifter.UpdateVelocity))]
    private static void UseVrMovementDirection(FirstPersonDrifter __instance)
    {
        if (!VrStage.Instance || !VrStage.Instance.transform.parent) return;

        var forward = VrStage.Instance.GetMovementDirection();
        var rotation = Quaternion.FromToRotation(VrStage.Instance.transform.forward, forward);

        var input = new Vector3(__instance.inputX, 0, __instance.inputY);
        input = rotation * input;

        __instance.inputX = input.x;
        __instance.inputY = input.z;
    }
}