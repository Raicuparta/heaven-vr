using BepInEx.Configuration;

namespace HeavenVr;

public static class VrSettings
{
    public enum ControlSchemeOption
    {
        Auto,
        Oculus,
        Vive,
        Index,
        Wmr,
    }

    private const string controlsCategory = "Controls";

    public static ConfigFile Config { get; private set; }
    public static ConfigEntry<bool> ControllerBasedMovementDirection { get; private set; }
    public static ConfigEntry<ControlSchemeOption> ControlScheme { get; private set; }

    public static void SetUp(ConfigFile config)
    {
        Config = config;
        ControlScheme = config.Bind(controlsCategory, "ControlScheme", ControlSchemeOption.Auto,
            "Control Scheme | Pick a control scheme manually or select auto to select it based on the detected controllers.");
        ControllerBasedMovementDirection = config.Bind(controlsCategory, "ControllerBasedMovementDirection", false,
            "Controller-based movement direction | Enabled: controller-based direction. Disabled: head-based direction.");
    }
}