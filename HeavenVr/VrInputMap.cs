using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public static class VrInputMap
{
    private static readonly Dictionary<string, IVrInputBinding> viveInputMap = new()
    {
        { "Submit", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        // That extra space at the end of the "DialogueAdvance " string needs to be there, that's how it is in the game.
        { "DialogueAdvance ", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Fire Card", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Fire Card Alt", new VrBoolBinding(XRNode.RightHand, CommonUsages.gripButton) },
        { "Start", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Pause", new VrBoolBinding(XRNode.LeftHand, CommonUsages.menuButton) },
        { "Cancel", new VrBoolBinding(XRNode.LeftHand, CommonUsages.menuButton) },
        { "DialogueFastForward", new VrBoolBinding(XRNode.RightHand, CommonUsages.menuButton) },
        { "Restart", new VrBoolBinding(XRNode.RightHand, CommonUsages.menuButton) },
        { "Jump", new VrBoolBinding(XRNode.LeftHand, CommonUsages.triggerButton) },
        { "Swap Card", new VrBoolBinding(XRNode.LeftHand, CommonUsages.gripButton) },
        { "MenuTabLeft", new VrBoolBinding(XRNode.LeftHand, CommonUsages.gripButton) },
        { "MenuTabRight", new VrBoolBinding(XRNode.RightHand, CommonUsages.gripButton) },
        { "Move", new VrVector2PressBinding(XRNode.LeftHand, CommonUsages.primary2DAxisClick, CommonUsages.primary2DAxis) },
        { "Look", new VrVector2PressBinding(XRNode.RightHand, CommonUsages.primary2DAxisClick, CommonUsages.primary2DAxis) },
    };

    private static readonly Dictionary<string, IVrInputBinding> oculusInputMap = new()
    {
        { "Submit", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "DialogueAdvance ", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Fire Card", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Fire Card Alt", new VrBoolBinding(XRNode.RightHand, CommonUsages.gripButton) },
        { "Start", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Pause", new VrBoolBinding(XRNode.LeftHand, CommonUsages.secondaryButton) },
        { "Cancel", new VrBoolBinding(XRNode.LeftHand, CommonUsages.secondaryButton) },
        { "DialogueFastForward", new VrBoolBinding(XRNode.RightHand, CommonUsages.secondaryButton) },
        { "Restart", new VrBoolBinding(XRNode.RightHand, CommonUsages.secondaryButton) },
        { "Jump", new VrBoolBinding(XRNode.LeftHand, CommonUsages.triggerButton) },
        { "Swap Card", new VrBoolBinding(XRNode.LeftHand, CommonUsages.gripButton) },
        { "MenuTabLeft", new VrBoolBinding(XRNode.LeftHand, CommonUsages.gripButton) },
        { "MenuTabRight", new VrBoolBinding(XRNode.RightHand, CommonUsages.gripButton) },
        { "Move", new VrVector2Binding(XRNode.LeftHand, CommonUsages.primary2DAxis) },
        { "Look", new VrVector2Binding(XRNode.RightHand, CommonUsages.primary2DAxis) }
    };

    private static readonly Dictionary<string, IVrInputBinding> inputMap = viveInputMap;

    public static IVrInputBinding GetBinding(string name)
    {
        inputMap.TryGetValue(name, out var binding);
        return binding;
    }

    public static void Update()
    {
        foreach (var binding in inputMap.Values)
        {
            binding.Update();
        }
    }
}