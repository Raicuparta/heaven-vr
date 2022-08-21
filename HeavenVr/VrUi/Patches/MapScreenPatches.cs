using HarmonyLib;
using HeavenVr.Helpers;

namespace HeavenVr.VrUi.Patches;

[HarmonyPatch]
public static class MapScreenPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuScreenMapAesthetics), nameof(MenuScreenMapAesthetics.Start))]
    private static void FixMapScreen(MenuScreenMapAesthetics __instance)
    {
        LayerHelper.SetLayerRecursive(__instance.gameObject, GameLayer.UI);
    }
}