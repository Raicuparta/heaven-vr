using System;
using System.Collections.Generic;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public static class InputMap
{
    private enum VrButton
    {
        Fire,
        Jump,
        Discard,
        Swap,
        Menu,
        Restart
    }

    private static readonly Dictionary<string, VrButton> VRInputMap = new()
    {
        {"Submit", VrButton.Fire},
        {"DialogueAdvance ", VrButton.Fire},
        {"Fire Card", VrButton.Fire},
        {"Fire Card Alt", VrButton.Discard},
        {"Start", VrButton.Jump},
        {"Pause", VrButton.Menu},
        {"Cancel", VrButton.Menu},
        {"DialogueFastForward", VrButton.Restart},
        {"Restart", VrButton.Restart},
        {"Jump", VrButton.Jump},
        {"Swap Card", VrButton.Swap},
        {"MenuTabLeft", VrButton.Swap},
        {"MenuTabRight", VrButton.Discard},
        // {"Move", new Vector2Binding(XRNode.LeftHand)},
        // {"Look", new Vector2Binding(XRNode.RightHand)}
    };

    private static readonly Dictionary<VrButton, BoolBinding> WmrInputMap = new()
    {
        { VrButton.Fire, new BoolBinding(XRNode.RightHand, CommonUsages.triggerButton ) },
        { VrButton.Jump, new BoolBinding(XRNode.LeftHand, CommonUsages.triggerButton ) },
        { VrButton.Discard, new BoolBinding(XRNode.RightHand, CommonUsages.gripButton ) },
        { VrButton.Swap, new BoolBinding(XRNode.LeftHand, CommonUsages.gripButton ) },
        { VrButton.Menu, new BoolBinding(XRNode.RightHand, CommonUsages.menuButton ) },
        { VrButton.Restart, new BoolBinding(XRNode.LeftHand, CommonUsages.menuButton ) }
    };
    
    private static readonly Dictionary<VrButton, BoolBinding> IndexInputMap = new()
    {
        { VrButton.Fire, new BoolBinding(XRNode.RightHand, CommonUsages.triggerButton ) },
        { VrButton.Jump, new BoolBinding(XRNode.LeftHand, CommonUsages.triggerButton ) },
        { VrButton.Discard, new BoolBinding(XRNode.RightHand, CommonUsages.primary2DAxisClick, CommonUsages.primaryButton ) },
        { VrButton.Swap, new BoolBinding(XRNode.LeftHand, CommonUsages.primary2DAxisClick, CommonUsages.primaryButton ) },
        { VrButton.Menu, new BoolBinding(XRNode.RightHand, CommonUsages.secondaryButton ) },
        { VrButton.Restart, new BoolBinding(XRNode.LeftHand, CommonUsages.secondaryButton ) }
    };
    
    private static readonly Dictionary<VrButton, BoolBinding> ViveInputMap = new()
    {
        { VrButton.Fire, new BoolBinding(XRNode.RightHand, CommonUsages.triggerButton ) },
        { VrButton.Jump, new BoolBinding(XRNode.LeftHand, CommonUsages.triggerButton ) },
        { VrButton.Discard, new BoolBinding(XRNode.RightHand, CommonUsages.gripButton ) },
        { VrButton.Swap, new BoolBinding(XRNode.LeftHand, CommonUsages.gripButton ) },
        { VrButton.Menu, new BoolBinding(XRNode.RightHand, CommonUsages.menuButton ) },
        { VrButton.Restart, new BoolBinding(XRNode.LeftHand, CommonUsages.menuButton ) }
    };
    
    private static readonly Dictionary<VrButton, BoolBinding> OculusInputMap = new()
    {
        { VrButton.Fire, new BoolBinding(XRNode.RightHand, CommonUsages.triggerButton ) },
        { VrButton.Jump, new BoolBinding(XRNode.LeftHand, CommonUsages.triggerButton ) },
        { VrButton.Discard, new BoolBinding(XRNode.RightHand, CommonUsages.gripButton, CommonUsages.primaryButton ) },
        { VrButton.Swap, new BoolBinding(XRNode.LeftHand, CommonUsages.gripButton, CommonUsages.primaryButton ) },
        { VrButton.Menu, new BoolBinding(XRNode.RightHand, CommonUsages.secondaryButton ) },
        { VrButton.Restart, new BoolBinding(XRNode.LeftHand, CommonUsages.secondaryButton ) }
    };

    private static Dictionary<VrButton, BoolBinding> GetAutoInputMap(InputDevice inputDevice)
    {
        if (StringHelper.ContainsCaseInsensitive(inputDevice.name, "vive"))
        {
            return ViveInputMap;
        }
        if (StringHelper.ContainsCaseInsensitive(inputDevice.name, "oculus"))
        {
            return OculusInputMap;
        }
        if (StringHelper.ContainsCaseInsensitive(inputDevice.name, "windows"))
        {
            return WmrInputMap;
        }
        if (StringHelper.ContainsCaseInsensitive(inputDevice.name, "index"))
        {
            return IndexInputMap;
        }

        throw new KeyNotFoundException(
            $"Failed to automatically find control scheme for {inputDevice.name}. Please select the control scheme manually in the VR settings menu.");
    }

    private static Dictionary<VrButton, BoolBinding> _inputMap = new();

    public static void UpdateInputMap(InputDevice inputDevice)
    {
        _inputMap = GetInputMap(inputDevice);
    }

    private static Dictionary<VrButton, BoolBinding> GetInputMap(InputDevice inputDevice)
    {
        return VrSettings.ControlScheme.Value switch
        {
            VrSettings.ControlSchemeOption.Index => IndexInputMap,
            VrSettings.ControlSchemeOption.Oculus => OculusInputMap,
            VrSettings.ControlSchemeOption.Vive => ViveInputMap,
            VrSettings.ControlSchemeOption.Wmr => WmrInputMap,
            _ => GetAutoInputMap(inputDevice)
        };
    }
    
    // TODO use an enum or a class or something instead of plain strings.
    public static IInputBinding GetBinding(string name)
    {
        VRInputMap.TryGetValue(name, out var vrButton);
        _inputMap.TryGetValue(vrButton, out var binding);
        return binding;
    }

    public static void Update()
    {
        foreach (var vrButton in Enum.GetValues(typeof(VrButton)))
        {
            _inputMap.TryGetValue((VrButton) vrButton, out var binding);
            binding?.Update();
        }
    }
}