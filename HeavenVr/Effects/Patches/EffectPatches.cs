using HarmonyLib;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HeavenVr.Effects.Patches;

[HarmonyPatch]
public abstract class EffectPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VolumeProfile), "OnEnable")]
    private static void DisablePostProcessing(VolumeProfile __instance)
    {
        // Post processing is only rendering in one eye. Disabling until I find a solution.
        __instance.components.RemoveAll(component => component is IPostProcessComponent);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BaseCRTEffect), nameof(BaseCRTEffect.Awake))]
    private static void DisableCrtEffect(BaseCRTEffect __instance)
    {
        __instance.enabled = false;
        __instance.mainCamera.enabled = false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanarReflectionRenderFeature), nameof(PlanarReflectionRenderFeature.Create))]
    private static void DisableReflections(PlanarReflectionRenderFeature __instance)
    {
        // Reflections are broken in VR. Disabling until I find a solution.
        __instance.m_Settings.m_ReflectLayers = 0;
    }
}