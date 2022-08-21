using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.Input;

public class BoolBinding: InputBinding<bool>
{
    private readonly InputMap.VrButton vrButton;
    
    public BoolBinding(XRNode hand, InputMap.VrButton vrButton) : base(hand)
    {
        this.vrButton = vrButton;
    }

    protected override bool GetValue()
    {
        var usage = InputMap.GetUsage(vrButton);
        if (!usage.HasValue) return false;

        InputManager.GetInputDevice(Hand).TryGetFeatureValue(usage.Value, out Value);
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