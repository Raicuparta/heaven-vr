using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrVector2Binding: VrInputBinding<Vector2>
{
    private readonly InputFeatureUsage<Vector2> usage;
    
    public VrVector2Binding(XRNode hand, InputFeatureUsage<Vector2> usage) : base(hand)
    {
        this.usage = usage;
    }

    public override Vector2 GetValue()
    {
        var device = VrInputManager.GetInputDevice(Hand);
        device.TryGetFeatureValue(usage, out Value);
        return Value;
    }
}