using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HeavenVr.Effects.Patches;

[HarmonyPatch]
public static class EffectPatches
{
    private static readonly string[] PostProcessingRemoveList =
    {
        "NW_MSVAO_Settings" // Ambient Occlusion just makes everything dark, removing it.
    };
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VolumeProfile), "OnEnable")]
    private static void DisableBlacklistedPostProcessinEffects(VolumeProfile __instance)
    {
        __instance.components.RemoveAll(component =>
            component is IPostProcessComponent && PostProcessingRemoveList.Contains(component.name));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VolumeComponent), "OnEnable")]
    private static void DisableSunFlares(VolumeComponent __instance)
    {
        if (__instance.GetType() != typeof(Beautify.Universal.Beautify)) return;
        
        var beautify = (Beautify.Universal.Beautify) __instance;
        
        beautify.sunFlaresIntensity.value = 0;
        beautify.bloomIntensity.value *= 0.3f;
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