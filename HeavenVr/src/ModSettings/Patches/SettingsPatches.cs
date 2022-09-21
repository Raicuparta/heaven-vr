using System.Linq;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

namespace HeavenVr.ModSettings.Patches;

[HarmonyPatch]
public static class SettingsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntroCard), nameof(IntroCard.IsDone))]
    private static bool SkipIntro(ref bool __result)
    {
        if (!VrSettings.SkipIntro.Value) return true;

        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuScreenDialogue), nameof(MenuScreenDialogue.OnFastForwardButtonToggled))]
    private static void SpeedDialogueSkip(MenuScreenDialogue __instance)
    {
        __instance.ffTimescale = 20f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataManager), nameof(GameDataManager.OnReadPowerPrefsComplete))]
    private static void PreventSubmittingScores(GameDataManager __instance)
    {
        // Since the VR mod changes a lot of how the game works,
        // it wouldn't make sense to submit scores to the same leaderboard.
        // For now I'm just disabling score submission altogether.
        // TODO: submit vr scores to a separate leaderboard.
        GameDataManager.powerPrefs.dontUploadToLeaderboard = true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuTabManager), nameof(OptionsMenuTabManager.Start))]
    private static void CreateMenuEntries(OptionsMenuTabManager __instance)
    {
        var panels = __instance._panels.Select(panel => panel.panel).ToArray();

        var generalPanel = panels.First(panel => panel.name.StartsWith("General"));
        var controlsPanel = panels.First(panel => panel.name.StartsWith("Controls"));
        var audioPanel = panels.First(panel => panel.name.StartsWith("Audio"));
        var videoPanel = panels.First(panel => panel.name.StartsWith("Video"));

        var aimingAngleOffsetSlider = Object.Instantiate(controlsPanel._sliderPrefab, controlsPanel._settingsColumn.transform);

        var aimingAngleOffsetOptionEntry = new OptionsMenuPanelInformation.OptionEntry()
        {
            SettingType = OptionMenuSetting.AimAssist,
            SliderMaximum = VrSettings.MaxAngleOffset,
            SliderMinimum = -VrSettings.MaxAngleOffset,
            StepSize = 1,
        };

        var settingText = VrSettings.AimingAngleOffset.Description.Description.Split('|');
        var settingTitle = settingText[0];
        var settingDescription = settingText[1];

        aimingAngleOffsetSlider.Initialise(VrSettings.AimingAngleOffset.Value,
            settingTitle,
            aimingAngleOffsetOptionEntry,
            () => controlsPanel._TipWindow.SetWindowTip(settingTitle, settingDescription),
            () => controlsPanel._TipWindow.ResetWindow());
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.TryGetTranslation))]
    private static void PassThroughMissingTranslations(ref bool __result, string Term, ref string Translation)
    {
        // Usually missing translations cause errors to be thrown.
        // I'm making it so missing translations just return the translation key text.
        // This way I can just pass the replacement text as the key.
        // I should probably have my own dictionary instead, but this is easier.
        if (__result) return;

        Translation = Term;
        __result = true;
    }
    
}