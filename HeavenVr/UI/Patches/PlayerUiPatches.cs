using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HeavenVr.UI.Patches;

[HarmonyPatch]
public static class PlayerUiPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerUI), nameof(PlayerUI.Start))]
    private static void FixPlayerUi(PlayerUI __instance)
    {
        Object.Destroy(__instance.Cam.GetComponent<CameraStackPriority>());
        Object.Destroy(__instance.Cam.GetComponent<UniversalAdditionalCameraData>());
        __instance.Cam.orthographicSize = 20;
        __instance.Cam.targetTexture = VrAssetLoader.VrUiRenderTexture;
        __instance.Cam.clearFlags = CameraClearFlags.Depth;
        __instance.Cam.depth = 1;
        __instance.Cam.cullingMask = LayerHelper.GetMask(GameLayer.UI);
        __instance.Cam.farClipPlane = 100;
        __instance.transform.localScale = Vector3.one * 3f;
        __instance.timerHolder.transform.localScale = Vector3.one * 0.05f;
        __instance.timerHolder.transform.localPosition = Vector3.up * 1.6f;
        __instance.demonCounterHolder.transform.localScale = Vector3.one * 1.5f;
        
        LayerHelper.SetLayerRecursive(__instance.gameObject, GameLayer.UI);
        UiTarget.PlayerHudCamera = __instance.Cam;
    }
}