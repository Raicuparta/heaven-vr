using HarmonyLib;
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
        if (!VrStage.Instance) return;

        var forward = VrStage.Instance.GetMovementDirection();
        var rotation = Quaternion.FromToRotation(VrStage.Instance.transform.parent.forward, forward);

        var input = new Vector3(__instance.inputX, 0, __instance.inputY);
        input = rotation * input;

        __instance.inputX = input.x;
        __instance.inputY = input.z;
    }    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInput),
        nameof(GameInput.GetKey),
        typeof(InputAction),
        typeof(int),
        typeof(int),
        typeof(int),
        typeof(float),
        typeof(bool))]
    private static bool UseVrBindingsInPrompts(ref string __result, InputAction inputAction,
        int fontsize,
        int bindingIndex = -1,
        int iconScale = 120,
        float iconOffset = -0.1f,
        bool tint = false)
    {
        var binding = InputMap.GetBinding(inputAction.name);

        if (binding == null) return true;

        __result = InputMap.GetBinding(inputAction.name).Name;
        return false;
    }

    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInput), nameof(GameInput.IsUsingGamepad))]
    private static bool ForceGamepadMode1(out bool __result)
    {
        __result = true;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInput), nameof(GameInput.GetDeviceClassFromPath))]
    [HarmonyPatch(typeof(GameInput), nameof(GameInput.GetDeviceClassFromControl))]
    private static bool ForceGamepadMode1(out GameInput.InputDeviceClass __result)
    {
        __result = GameInput.InputDeviceClass.Gamepad;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInput), nameof(GameInput.ApplySensitivityByInputDeviceClass))]
    private static void ForceGamepadMode2(out GameInput.InputDeviceClass inputDeviceClass)
    {
        inputDeviceClass = GameInput.InputDeviceClass.Gamepad;
    }
}