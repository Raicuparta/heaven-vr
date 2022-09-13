using System;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public class InputManager: MonoBehaviour
{
    private static InputDevice _leftInputDevice;
    private static InputDevice _rightInputDevice;
    private const float SnapTurnThreshold = 0.1f;

    private void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        VrSettings.ControlScheme.SettingChanged += OnControlSchemeChanged;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        VrSettings.ControlScheme.SettingChanged -= OnControlSchemeChanged;
    }

    private static void OnDeviceConnected(InputDevice device)
    {
        if ((device.characteristics & InputDeviceCharacteristics.Controller) == 0) return;

        if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
        {
            _rightInputDevice = device;
        }
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
        {
            _leftInputDevice = device;
        }
        InputMap.UpdateInputMap(device);
    }

    private static void OnControlSchemeChanged(object sender, EventArgs e)
    {
        InputMap.UpdateInputMap(_rightInputDevice);
    }

    public static void Create()
    {
        new GameObject("VrInputManager").AddComponent<InputManager>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        InputMap.Update();
        UpdateSnapTurning();
    }

    private static void UpdateSnapTurning()
    {
        if (!RM.drifter || VrSettings.TurningMode.Value == VrSettings.TurningModeValue.Smooth) return;

        var lookBinding = InputMap.GetBinding("Look");

        if (lookBinding is not { WasPressedThisFrame: true }) return;
        
        var snapTurningAngle = (int)VrSettings.TurningMode.Value;
        var mouseLookX = RM.drifter.mouseLookX;

        switch (lookBinding.Position.x)
        {
            case > SnapTurnThreshold:
                mouseLookX.AddFrameRotation(snapTurningAngle, 0);
                break;
            case < -SnapTurnThreshold:
                mouseLookX.AddFrameRotation(-snapTurningAngle, 0);
                break;
        }
    }

    public static InputDevice GetInputDevice(XRNode hand)
    {
        return hand switch
        {
            XRNode.RightHand => _rightInputDevice,
            XRNode.LeftHand => _leftInputDevice,
            _ => throw new ArgumentOutOfRangeException(nameof(hand), hand, null)
        };
    }

    // TODO move all this input prompt code to another class.
    [CanBeNull]
    private static string GetGenericUsageName<T>(InputFeatureUsage<T> usage)
    {
        if (usage.name == CommonUsages.triggerButton.name)
        {
            return "Trigger";
        }
        if (usage.name == CommonUsages.gripButton.name)
        {
            return "Grip";
        }
        if (usage.name == CommonUsages.menuButton.name)
        {
            return "Menu Button";
        }
        if (usage.name == CommonUsages.primary2DAxis.name)
        {
            return "Thumbstick";
        }
        if (usage.name == CommonUsages.primary2DAxisClick.name)
        {
            return "Touchpad";
        }

        return null;
    }

    [CanBeNull]
    private static string GetIndexUsageName<T>(InputFeatureUsage<T> usage)
    {
        if (usage.name == CommonUsages.primaryButton.name)
        {
            return "A";
        }
        if (usage.name == CommonUsages.secondaryButton.name)
        {
            return "B";
        }

        return null;
    }

    [CanBeNull]
    private static string GetOculusUsageName<T>(InputFeatureUsage<T> usage, XRNode hand)
    {
        if (usage.name == CommonUsages.primaryButton.name)
        {
            return hand == XRNode.RightHand ? "A" : "X";
        }
        if (usage.name == CommonUsages.secondaryButton.name)
        {
            return hand == XRNode.RightHand ? "B" : "Y";
        }

        return null;
    }

    public static bool IsDevice(string deviceName)
    {
        return StringHelper.ContainsCaseInsensitive(_rightInputDevice.name, deviceName);
    }
    
    public static string GetUsageName<T>(InputFeatureUsage<T> usage, XRNode hand)
    {
        var usageName = GetGenericUsageName(usage);
        var handSufix = hand == XRNode.RightHand ? "Right " : "Left ";

        if (usageName == null)
        {
            if (IsDevice("index"))
            {
                usageName = GetIndexUsageName(usage);
            }
            if (IsDevice("oculus"))
            {
                usageName = GetOculusUsageName(usage, hand);
                handSufix = "";
            }
        }

        return usageName == null ? "[N/A]" : $"{handSufix}{usageName}";
    }
}