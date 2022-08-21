using Beautify.Universal;
using HarmonyLib;
using LIV.SDK.Unity;
using UnityEngine.Rendering.Universal;

namespace HeavenVr.Liv.Patches;

[HarmonyPatch]
public static class LivPatches
{
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
}