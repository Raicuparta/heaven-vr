using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public static class VrInputMap
{
    private static readonly Dictionary<string, VrBoolBinding> boolViveInputMap = new()
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
    };

    private static readonly Dictionary<string, VrBoolBinding> boolOculusInputMap = new()
    {
        { "Submit", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "DialogueAdvance ", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Fire Card", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Fire Card Alt", new VrBoolBinding(XRNode.RightHand, CommonUsages.primaryButton) },
        { "Start", new VrBoolBinding(XRNode.RightHand, CommonUsages.triggerButton) },
        { "Pause", new VrBoolBinding(XRNode.LeftHand, CommonUsages.secondaryButton) },
        { "Cancel", new VrBoolBinding(XRNode.LeftHand, CommonUsages.secondaryButton) },
        { "DialogueFastForward", new VrBoolBinding(XRNode.RightHand, CommonUsages.secondaryButton) },
        { "Restart", new VrBoolBinding(XRNode.RightHand, CommonUsages.secondaryButton) },
        { "Jump", new VrBoolBinding(XRNode.LeftHand, CommonUsages.triggerButton) },
    };

    private static readonly Dictionary<string, VrInputBinding<Vector2>> vector2ViveInputMap = new()
    {
        { "Move", new VrVector2PressBinding(XRNode.LeftHand, CommonUsages.primary2DAxisClick, CommonUsages.primary2DAxis) },
        { "Look", new VrVector2PressBinding(XRNode.RightHand, CommonUsages.primary2DAxisClick, CommonUsages.primary2DAxis) },
    };
    
    private static readonly Dictionary<string, VrInputBinding<Vector2>> vector2OculusInputMap = new()
    {
        { "Move", new VrVector2Binding(XRNode.LeftHand, CommonUsages.primary2DAxis) },
        { "Look", new VrVector2Binding(XRNode.RightHand, CommonUsages.primary2DAxis) }
    };

    private static readonly Dictionary<string, VrBoolBinding> boolInputMap = boolOculusInputMap;
    private static readonly Dictionary<string, VrInputBinding<Vector2>> vector2InputMap = vector2OculusInputMap;

    public static VrBoolBinding GetBoolBinding(string name)
    {
        boolInputMap.TryGetValue(name, out var binding);
        return binding;
    }
    
    public static VrInputBinding<Vector2> GetVector2Binding(string name)
    {
        vector2InputMap.TryGetValue(name, out var binding);
        return binding;
    }

    public static void Update()
    {
        foreach (var binding in boolInputMap.Values)
        {
            binding.Update();
        }
    }
}