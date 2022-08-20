using System.Collections.Generic;
using UnityEngine.XR;

namespace HeavenVr;

public static class VrInputMap
{
    public enum VrButton
    {
        PrimaryButton,
        SecondaryButton,
        MenuButton,
        Axis,
    }

    private static readonly Dictionary<string, IVrInputBinding> vrInputMap = new()
    {
        {"Submit", new VrBoolBinding(XRNode.RightHand, VrButton.PrimaryButton)},
        {"DialogueAdvance ", new VrBoolBinding(XRNode.RightHand, VrButton.PrimaryButton)},
        {"Fire Card", new VrBoolBinding(XRNode.RightHand, VrButton.PrimaryButton)},
        {"Fire Card Alt", new VrBoolBinding(XRNode.RightHand, VrButton.SecondaryButton)},
        {"Start", new VrBoolBinding(XRNode.LeftHand, VrButton.PrimaryButton)},
        {"Pause", new VrBoolBinding(XRNode.LeftHand, VrButton.MenuButton)},
        {"Cancel", new VrBoolBinding(XRNode.LeftHand, VrButton.MenuButton)},
        {"DialogueFastForward", new VrBoolBinding(XRNode.RightHand, VrButton.MenuButton)},
        {"Restart", new VrBoolBinding(XRNode.RightHand, VrButton.MenuButton)},
        {"Jump", new VrBoolBinding(XRNode.LeftHand, VrButton.PrimaryButton)},
        {"Swap Card", new VrBoolBinding(XRNode.LeftHand, VrButton.SecondaryButton)},
        {"MenuTabLeft", new VrBoolBinding(XRNode.LeftHand, VrButton.SecondaryButton)},
        {"MenuTabRight", new VrBoolBinding(XRNode.RightHand, VrButton.SecondaryButton)},
        {"Move", new VrVector2PressBinding(XRNode.LeftHand)},
        {"Look", new VrVector2PressBinding(XRNode.RightHand)}
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

    private static Dictionary<VrButton, InputFeatureUsage<bool>> GetAutoInputMap() => oculusInputMap; // TODO auto input map.
    private static Dictionary<VrButton, InputFeatureUsage<bool>> GetInputMap()
    {
        return VrSettings.ControlScheme.Value switch
        {
            VrSettings.ControlSchemeOption.Index => indexInputMap,
            VrSettings.ControlSchemeOption.Oculus => oculusInputMap,
            VrSettings.ControlSchemeOption.Vive => viveInputMap,
            VrSettings.ControlSchemeOption.Wmr => wmrInputMap,
            _ => GetAutoInputMap()
        };
    }

    public static InputFeatureUsage<bool> GetUsage(VrButton vrButton)
    {
        GetInputMap().TryGetValue(vrButton, out var usage);
        return usage;
    }
    
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