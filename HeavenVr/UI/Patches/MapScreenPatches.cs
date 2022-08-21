using HarmonyLib;

namespace HeavenVr.UI.Patches;

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