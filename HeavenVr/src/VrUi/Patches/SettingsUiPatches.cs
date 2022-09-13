using HarmonyLib;
using UnityEngine;

namespace HeavenVr.VrUi.Patches;

[HarmonyPatch]
public static class SettingsUiPatches
{
    const string TurningSpeedSettingText = "Turning speed";

    private static void DisableSettingInput(Component component)
    {
        if (component == null) return;

        component.transform.gameObject.SetActive(false);
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
        DisableSettingInput(__instance._mouseSensitivitySlider);
        DisableSettingInput(__instance._gamepadSensitivityVerticalSlider);
        DisableSettingInput(__instance._responseCurveSelector);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuScreenOptionsPanel), nameof(MenuScreenOptionsPanel.SpawnColumnSettings))]
    private static void ChangeMouseSensitivitySettingName(MenuScreenOptionsPanel __instance)
    {
        if (!__instance._mouseSensitivitySlider) return;
        
        var titleText = __instance._mouseSensitivitySlider._titleText;
        if (titleText.unityUIText)
        {
            titleText.unityUIText.text = TurningSpeedSettingText;
        }
        if (titleText.textMesh)
        {
            titleText.textMesh.text = TurningSpeedSettingText;
        }
        if (titleText.textMeshPro)
        {
            titleText.textMeshPro.text = TurningSpeedSettingText;
        }
        if (titleText.textMeshProUGUI)
        {
            titleText.textMeshProUGUI.text = TurningSpeedSettingText;
        }
    }
}