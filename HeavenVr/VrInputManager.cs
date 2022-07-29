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
        foreach (var binding in VrInputMap.InputMap.Values)
        {
            binding.Update();
        }
    }

    public static InputDevice GetInputDevice(XRNode hand)
    {
        var devices = new List<InputDevice>();
	    InputDevices.GetDevicesAtXRNode(hand, devices);
        return devices.Count > 0 ? devices[0] : default;
    }
    
    private static (XRNode, InputFeatureUsage<Vector2>)? GetVector2Usage(string name)
    {
        return name switch
        {
            "Move" => (XRNode.LeftHand, CommonUsages.primary2DAxis),
            "Navigate" => (XRNode.LeftHand, CommonUsages.primary2DAxis),
            "Look" => (XRNode.RightHand, CommonUsages.primary2DAxis),
            _ => null
        };
    }

    public static Vector2? GetInputVector2(string name)
    {
        var input = GetVector2Usage(name);
        if (!input.HasValue) return null;
        var (hand, usage) = input.Value;
        GetInputDevice(hand).TryGetFeatureValue(usage, out var value);
        return value;
    }
    
    private static (XRNode, InputFeatureUsage<bool>)? GetBoolUsage(string name)
    {
        return name switch
        {
            "Submit" => (XRNode.RightHand, CommonUsages.triggerButton),
            "DialogueAdvance " => (XRNode.RightHand, CommonUsages.triggerButton),
            "Fire Card" => (XRNode.RightHand, CommonUsages.triggerButton),
            "Fire Card Alt" => (XRNode.RightHand, CommonUsages.gripButton),
            "Start" => (XRNode.LeftHand, CommonUsages.menuButton),
            "Pause" => (XRNode.LeftHand, CommonUsages.menuButton),
            "Cancel" => (XRNode.LeftHand, CommonUsages.menuButton),
            "DialogueFastForward" => (XRNode.RightHand, CommonUsages.menuButton),
            "Restart" => (XRNode.RightHand, CommonUsages.menuButton),
            "Card" => (XRNode.LeftHand, CommonUsages.triggerButton),
            "Jump" => (XRNode.RightHand, CommonUsages.primary2DAxisClick),
            "Swap Card" => null,
            "MenuTabLeft" => null,
            "Leaderboards" => null,
            "Click" => null,
            "Alt" => null,
            "Look" => null,
            "MenuTabRight" => null,
            "MiddleClick" => null,
            "Point" => null,
            "RightClick" => null,
            "ScrollWheel" => null,
            "up" => null,
            "down" => null,
            "left" => null,
            "right" => null,
            _ => null
        };
    }
}