using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrBoolBinding: VrInputBinding<bool>
{
    private readonly VrInputMap.VrButton vrButton;
    
    public VrBoolBinding(XRNode hand, VrInputMap.VrButton vrButton) : base(hand)
    {
        this.vrButton = vrButton;
    }

    protected override bool GetValue()
    {
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(VrInputMap.GetUsage(vrButton), out Value);
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