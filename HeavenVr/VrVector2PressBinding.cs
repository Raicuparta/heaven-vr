using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrVector2PressBinding: VrInputBinding<Vector2>
{
    private readonly InputFeatureUsage<bool> usagePress;
    private readonly InputFeatureUsage<Vector2> usagePosition;
    private const float inputThreshold = 0.5f;

    public VrVector2PressBinding(XRNode hand,
        InputFeatureUsage<bool> usagePress = default,
        InputFeatureUsage<Vector2> usagePosition = default) : base(hand)
    {
        this.usagePress = usagePress;
        this.usagePosition = usagePosition;
    }

    private float GetFloatValue()
    {
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(usagePress, out var value);
        return value ? 1 : 0;
    }

    public override Vector2 GetValue()
    {
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(usagePosition, out var valueVector2);
        var valueNorth = valueVector2.y > inputThreshold ? GetFloatValue() : 0;
        var valueSouth = valueVector2.y < -inputThreshold ? GetFloatValue() : 0;
        var valueEast = valueVector2.x > inputThreshold ? GetFloatValue() : 0;
        var valueWest = valueVector2.x < -inputThreshold ? GetFloatValue() : 0;
        return new Vector2(valueEast - valueWest, valueNorth - valueSouth);
    }
}