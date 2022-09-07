using System;
using HeavenVr.ModSettings;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public class InputManager: MonoBehaviour
{
    private static InputDevice _leftInputDevice;
    private static InputDevice _rightInputDevice;

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
}