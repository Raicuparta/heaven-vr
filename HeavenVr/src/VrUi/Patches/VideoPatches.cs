using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine.Video;

namespace HeavenVr.VrUi.Patches;

[HarmonyPatch]
public static class VideoPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VideoPlayer), "Play")]
    private static void RenderVideoToVrUi(VideoPlayer __instance)
    {
        if (VrStage.Instance == null) return;

        __instance.renderMode = VideoRenderMode.MaterialOverride;
        __instance.targetMaterialRenderer = VrStage.Instance.UiTarget.GetQuadRenderer();
    }
}