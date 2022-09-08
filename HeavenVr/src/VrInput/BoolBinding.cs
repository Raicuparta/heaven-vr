using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public class BoolBinding: InputBinding<bool>
{
    private readonly InputFeatureUsage<bool>[] _usages;
    
    public BoolBinding(XRNode hand, params InputFeatureUsage<bool>[] usages) : base(hand)
    {
        this._usages = usages;
    }

    protected override bool GetValue()
    {
        Value = false;

        foreach (var usage in _usages)
        {
            InputManager.GetInputDevice(Hand).TryGetFeatureValue(usage, out var usageValue);
            Value = Value || usageValue;
        }
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