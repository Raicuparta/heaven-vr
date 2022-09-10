using HarmonyLib;
using UnityEngine;
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIPointer), nameof(UIPointer.Awake))]
    private static void RemoveMousePointer(UIPointer __instance)
    {
        Object.Destroy(__instance.transform.parent.gameObject);
    }
}