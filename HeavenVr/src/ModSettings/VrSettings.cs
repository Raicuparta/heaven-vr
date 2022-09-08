using BepInEx.Configuration;

namespace HeavenVr.ModSettings;

public static class VrSettings
{
    public enum ControlSchemeOption
    {
        Auto,
        Oculus,
        Vive,
        Index,
        Wmr
    }
    
    public enum AxisModeOption
    {
        Auto,
        Touch,
        Click
    }

    private const string ControlsCategory = "Controls";
    private const string VisualsCategory = "Visuals";
    public const int MaxTriggerSensitivity = 100;

    public static ConfigFile Config { get; private set; }
    public static ConfigEntry<bool> ControllerBasedMovementDirection { get; private set; }
    public static ConfigEntry<ControlSchemeOption> ControlScheme { get; private set; }
    public static ConfigEntry<AxisModeOption> AxisMode { get; private set; }
    public static ConfigEntry<int> TriggerSensitivity { get; private set; }
    public static ConfigEntry<bool> ShowPlayerBody { get; private set; }

    public static void SetUp(ConfigFile config)
    {
        Config = config;

        ControlScheme = config.Bind(ControlsCategory, "ControlScheme", ControlSchemeOption.Auto,
            "Control scheme | Pick a control scheme manually or select auto to select it based on the detected controllers.");

        AxisMode = config.Bind(ControlsCategory, "AxisMode", AxisModeOption.Auto,
            "Axis mode | Touch is better for thumb sticks. Click is better for touch pads. Auto tries to pick the best for your controller.");

        ControllerBasedMovementDirection = config.Bind(ControlsCategory, "ControllerBasedMovementDirection", false,
            "Controller-based movement direction | Enabled: controller-based direction. Disabled: head-based direction.");

        TriggerSensitivity = config.Bind(ControlsCategory, "TriggerSensitivity", 0,
            new ConfigDescription(
                "Trigger Sensitivity | Leave at zero to use the default \"trigger click\" sensitivity.",
                new AcceptableValueRange<int>(0, MaxTriggerSensitivity)));

        ShowPlayerBody = config.Bind(VisualsCategory, "ShowPlayerBody", false, "Show player body | It's pretty broken, so mostly useful for LIV");
    }
}