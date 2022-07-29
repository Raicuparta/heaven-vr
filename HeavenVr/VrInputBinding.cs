using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrInputBinding
{
    private readonly string name;
    private readonly XRNode hand;
    private readonly InputFeatureUsage<bool> usage;
    private bool value;
    private bool previousValue;
    public bool WasReleasedThisFrame;
    public bool WasPressedThisFrame;
    
    public VrInputBinding(string name, XRNode hand, InputFeatureUsage<bool> usage)
    {
        VrInputMap.InputMap[name] = this;
        this.name = name;
        this.hand = hand;
        this.usage = usage;
    }

    public void Update()
    {
        value = GetValue();
        WasPressedThisFrame = false;
        WasReleasedThisFrame = false;
        if (!previousValue && value) WasPressedThisFrame = true;
        if (previousValue && !value) WasReleasedThisFrame = true;
        previousValue = value;
    }

    public bool GetValue()
    {
        VrInputManager.GetInputDevice(hand).TryGetFeatureValue(usage, out value);
        return value;
    }
    

}