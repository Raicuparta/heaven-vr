﻿using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private static Vector3 cameraParentPosition;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MechController), nameof(MechController.DoDiscardAbility))]
    private static void SetUpDiscardAbilityDirection(MechController __instance)
    {
        if (!VrAimLaser.Laser) return;

        // TODO dunno if I'm supposed to be able to dash upwards.
        var cameraTransform = __instance.playerCamera.transform;
        cameraRotation = cameraTransform.rotation;
        cameraPosition = cameraTransform.position;
        cameraParentRotation = cameraTransform.parent.rotation;
        cameraParentPosition = cameraTransform.parent.position;

        cameraTransform.rotation = cameraTransform.parent.rotation = VrAimLaser.Laser.rotation;
        cameraTransform.position = cameraTransform.parent.position = VrAimLaser.Laser.position;
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
        cameraTransform.parent.position = cameraParentPosition;
    }
    

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirstPersonDrifter), nameof(FirstPersonDrifter.Update))]
    private static void PreventRotatingCameraVertically(FirstPersonDrifter __instance)
    {
        __instance.m_cameraRotationX = __instance.m_cameraHolder.parent.localRotation;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FirstPersonDrifter), nameof(FirstPersonDrifter.Update))]
    private static void UpdateStageRotation()
    {
        if (!VrStage.Instance) return;
        VrStage.Instance.UpdateRotation();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void AddCanvasCollider(CanvasScaler __instance)
    {
        if (__instance.name.StartsWith("com.sinai")) return;
        
        var collider = __instance.GetComponent<BoxCollider>();
        if (collider != null) return;

        __instance.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var position = (Camera.main ? Camera.main.transform.position + Vector3.forward : Vector3.forward);
        __instance.transform.position = position;

        // var rectTransform = __instance.GetComponent<RectTransform>();
        // collider = __instance.gameObject.AddComponent<BoxCollider>();
        // var rectSize = rectTransform.sizeDelta;
        // collider.size = new Vector3(rectSize.x, rectSize.y, 0.1f);
        // __instance.gameObject.layer = LayerMask.NameToLayer("UI"); // TODO careful
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