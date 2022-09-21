using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private static void AddSlider(MenuScreenOptionsPanel panel, ConfigEntry<float> configEntry, float min, float max, float step)
    {
        var aimingAngleOffsetSlider = Object.Instantiate(panel._sliderPrefab, panel._settingsColumn.transform);

        var aimingAngleOffsetOptionEntry = new OptionsMenuPanelInformation.OptionEntry
        {
            SettingType = OptionMenuSetting.AimAssist,
            SliderMinimum = min,
            SliderMaximum = max,
            StepSize = step
        };

        var settingText = configEntry.Description.Description.Split('|');
        var settingTitle = settingText[0];
        var settingDescription = settingText[1];

        aimingAngleOffsetSlider.Initialise(configEntry.Value,
            settingTitle + " [{0}]",
            aimingAngleOffsetOptionEntry,
            () => panel._TipWindow.SetWindowTip(settingTitle, settingDescription),
            () => panel._TipWindow.ResetWindow());
        
        // There's an OnValueChange event that would happen immediately, but that can be problematic.
        // So I'm adding a pointer up event that only changes the setting once the user lets go of the trigger.
        var eventTrigger = aimingAngleOffsetSlider.gameObject.AddComponent<EventTrigger>();
        var pointerUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUp.callback.AddListener(_ => configEntry.Value = aimingAngleOffsetSlider.Value);
        eventTrigger.triggers.Add(pointerUp);
    }
    
    private static void AddToggle(MenuScreenOptionsPanel panel, ConfigEntry<bool> configEntry)
    {
        var aimingAngleOffsetSlider = Object.Instantiate(panel._togglePrefab, panel._settingsColumn.transform);

        var settingText = configEntry.Description.Description.Split('|');
        var settingTitle = settingText[0];
        var settingDescription = settingText[1];

        aimingAngleOffsetSlider.Initialise(configEntry.Value,
            settingTitle,
            () => panel._TipWindow.SetWindowTip(settingTitle, settingDescription),
            () => panel._TipWindow.ResetWindow());

        aimingAngleOffsetSlider.OnToggleValueChanged += value => configEntry.Value = value;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuTabManager), nameof(OptionsMenuTabManager.Start))]
    private static void CreateMenuEntries(OptionsMenuTabManager __instance)
    {
        var panels = __instance._panels.Select(panel => panel.panel).ToArray();

        var generalPanel = panels.First(panel => panel.name.StartsWith("General"));
        var controlsPanel = panels.First(panel => panel.name.StartsWith("Controls"));
        var videoPanel = panels.First(panel => panel.name.StartsWith("Video"));

        AddSlider(generalPanel,
            VrSettings.AimingAngleOffset,
            -VrSettings.MaxAngleOffset,
            VrSettings.MaxAngleOffset,
            0.5f);

        AddSlider(controlsPanel,
            VrSettings.TriggerSensitivity,
            0,
            VrSettings.MaxTriggerSensitivity,
            1f);

        AddToggle(controlsPanel, VrSettings.ControllerBasedMovementDirection);
        AddToggle(generalPanel, VrSettings.ShowPlayerBody);
        AddToggle(generalPanel, VrSettings.SkipIntro);
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