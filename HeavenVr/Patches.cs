using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace HeavenVr;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCamera), nameof(PlayerCamera.Start))]
    private static void EnableCameraTracking(PlayerCamera __instance)
    {
        VrStage.Create(__instance.PlayerCam);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuCamera), nameof(MenuCamera.Start))]
    private static void EnableCameraTracking(MenuCamera __instance)
    {
        VrStage.Create(__instance.cam);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShakePosition), nameof(ShakePosition.Start))]
    private static void EnableCameraTracking(ShakePosition __instance)
    {
        var camera = __instance.GetComponent<Camera>();
        if (camera)
        {
            VrStage.Create(camera);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanarReflectionRenderFeature), nameof(PlanarReflectionRenderFeature.Create))]
    private static void DisableReflections(PlanarReflectionRenderFeature __instance)
    {
        __instance.m_Settings.m_ReflectLayers = 0;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void ScaleCanvas(CanvasScaler __instance)
    {
        if (!__instance.transform.parent) return;
        __instance.transform.parent.localScale = Vector3.one * 1.8f;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Camera), nameof(Camera.ViewportPointToRay), typeof(Vector3))]
    private static bool UseVrAimingDirection(ref Ray __result, Camera __instance)
    {
        if (!__instance.CompareTag("MainCamera") || !VrAimLaser.Laser) return true;

        __result = new Ray(VrAimLaser.Laser.position, VrAimLaser.Laser.forward);
        return false;

    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FirstPersonDrifter), "Start")]
    private static void SetUpDummyRotation(FirstPersonDrifter __instance)
    {
        var dummy = new GameObject("VrCameraRotationDummy").transform;
        dummy.SetParent(__instance.m_cameraHolder.parent);
        __instance.m_cameraHolder = dummy;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.IsPressed))]
    private static bool SetInputIsPressed(ref bool __result, InputAction __instance)
    {
        var binding = VrInputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.IsPressed;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasPressedThisFrame))]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasPerformedThisFrame))]
    private static bool SetInputWasPressed(ref bool __result, InputAction __instance)
    {
        var binding = VrInputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.WasPressedThisFrame;

        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasReleasedThisFrame))]
    private static bool SetPreviousBoolInputsReleased(ref bool __result, InputAction __instance)
    {
        var binding = VrInputMap.GetBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.WasReleasedThisFrame;
        
        return false;
    }

    private static Quaternion cameraRotation;
    private static Vector3 cameraPosition;
    private static Quaternion cameraParentRotation;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MechController), nameof(MechController.DoDiscardAbility))]
    private static void SetUpDiscardAbilityDirection(MechController __instance)
    {
        if (!VrAimLaser.Laser) return;

        var cameraTransform = __instance.playerCamera.transform;
        cameraRotation = cameraTransform.rotation;
        cameraPosition = cameraTransform.position;
        cameraParentRotation = cameraTransform.parent.rotation;

        cameraTransform.position = VrAimLaser.Laser.position;
        cameraTransform.rotation = VrAimLaser.Laser.rotation;
        cameraTransform.parent.rotation = quaternion.LookRotation(MathHelper.GetProjectedForward(cameraTransform), Vector3.up);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MechController), nameof(MechController.DoDiscardAbility))]
    private static void ResetDiscardAbilityDirection(MechController __instance)
    {
        if (!VrAimLaser.Laser) return;

        var cameraTransform = __instance.playerCamera.transform;
        cameraTransform.rotation = cameraRotation;
        cameraTransform.position = cameraPosition;
        cameraTransform.parent.rotation = cameraParentRotation;
    }
    

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(MouseLook), nameof(MouseLook.UpdateRotation))]
    // private static void PreventRotatingCameraVertically(MouseLook __instance)
    // {
    //     if (!VrStage.Instance) return;
    //     __instance.originalRotation *= Quaternion.Euler(0, VrStage.Instance.AngleDelta, 0);
    // }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Application), nameof(Application.targetFrameRate), MethodType.Setter)]
    private static bool SyncFramerateWithHmdSetter(ref int value)
    {
        value = -1;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Application), nameof(Application.targetFrameRate), MethodType.Getter)]
    private static bool SyncFramerateWithHmdGetter(ref int __result)
    {
        __result = -1;
        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuScreenMapAesthetics), nameof(MenuScreenMapAesthetics.Start))]
    private static void FixMapScreen(MenuScreenMapAesthetics __instance)
    {
        VrUi.Create(__instance.transform.Find("Map"), 0.5f);
        __instance.transform.localScale *= 0.5f;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void AddCanvasCollider(CanvasScaler __instance)
    {
        VrUi.Create(__instance.transform);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VolumeProfile), "OnEnable")]
    private static void DisablePostProcessing(VolumeProfile __instance)
    {
        // Post processing is only rendering in one eye. Disabling it for now.
        __instance.components.RemoveAll(component => component is IPostProcessComponent);
    }
    
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

    [HarmonyPatch]
    public static class FloatInputPatches {
        [HarmonyTargetMethod]
        private static MethodInfo TargetMethod() {
            return typeof(InputAction).GetAnyMethod(nameof(InputAction.ReadValue)).MakeGenericMethod(typeof(float));
        }

        [HarmonyPrefix]
        private static bool SetFloatInputs(ref float __result, InputAction __instance)
        {
            var binding = VrInputMap.GetBinding(__instance.name);
            if (binding == null) return true;

            __result = binding.IsPressed ? 1 : 0;
    
            return false;
        }
    }
}