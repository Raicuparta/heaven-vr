using System;
using System.Collections.Generic;
using HeavenVr.ModSettings;
using JetBrains.Annotations;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public static class InputMap
{
    public enum VrButton
    {
        PrimaryButton,
        SecondaryButton,
        MenuButton
    }

    private static readonly Dictionary<string, IInputBinding> VRInputMap = new()
    {
        {"Submit", new BoolBinding(XRNode.RightHand, VrButton.PrimaryButton)},
        {"DialogueAdvance ", new BoolBinding(XRNode.RightHand, VrButton.PrimaryButton)},
        {"Fire Card", new BoolBinding(XRNode.RightHand, VrButton.PrimaryButton)},
        {"Fire Card Alt", new BoolBinding(XRNode.RightHand, VrButton.SecondaryButton)},
        {"Start", new BoolBinding(XRNode.LeftHand, VrButton.PrimaryButton)},
        {"Pause", new BoolBinding(XRNode.LeftHand, VrButton.MenuButton)},
        {"Cancel", new BoolBinding(XRNode.LeftHand, VrButton.MenuButton)},
        {"DialogueFastForward", new BoolBinding(XRNode.RightHand, VrButton.MenuButton)},
        {"Restart", new BoolBinding(XRNode.RightHand, VrButton.MenuButton)},
        {"Jump", new BoolBinding(XRNode.LeftHand, VrButton.PrimaryButton)},
        {"Swap Card", new BoolBinding(XRNode.LeftHand, VrButton.SecondaryButton)},
        {"MenuTabLeft", new BoolBinding(XRNode.LeftHand, VrButton.SecondaryButton)},
        {"MenuTabRight", new BoolBinding(XRNode.RightHand, VrButton.SecondaryButton)},
        {"Move", new Vector2Binding(XRNode.LeftHand)},
        {"Look", new Vector2Binding(XRNode.RightHand)}
    };

    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>[]> WmrInputMap = new()
    {
        { VrButton.PrimaryButton, new[] { CommonUsages.triggerButton } },
        { VrButton.SecondaryButton, new[] { CommonUsages.gripButton } },
        { VrButton.MenuButton, new[] { CommonUsages.secondaryButton } }
    };
    
    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>[]> IndexInputMap = new()
    {
        { VrButton.PrimaryButton, new[] { CommonUsages.triggerButton } },
        { VrButton.SecondaryButton, new[] { CommonUsages.primary2DAxisClick, CommonUsages.primaryButton } },
        { VrButton.MenuButton, new[] { CommonUsages.secondaryButton } }
    };
    
    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>[]> ViveInputMap = new()
    {
        { VrButton.PrimaryButton, new[] { CommonUsages.triggerButton } },
        { VrButton.SecondaryButton, new[] { CommonUsages.gripButton } },
        { VrButton.MenuButton, new[] { CommonUsages.menuButton } }
    };
    
    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>[]> OculusInputMap = new()
    {
        { VrButton.PrimaryButton, new[] { CommonUsages.triggerButton } },
        { VrButton.SecondaryButton, new[] { CommonUsages.gripButton, CommonUsages.primaryButton } },
        { VrButton.MenuButton, new[] { CommonUsages.secondaryButton } }
    };

    private static Dictionary<VrButton, InputFeatureUsage<bool>[]> GetAutoInputMap(InputDevice inputDevice)
    {
        if (inputDevice.name.IndexOf("vive", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return ViveInputMap;
        }
        if (inputDevice.name.IndexOf("oculus", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return OculusInputMap;
        }
        if (inputDevice.name.IndexOf("windows", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return WmrInputMap;
        }
        if (inputDevice.name.IndexOf("index", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return IndexInputMap;
        }

        throw new KeyNotFoundException(
            $"Failed to automatically find control scheme for {inputDevice.name}. Please select the control scheme manually in the VR settings menu.");
    }

    private static Dictionary<VrButton, InputFeatureUsage<bool>[]> _inputMap;

    public static void UpdateInputMap(InputDevice inputDevice)
    {
        _inputMap = GetInputMap(inputDevice);
    }

    private static Dictionary<VrButton, InputFeatureUsage<bool>[]> GetInputMap(InputDevice inputDevice)
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

    [CanBeNull]
    public static InputFeatureUsage<bool>[] GetUsages(VrButton vrButton)
    {
        // TODO _inputMap might not be updated with manual settings.
        if (_inputMap == null) return null;

        _inputMap.TryGetValue(vrButton, out var usages);
        return usages;
    }
    
    // TODO use an enum or a class or something instead of plain strings.
    public static IInputBinding GetBinding(string name)
    {
        VRInputMap.TryGetValue(name, out var binding);
        return binding;
    }

    public static void Update()
    {
        foreach (var binding in VRInputMap.Values)
        {
            binding.Update();
        }
    }
}