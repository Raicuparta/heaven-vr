using System.Collections.Generic;
using System.Linq;

namespace HeavenVr.ModSettings.Patches;

public static class SteamSettingsPatchHelper
{
    // Keeping this in a separate file to avoid issues on Xbox version.
    public static IEnumerable<MenuScreenOptionsPanel> GetSteamPanels(OptionsMenuTabManager tabManager)
    {
        return tabManager._panels.Select(panel => panel.panel);
    }
}