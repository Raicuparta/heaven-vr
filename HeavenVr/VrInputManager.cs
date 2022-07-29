using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrInputManager: MonoBehaviour
{
    private static InputDevice leftInputDevice;
    private static InputDevice rightInputDevice;
    
    public static void Create()
    {
        new GameObject("VrInputManager").AddComponent<VrInputManager>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        VrInputMap.Update();
    }

    public static InputDevice GetInputDevice(XRNode hand)
    {
        var devices = new List<InputDevice>();
	    InputDevices.GetDevicesAtXRNode(hand, devices);
        return devices.Count > 0 ? devices[0] : default;
    }
}