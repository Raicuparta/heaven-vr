using System.Collections.Generic;
using UnityEngine.XR;

namespace HeavenVr;

public static class VrInputMap
{
    public static readonly Dictionary<string, VrInputBinding> InputMap = new();
    public static VrInputBinding Submit = new("Submit", XRNode.RightHand, CommonUsages.triggerButton);
    public static VrInputBinding DialogueAdvance =  new("DialogueAdvance ", XRNode.RightHand, CommonUsages.triggerButton);
    public static VrInputBinding FireCard = new("Fire Card", XRNode.RightHand, CommonUsages.triggerButton);
    public static VrInputBinding FireCardAlt = new("Fire Card Alt", XRNode.RightHand, CommonUsages.gripButton);
    public static VrInputBinding Start = new("Start", XRNode.LeftHand, CommonUsages.menuButton);
    public static VrInputBinding Pause = new("Pause", XRNode.LeftHand, CommonUsages.menuButton);
    public static VrInputBinding Cancel = new("Cancel", XRNode.LeftHand, CommonUsages.menuButton);
    public static VrInputBinding DialogueFastForward = new("DialogueFastForward", XRNode.RightHand, CommonUsages.menuButton);
    public static VrInputBinding Restart = new("Restart", XRNode.RightHand, CommonUsages.menuButton);
    public static VrInputBinding Card = new("Card", XRNode.LeftHand, CommonUsages.triggerButton);
    public static VrInputBinding Jump = new("Jump", XRNode.RightHand, CommonUsages.primary2DAxisClick);

    public static VrInputBinding GetBinding(string name)
    {
        InputMap.TryGetValue(name, out var binding);
        return binding;
    }
}