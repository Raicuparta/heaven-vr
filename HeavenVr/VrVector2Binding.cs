using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrVector2Binding: VrInputBinding<Vector2>
{
    private readonly InputFeatureUsage<Vector2> usage;
    
    public VrVector2Binding(string name, XRNode hand, InputFeatureUsage<Vector2> usage) : base(hand)
    {
        this.usage = usage;
        VrInputMap.Vector2InputMap[name] = this;
    }

    public override Vector2 GetValue()
    {
        var device = VrInputManager.GetInputDevice(Hand);
        device.TryGetFeatureValue(usage, out var value);
        // TODO this is only useful for VIVE.
        device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out var clicking);

        Value = clicking ? value : Vector2.zero;
        return Value;
    }
}