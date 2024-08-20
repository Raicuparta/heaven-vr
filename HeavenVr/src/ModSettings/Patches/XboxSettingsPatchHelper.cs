using System;
using System.Collections.Generic;
using HeavenVr.Helpers;
using UnityEngine;

namespace HeavenVr.ModSettings.Patches;

public static class XboxSettingsPatchHelper
{
    public static IEnumerable<MenuScreenOptionsPanel> GetXboxPanels(OptionsMenuTabManager tabManager)
    {
        try
        {
            // Xbox version (or maybe any other future versions).
            // Force it to a different type because this project uses the Steam version as a dependency,
            // so the type at compile time isn't the same as the type at runtime.
            return tabManager.GetValue<List<MenuScreenOptionsPanel>>("_panels");
        }
        catch (Exception exception)
        {
            Debug.Log($"Failed to get settings panels with Xbox method. This is OK and expected on the Steam version. Error: {exception}");
            // Ignore error if it's not found, just return null and continue.
            return null;
        }
    }
}