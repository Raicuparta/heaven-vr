using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrVector2PressBinding: VrInputBinding<Vector2>
{
    private readonly InputFeatureUsage<bool>? usagePress;
    private readonly InputFeatureUsage<Vector2> usagePosition;
    private const float inputThreshold = 0.5f;

    public VrVector2PressBinding(XRNode hand) : base(hand)
    {
        usagePress = CommonUsages.primary2DAxisClick; // TODO press only on Vive
        usagePosition = CommonUsages.primary2DAxis;
    }

    private float GetFloatValue()
    {
        if (!usagePress.HasValue) return 1;

        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(usagePress.Value, out var value);
        return value ? 1 : 0;
    }

    protected override Vector2 GetValue()
    {
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(usagePosition, out var valueVector2);
        var valueNorth = valueVector2.y > inputThreshold ? GetFloatValue() : 0;
        var valueSouth = valueVector2.y < -inputThreshold ? GetFloatValue() : 0;
        var valueEast = valueVector2.x > inputThreshold ? GetFloatValue() : 0;
        var valueWest = valueVector2.x < -inputThreshold ? GetFloatValue() : 0;
        return new Vector2(valueEast - valueWest, valueNorth - valueSouth);
    }
    
    protected override bool GetValueAsBool(Vector2 value)
    {
        return value != Vector2.zero;
    }

    protected override Vector2 GetValueAsVector2(Vector2 value)
    {
        return Value;
    }
}