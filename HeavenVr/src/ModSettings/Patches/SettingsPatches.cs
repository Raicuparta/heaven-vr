using System;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

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
        var slider = Object.Instantiate(panel._sliderPrefab, panel._settingsColumn.transform);

        var optionEntry = new OptionsMenuPanelInformation.OptionEntry
        {
            SettingType = OptionMenuSetting.AimAssist,
            SliderMinimum = min,
            SliderMaximum = max,
            StepSize = step
        };

        var settingText = configEntry.Description.Description.Split('|');
        var settingTitle = settingText[0];
        var settingDescription = settingText[1];

        slider.Initialise(configEntry.Value,
            settingTitle + " [{0}]",
            optionEntry,
            () => panel._TipWindow.SetWindowTip(settingTitle, settingDescription),
            () => panel._TipWindow.ResetWindow());
        
        // There's an OnValueChange event that would happen immediately, but that can be problematic.
        // So I'm adding a pointer up event that only changes the setting once the user lets go of the trigger.
        var eventTrigger = slider.gameObject.AddComponent<EventTrigger>();
        var pointerUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUp.callback.AddListener(_ => configEntry.Value = slider.Value);
        eventTrigger.triggers.Add(pointerUp);

        configEntry.SettingChanged += (_, _) => slider.Value = configEntry.Value;
    }
    
    private static void AddToggle(MenuScreenOptionsPanel panel, ConfigEntry<bool> configEntry)
    {
        var toggle = Object.Instantiate(panel._togglePrefab, panel._settingsColumn.transform);

        var settingText = configEntry.Description.Description.Split('|');
        var settingTitle = settingText[0];
        var settingDescription = settingText[1];

        toggle.Initialise(configEntry.Value,
            settingTitle,
            () => panel._TipWindow.SetWindowTip(settingTitle, settingDescription),
            () => panel._TipWindow.ResetWindow());

        toggle.OnToggleValueChanged += value => configEntry.Value = value;

        configEntry.SettingChanged += (_, _) => toggle.Value = configEntry.Value;
    }

    private static int GetEnumIndex<TSettingValue>(TSettingValue value)
    {
        return Array.IndexOf(Enum.GetValues(typeof(TSettingValue)), value);
    }
    
    private static void AddSelect<TSettingValue>(MenuScreenOptionsPanel panel, ConfigEntry<TSettingValue> configEntry)
    {
        var enumValueNames = Enum.GetNames(typeof(TSettingValue)).ToList();
        
        Debug.Log($"Enum values for {typeof(TSettingValue)}: {string.Join(", ", enumValueNames)}");
        
        var select = Object.Instantiate(panel._stringListPrefab, panel._settingsColumn.transform);

        var settingText = configEntry.Description.Description.Split('|');
        var settingTitle = settingText[0];
        var settingDescription = settingText[1];

        select.SetTitleKey(settingTitle,
            () => panel._TipWindow.SetWindowTip(settingTitle, settingDescription),
            () => panel._TipWindow.ResetWindow(), null);
        
        select.SetStrings(enumValueNames, true);
        select.OnSelectorValueChanged += () =>
            configEntry.Value =
                (TSettingValue)Enum.Parse(typeof(TSettingValue), enumValueNames[select._currentStringIndex]);
        
        configEntry.SettingChanged += (_, _) => select.SetSelectionIndex(GetEnumIndex(configEntry.Value));
        
        select.SetSelectionIndex(GetEnumIndex(configEntry.Value));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuTabManager), nameof(OptionsMenuTabManager.Start))]
    private static void CreateMenuEntries(OptionsMenuTabManager __instance)
    {
        var panels = __instance._panels.Select(panel => panel.panel).ToArray();

        var generalPanel = panels.First(panel => panel.name.StartsWith("General"));
        var controlsPanel = panels.First(panel => panel.name.StartsWith("Controls"));

        AddSlider(generalPanel,
            VrSettings.AimingAngleOffset,
            -VrSettings.MaxAngleOffset,
            VrSettings.MaxAngleOffset,
            0.5f);

        AddSlider(controlsPanel, VrSettings.TriggerSensitivity, 0, VrSettings.MaxTriggerSensitivity, 1f);

        AddToggle(controlsPanel, VrSettings.ControllerBasedMovementDirection);
        AddToggle(generalPanel, VrSettings.ShowPlayerBody);
        AddToggle(generalPanel, VrSettings.SkipIntro);

        AddSelect(controlsPanel, VrSettings.AxisMode);
        AddSelect(controlsPanel, VrSettings.ControlScheme);
        AddSelect(controlsPanel, VrSettings.TurningMode);
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
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MenuScreenOptionsPanel), nameof(MenuScreenOptionsPanel.SpawnColumnSettings))]
    private static void RemoveControlsSettingsSpacers(MenuScreenOptionsPanel __instance)
    {
        // These spacers are unnecessary after removing most of the original game's controls settings.
        foreach (var spacer in __instance._spacerPrefabs)
        {
            spacer.SetActive(false);
        }
    }
    
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataManager), nameof(GameDataManager.DeletePrefs))]
    private static void ResetModSettingsOnResettingGameSettings()
    {
        foreach (var (_, value) in VrSettings.Config)
        {
            value.BoxedValue = value.DefaultValue;
        }
    }
    
    
}