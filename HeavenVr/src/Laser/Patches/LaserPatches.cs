using HarmonyLib;
using UnityEngine.EventSystems;

namespace HeavenVr.Laser.Patches;

[HarmonyPatch]
public static class LaserPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EventSystem), "OnEnable")]
    private static void AddLaserInputModule(EventSystem __instance)
    {
        if (__instance.name.Contains("UniverseLib")) return;

        LaserInputModule.Create(__instance);
        DefaultInputModuleDisabler.Create(__instance);
    }    
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TargetAssist), nameof(TargetAssist.LoadPrefs))]
    private static bool ForceDisableTargetAssist(TargetAssist __instance)
    {
        __instance.MasterAssistIntensity = 0;
        return false;
    }

    private static void ForceAimingSettings(MenuScreenOptionsPanel optionsPanel)
    {
        if (optionsPanel._lockOnToggle)
        {
            optionsPanel._lockOnToggle.Value = false;
        }

        if (optionsPanel._aimAssistSlider)
        {
            optionsPanel._aimAssistSlider.Value = 0;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuScreenOptionsPanel), nameof(MenuScreenOptionsPanel.ApplyChanges))]
    private static void ForceAimingSettingsPre(MenuScreenOptionsPanel __instance)
    {
        ForceAimingSettings(__instance);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuScreenOptionsPanel), nameof(MenuScreenOptionsPanel.LoadValues))]
    [HarmonyPatch(typeof(MenuScreenOptionsPanel), nameof(MenuScreenOptionsPanel.SpawnColumnSettings))]
    private static void ForceAimingSettingsPost(MenuScreenOptionsPanel __instance)
    {
        ForceAimingSettings(__instance);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GS), nameof(GS.LockOn))]
    private static void ForceAimingSettingsSet(out bool enable)
    {
        enable = false;
    }
}