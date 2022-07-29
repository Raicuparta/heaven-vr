using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public static class VrInputMap
{
    public static readonly Dictionary<string, VrBoolBinding> BoolInputMap = new();
    public static VrBoolBinding Submit = new("Submit", XRNode.RightHand, CommonUsages.triggerButton);
    public static VrBoolBinding DialogueAdvance =  new("DialogueAdvance ", XRNode.RightHand, CommonUsages.triggerButton);
    public static VrBoolBinding FireCard = new("Fire Card", XRNode.RightHand, CommonUsages.triggerButton);
    public static VrBoolBinding FireCardAlt = new("Fire Card Alt", XRNode.RightHand, CommonUsages.gripButton);
    public static VrBoolBinding Start = new("Start", XRNode.RightHand, CommonUsages.triggerButton);
    public static VrBoolBinding Pause = new("Pause", XRNode.LeftHand, CommonUsages.menuButton);
    public static VrBoolBinding Cancel = new("Cancel", XRNode.LeftHand, CommonUsages.menuButton);
    public static VrBoolBinding DialogueFastForward = new("DialogueFastForward", XRNode.RightHand, CommonUsages.menuButton);
    public static VrBoolBinding Restart = new("Restart", XRNode.RightHand, CommonUsages.menuButton);
    public static VrBoolBinding Jump = new("Jump", XRNode.LeftHand, CommonUsages.triggerButton);
    
    public static readonly Dictionary<string, VrInputBinding<Vector2>> Vector2InputMap = new();
    public static VrVector2CompositeBinding Move = new("Move", XRNode.LeftHand, CommonUsages.primary2DAxisClick, CommonUsages.primary2DAxisClick);
    public static VrVector2Binding Look = new("Look", XRNode.RightHand, CommonUsages.primary2DAxis);

    public static VrBoolBinding GetBoolBinding(string name)
    {
        BoolInputMap.TryGetValue(name, out var binding);
        return binding;
    }
    
    public static VrInputBinding<Vector2> GetVector2Binding(string name)
    {
        Vector2InputMap.TryGetValue(name, out var binding);
        return binding;
    }
}