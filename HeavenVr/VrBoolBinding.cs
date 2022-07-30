using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrBoolBinding: VrInputBinding<bool>
{
    private readonly InputFeatureUsage<bool> usage;
    
    public VrBoolBinding(XRNode hand, InputFeatureUsage<bool> usage) : base(hand)
    {
        this.usage = usage;
    }

    protected override bool GetValue()
    {
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(usage, out Value);
        return Value;
    }

    protected override bool GetValueAsBool(bool value)
    {
        return value;
    }

    protected override Vector2 GetValueAsVector2(bool value)
    {
        return value ? Vector2.one : Vector2.zero;
    }
}