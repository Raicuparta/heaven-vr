using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace HeavenVr;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCamera), "Start")]
    private static void EnableCameraTracking(PlayerCamera __instance)
    {
        var trackedPoseDriver = __instance.gameObject.AddComponent<TrackedPoseDriver>();
        trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanarReflectionRenderFeature), nameof(PlanarReflectionRenderFeature.Create))]
    private static void DisableReflections(PlanarReflectionRenderFeature __instance)
    {
        __instance.m_Settings.m_ReflectLayers = 0;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MouseLook), "Start")]
    private static void DrawAimLaser(MouseLook __instance)
    {
        VrAimLaser.Create(__instance.transform.parent);
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
        if (!__instance.CompareTag("MainCamera") || !VrAimLaser.Instance) return true;

        __result = new Ray(VrAimLaser.Instance.transform.position, VrAimLaser.Instance.transform.forward);
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
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasPressedThisFrame))]
    private static bool SetBoolInputsPressed(ref bool __result, InputAction __instance)
    {
        var binding = VrInputMap.GetBoolBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.WasPressedThisFrame;

        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputAction), nameof(InputAction.WasReleasedThisFrame))]
    private static bool SetPreviousBoolInputsReleased(ref bool __result, InputAction __instance)
    {
        var binding = VrInputMap.GetBoolBinding(__instance.name);
        if (binding == null) return true;

        __result = binding.WasReleasedThisFrame;
        
        return false;
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
            var binding = VrInputMap.GetVector2Binding(__instance.name);
            if (binding == null) return true;
            
            __result = binding.GetValue();
    
            return false;
        }
    }

    private static Quaternion cameraRotation;
    private static Vector3 cameraPosition;
    private static Quaternion cameraParentRotation;
    private static Vector3 cameraParentPosition;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MechController), nameof(MechController.DoDiscardAbility))]
    private static void SetUpDiscardAbilityDirection(MechController __instance)
    {
        if (!VrAimLaser.Instance) return;

        // TODO dunno if I'm supposed to be able to dash upwards.
        var cameraTransform = __instance.playerCamera.transform;
        cameraRotation = cameraTransform.rotation;
        cameraPosition = cameraTransform.position;
        cameraParentRotation = cameraTransform.parent.rotation;
        cameraParentPosition = cameraTransform.parent.position;

        cameraTransform.rotation = cameraTransform.parent.rotation = VrAimLaser.Instance.transform.rotation;
        cameraTransform.position = cameraTransform.parent.position = VrAimLaser.Instance.transform.position;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MechController), nameof(MechController.DoDiscardAbility))]
    private static void ResetDiscardAbilityDirection(MechController __instance)
    {
        if (!VrAimLaser.Instance) return;

        var cameraTransform = __instance.playerCamera.transform;
        cameraTransform.rotation = cameraRotation;
        cameraTransform.position = cameraPosition;
        cameraTransform.parent.rotation = cameraParentRotation;
        cameraTransform.parent.position = cameraParentPosition;
    }
    
    [HarmonyPatch]
    public static class FloatInputPatches {
        [HarmonyTargetMethod]
        private static MethodInfo TargetMethod() {
            return typeof(InputAction).GetAnyMethod(nameof(InputAction.ReadValue)).MakeGenericMethod(typeof(float));
        }

        [HarmonyPrefix]
        private static bool SetVector2Inputs(ref float __result, InputAction __instance)
        {
            var binding = VrInputMap.GetBoolBinding(__instance.name);
            if (binding == null) return true;

            __result = binding.GetValue() ? 1 : 0;
    
            return false;
        }
    }
}