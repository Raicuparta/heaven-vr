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
    
    private static Vector3 GetMovementDirection()
    {
        // TODO use a laser for the movement direction.
        var trackedTransform = VrSettings.ControllerBasedMovementDirection.Value ? VrStage.Instance.movementDirectionPointer.transform : VrStage.Instance.VrCamera.transform;
        var forward = trackedTransform.forward;
        forward.y = 0;
        return forward;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirstPersonDrifter), nameof(FirstPersonDrifter.UpdateVelocity))]
    private static void UseVrMovementDirection(FirstPersonDrifter __instance)
    {
        if (!VrStage.Instance || !VrStage.Instance.VrCamera) return;

        var forward = GetMovementDirection();
        var rotation = Quaternion.FromToRotation(VrStage.Instance.transform.parent.forward, forward);

        var input = new Vector3(__instance.inputX, 0, __instance.inputY);
        input = rotation * input;

        __instance.inputX = input.x;
        __instance.inputY = input.z;
    }
}