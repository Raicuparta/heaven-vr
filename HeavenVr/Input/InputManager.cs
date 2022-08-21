using System;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.Input;

public class InputManager: MonoBehaviour
{
    private static InputDevice leftInputDevice;
    private static InputDevice rightInputDevice;

    private void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private static void OnDeviceConnected(InputDevice device)
    {
        if ((device.characteristics & InputDeviceCharacteristics.Controller) == 0) return;

        if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
        {
            rightInputDevice = device;
        }
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
        {
            leftInputDevice = device;
        }
        InputMap.UpdateInputMap(device);
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
            XRNode.RightHand => rightInputDevice,
            XRNode.LeftHand => leftInputDevice,
            _ => throw new ArgumentOutOfRangeException(nameof(hand), hand, null)
        };
    }
}