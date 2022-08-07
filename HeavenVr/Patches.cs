using System.Reflection;
using Beautify.Universal;
using HarmonyLib;
using LIV.SDK.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HeavenVr;

[HarmonyPatch]
public class Patches: HeavenVrPatch
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
        var stage = VrStage.Create(__instance.cam);
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
    [HarmonyPatch(typeof(PlayerUI), nameof(PlayerUI.Start))]
    private static void FixPlayerUi(PlayerUI __instance)
    {
        Object.Destroy(__instance.Cam.GetComponent<CameraStackPriority>());
        Object.Destroy(__instance.Cam.GetComponent<UniversalAdditionalCameraData>());
        __instance.Cam.orthographicSize = 20;
        __instance.Cam.targetTexture = VrAssetLoader.VrUiRenderTexture;
        __instance.Cam.clearFlags = CameraClearFlags.Nothing;
        __instance.Cam.depth = 1;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Camera), nameof(Camera.ViewportPointToRay), typeof(Vector3))]
    private static bool UseVrAimingDirection(ref Ray __result, Camera __instance)
    {
        if (!__instance.CompareTag("MainCamera") || !VrStage.Instance || !VrStage.Instance.AimLaser) return true;

        __result = new Ray(VrStage.Instance.AimLaser.transform.position, VrStage.Instance.AimLaser.transform.forward);
        return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.SetVisualOffsetAmount))]
    private static bool AdjustProjectileVisualOffset(ProjectileBase __instance)
    {
        __instance._projectileModelHolder.localPosition = Vector3.forward * 1.5f;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.OnSpawn))]
    private static void AdjustProjectileCameraOffset(ProjectileBase __instance)
    {
        Debug.Log($"_spawnCameraOffset: {__instance._spawnCameraOffset}");
        __instance._spawnCameraOffset = Vector3.zero;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirstPersonDrifter), nameof(FirstPersonDrifter.ForceDash))]
    private static void PreventRotatingCameraVertically(ref Vector3 newDashDirection, ref Vector3 newDashEndVelocity)
    {
        if (!VrStage.Instance || !VrStage.Instance.AimLaser || !RM.mechController) return;

        var direction = VrStage.Instance.AimLaser.transform.forward;
        direction.y = 0;
        newDashDirection = direction;
        newDashEndVelocity = direction * RM.mechController.abilityDashEndVelocity;
        
    }
    
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
        // VrUi.Create(__instance.transform.Find("Map"), 0.5f);
        // __instance.transform.localScale *= 0.5f;

    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void AddCanvasCollider(CanvasScaler __instance)
    {
        VrUi.Create(__instance.transform);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.CreateProjectile), typeof(string), typeof(Vector3),
        typeof(Vector3), typeof(ProjectileWeapon))]
    private static void AimProjectilesWithVrLaser(ref Vector3 origin, ref Vector3 forward)
    {
        if (!VrStage.Instance || !VrStage.Instance.AimLaser) return;

        Debug.Log("aiming");

        
        
        origin = VrStage.Instance.AimLaser.transform.position;
        forward = VrStage.Instance.AimLaser.transform.forward;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VolumeProfile), "OnEnable")]
    private static void DisablePostProcessing(VolumeProfile __instance)
    {
        // Post processing is only rendering in one eye. Disabling it for now.
        __instance.components.RemoveAll(component => component is IPostProcessComponent);
    }
    
    /*
     * Couldn't figure out how to add a new rendering pass, so I'm patching an existing rendering path instead.
     * The missing link to adding new rendering passes seems to be ForwardRendererData.m_RendererFeatureMap.
     * m_RendererFeatureMap is set in the Unity Editor and the value seems to be based on a file, so I couldn't figure
     * out a way around it.
     * The class SDKUniversalRenderFeature is only used for storing the passes property, which is accessed in LIV code.
     */
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BeautifyRendererFeature), nameof(BeautifyRendererFeature.AddRenderPasses))]
    private static void AddLivRenderPasses(BeautifyRendererFeature __instance, ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var passes = SDKUniversalRenderFeature.passes;
        if (passes == null) return;
        while (passes.Count > 0)
        {
            renderer.EnqueuePass(passes[0]);
            passes.RemoveAt(0);
        }
    }
    
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntroCard), nameof(IntroCard.IsDone))]
    private static bool SkipIntro(ref bool __result)
    {
        __result = true;
        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EventSystem), "OnEnable")]
    private static void AddLaserInputModule(EventSystem __instance)
    {
        if (__instance.name.Contains("UniverseLib")) return;

        LaserInputModule.Create(__instance);
        DefaultInputModuleDisabler.Create(__instance);
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