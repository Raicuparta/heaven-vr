﻿using BepInEx.Configuration;

namespace HeavenVr.ModSettings;

public static class VrSettings
{
    public enum AxisModeOption
    {
        Auto,
        Touch,
        Click
    }

    public enum ControlSchemeOption
    {
        Auto,
        Oculus,
        Vive,
        Index,
        Wmr
    }

    public enum TurningModeValue
    {
        // Unused members because they're used as settings values.
        // ReSharper disable UnusedMember.Global
        Smooth = 0,
        Snap23 = 23,
        Snap30 = 30,
        Snap45 = 45,
        Snap60 = 60,
        Snap90 = 90
        // ReSharper restore UnusedMember.Global
    }

    private const string ControlsCategory = "Controls";
    private const string MiscCategory = "Misc";
    public const float MaxTriggerSensitivity = 100;
    public const float MaxAngleOffset = 45;

    public static ConfigFile Config { get; private set; }
    public static ConfigEntry<bool> ControllerBasedMovementDirection { get; private set; }
    public static ConfigEntry<ControlSchemeOption> ControlScheme { get; private set; }
    public static ConfigEntry<AxisModeOption> AxisMode { get; private set; }
    public static ConfigEntry<float> TriggerSensitivity { get; private set; }
    public static ConfigEntry<float> AimingAngleOffset { get; private set; }
    public static ConfigEntry<bool> ShowPlayerBody { get; private set; }
    public static ConfigEntry<bool> SkipIntro { get; private set; }
    public static ConfigEntry<bool> LeftHandedMode { get; private set; }
    public static ConfigEntry<bool> SwapSticks { get; private set; }
    public static ConfigEntry<TurningModeValue> TurningMode { get; private set; }
    public static ConfigEntry<float> AimSmoothing { get; private set; }

    public static void SetUp(ConfigFile config)
    {
        Config = config;

        ControlScheme = config.Bind(ControlsCategory, nameof(ControlScheme), ControlSchemeOption.Auto,
            "Control scheme|Pick a control scheme manually or select auto to select it based on the detected controllers.");

        TurningMode = config.Bind(ControlsCategory, nameof(TurningMode), TurningModeValue.Smooth,
            "Turning mode|Pick between smooth or snap turning. Smooth turning speed can be configured in the Neon White settings menu.");

        AxisMode = config.Bind(ControlsCategory, nameof(AxisMode), AxisModeOption.Auto,
            "Axis mode|Touch is better for thumb sticks. Click is better for touch pads. Auto tries to pick the best for your controller.");
        
        AimSmoothing = config.Bind(ControlsCategory, nameof(AimSmoothing), 0.5f,
            new ConfigDescription("Aim smoothing|0 means no smoothing. 1 means a lot of smoothing",
                new AcceptableValueRange<float>(0f, 1f)));

        ControllerBasedMovementDirection = config.Bind(ControlsCategory, nameof(ControllerBasedMovementDirection),
            false,
            "Controller-based movement direction|Enabled: controller-based direction. Disabled: head-based direction.");

        LeftHandedMode = config.Bind(ControlsCategory, nameof(LeftHandedMode),
            false,
            "Left-handed mode|Swaps everything in each hand, except the movement/rotation sticks.");
        
        SwapSticks = config.Bind(ControlsCategory, nameof(SwapSticks),
            false,
            "Swap movement/rotation hands|Swaps the thumbsticks/touchpads used for movement and rotation.");

        TriggerSensitivity = config.Bind(ControlsCategory, nameof(TriggerSensitivity), 0f,
            new ConfigDescription(
                "Trigger Sensitivity|Leave at zero to use the default \"trigger click\" sensitivity.",
                new AcceptableValueRange<float>(0f, MaxTriggerSensitivity)));

        AimingAngleOffset = config.Bind(ControlsCategory, nameof(AimingAngleOffset), 0f,
            new ConfigDescription(
                "Aiming laser angle offset|Offset in degrees to apply to the aiming laser (positive is up, negative is down)",
                new AcceptableValueRange<float>(-MaxAngleOffset, MaxAngleOffset)));

        ShowPlayerBody = config.Bind(MiscCategory, nameof(ShowPlayerBody), false,
            "Show player body|It's pretty broken, so mostly useful for LIV (third person capture).");

        SkipIntro = config.Bind(MiscCategory, nameof(SkipIntro), false, "Skip game intro cutscene|Yes I know you're called Neons bla bla bla.");
    }
}