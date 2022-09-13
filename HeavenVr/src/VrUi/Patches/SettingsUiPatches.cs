using HarmonyLib;
using UnityEngine;

namespace HeavenVr.VrUi.Patches;

[HarmonyPatch]
public static class SettingsUiPatches
{
    private static void DisableSettingInput(MonoBehaviour behaviour)
    {
        if (behaviour == null) return;

        behaviour.transform.gameObject.SetActive(false);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuScreenOptionsPanel), nameof(MenuScreenOptionsPanel.SpawnColumnSettings))]
    private static void DisableUnneededOptions(MenuScreenOptionsPanel __instance)
    {
        DisableSettingInput(__instance._aimAssistSlider);
        DisableSettingInput(__instance._FovSlider);
        DisableSettingInput(__instance._invertYAxisToggle);
        DisableSettingInput(__instance._lockOnToggle);
        DisableSettingInput(__instance._frameCapSelector);
        DisableSettingInput(__instance._vSyncToggle);
        DisableSettingInput(__instance._ambientOcclusionToggle);
        DisableSettingInput(__instance._antiAliasingSelector);
        DisableSettingInput(__instance._rebindControlsButton);
    }
}