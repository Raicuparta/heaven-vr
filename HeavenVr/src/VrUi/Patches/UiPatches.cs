using HarmonyLib;
using UnityEngine.UI;

namespace HeavenVr.VrUi.Patches;

[HarmonyPatch]
public static class UiPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void AddCanvasCollider(CanvasScaler __instance)
    {
        VrUi.Create(__instance.transform);
    }
}