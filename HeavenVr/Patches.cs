using System.Linq;
using Beautify.Universal;
using HarmonyLib;
using HeavenVr.Stage;
using LIV.SDK.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace HeavenVr;

[HarmonyPatch]
public class Patches: HeavenVrPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanarReflectionRenderFeature), nameof(PlanarReflectionRenderFeature.Create))]
    private static void DisableReflections(PlanarReflectionRenderFeature __instance)
    {
        __instance.m_Settings.m_ReflectLayers = 0;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BaseCRTEffect), nameof(BaseCRTEffect.Awake))]
    private static void DisableCrtEffect(BaseCRTEffect __instance)
    {
        __instance.enabled = false;
        __instance.mainCamera.enabled = false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Camera), nameof(Camera.ViewportPointToRay), typeof(Vector3))]
    private static void UseVrAimingDirection(Vector3 pos, ref Ray __result, Camera __instance)
    {
        if (!__instance.CompareTag("MainCamera") || !VrStage.Instance || !VrStage.Instance.aimLaser) return;

        var laserTransform = VrStage.Instance.aimLaser.transform;
        
        var centerDirection = __instance.ViewportPointToRay(Vector3.one * 0.5f, Camera.MonoOrStereoscopicEye.Mono).direction;
        var originalDirection = __result.direction;
        var spreadOffset = originalDirection - centerDirection;
        var newDirection = laserTransform.forward + spreadOffset;
        
        __result = new Ray(laserTransform.position, newDirection);
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
    [HarmonyPatch(typeof(FirstPersonDrifter), nameof(FirstPersonDrifter.ForceDash))]
    private static void PreventRotatingCameraVertically(ref Vector3 newDashDirection, ref Vector3 newDashEndVelocity)
    {
        if (!VrStage.Instance || !VrStage.Instance.aimLaser || !RM.mechController) return;

        float endVelocity;
        var direction = VrStage.Instance.aimLaser.transform.forward;
        
        switch (RM.mechController.m_lastDiscardAbility)
        {
            case PlayerCardData.DiscardAbility.Dash:
            {
                endVelocity = RM.mechController.abilityDashEndVelocity;
                direction.y = 0;
                break;
            }
            case PlayerCardData.DiscardAbility.ShieldBash:
            {
                endVelocity = RM.mechController.abilityShieldBashEndVelocity;
                direction.y = 0;
                break;
            }
            case PlayerCardData.DiscardAbility.Fireball:
            {
                endVelocity = RM.mechController.abilityFireballEndVelocity;
                break;
            }
            default:
            {
                return;
            }
        }
        
        newDashDirection = direction;
        newDashEndVelocity = direction * endVelocity;
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

    private static readonly string[] ProjectileIds = {
        "Projectiles/ProjectileBomb",
        "Projectiles/ProjectileMine",
        "Projectiles/ProjectileRocketFast"
    };
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.CreateProjectile), typeof(string), typeof(Vector3),
        typeof(Vector3), typeof(ProjectileWeapon))]
    private static void AimProjectilesWithVrLaser(string path, ref Vector3 origin, ref Vector3 forward)
    {
        if (!VrStage.Instance || !VrStage.Instance.aimLaser || !ProjectileIds.Contains(path)) return;
        
        origin = VrStage.Instance.aimLaser.transform.position;
        forward = VrStage.Instance.aimLaser.transform.forward;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TargetAssist), nameof(TargetAssist.LoadPrefs))]
    private static bool ForceDisableTargetAssist(TargetAssist __instance)
    {
        __instance.MasterAssistIntensity = 0;
        return false;
    }
}