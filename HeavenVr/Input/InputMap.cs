using System;
using System.Collections.Generic;
using UnityEngine.XR;

namespace HeavenVr.Input;

public static class InputMap
{
    public enum VrButton
    {
        PrimaryButton,
        SecondaryButton,
        MenuButton
    }

    private static readonly Dictionary<string, IVrInputBinding> vrInputMap = new()
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

    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>> wmrInputMap = new()
    {
        { VrButton.PrimaryButton, CommonUsages.triggerButton },
        { VrButton.SecondaryButton, CommonUsages.gripButton },
        { VrButton.MenuButton, CommonUsages.secondaryButton }
    };
    
    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>> indexInputMap = new()
    {
        { VrButton.PrimaryButton, CommonUsages.triggerButton },
        { VrButton.SecondaryButton, CommonUsages.primary2DAxisClick },
        { VrButton.MenuButton, CommonUsages.secondaryButton }
    };
    
    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>> viveInputMap = new()
    {
        { VrButton.PrimaryButton, CommonUsages.triggerButton },
        { VrButton.SecondaryButton, CommonUsages.gripButton },
        { VrButton.MenuButton, CommonUsages.menuButton }
    };
    
    private static readonly Dictionary<VrButton, InputFeatureUsage<bool>> oculusInputMap = new()
    {
        { VrButton.PrimaryButton, CommonUsages.triggerButton },
        { VrButton.SecondaryButton, CommonUsages.gripButton },
        { VrButton.MenuButton, CommonUsages.secondaryButton }
    };

    private static Dictionary<VrButton, InputFeatureUsage<bool>> GetAutoInputMap(InputDevice inputDevice)
    {
        if (inputDevice.name.IndexOf("vive", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return viveInputMap;
        }
        if (inputDevice.name.IndexOf("oculus", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return oculusInputMap;
        }
        if (inputDevice.name.IndexOf("windows", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return wmrInputMap;
        }
        if (inputDevice.name.IndexOf("index", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return indexInputMap;
        }

        throw new KeyNotFoundException(
            $"Failed to automatically find control scheme for {inputDevice.name}. Please select the control scheme manually in the VR settings menu.");
    }

    private static Dictionary<VrButton, InputFeatureUsage<bool>> inputMap;

    public static void UpdateInputMap(InputDevice inputDevice)
    {
        inputMap = GetInputMap(inputDevice);
    }

    private static Dictionary<VrButton, InputFeatureUsage<bool>> GetInputMap(InputDevice inputDevice)
    {
        return VrSettings.ControlScheme.Value switch
        {
            VrSettings.ControlSchemeOption.Index => indexInputMap,
            VrSettings.ControlSchemeOption.Oculus => oculusInputMap,
            VrSettings.ControlSchemeOption.Vive => viveInputMap,
            VrSettings.ControlSchemeOption.Wmr => wmrInputMap,
            _ => GetAutoInputMap(inputDevice)
        };
    }

    public static InputFeatureUsage<bool>? GetUsage(VrButton vrButton)
    {
        if (inputMap == null) return null;

        inputMap.TryGetValue(vrButton, out var usage);
        return usage;
    }
    
    // TODO use an enum or a class or something instead of plain strings.
    public static IVrInputBinding GetBinding(string name)
    {
        vrInputMap.TryGetValue(name, out var binding);
        return binding;
    }

    public static void Update()
    {
        foreach (var binding in vrInputMap.Values)
        {
            binding.Update();
        }
    }
}