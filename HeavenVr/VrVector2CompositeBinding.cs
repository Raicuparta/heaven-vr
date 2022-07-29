using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr;

public class VrVector2CompositeBinding: VrInputBinding<Vector2>
{ 
    InputFeatureUsage<bool> usageNorth;
    InputFeatureUsage<bool> usageSouth;
    InputFeatureUsage<bool> usageEast;
    private InputFeatureUsage<bool> usageWest;
    private const float inputThreshold = 0.5f;

    public VrVector2CompositeBinding(string name,
        XRNode hand,
        InputFeatureUsage<bool> usageNorth = default,
        InputFeatureUsage<bool> usageSouth = default,
        InputFeatureUsage<bool> usageEast = default,
        InputFeatureUsage<bool> usageWest = default) : base(hand)
    {
        this.usageNorth = usageNorth;
        this.usageSouth = usageSouth;
        this.usageEast = usageEast;
        this.usageWest = usageWest;
        VrInputMap.Vector2InputMap[name] = this;
    }

    private float GetFloatValue(InputFeatureUsage<bool> usage)
    {
        if (usage == default) return 0;
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(usage, out var value);
        return value ? 1 : 0;
    }

    public override Vector2 GetValue()
    {
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(CommonUsages.primary2DAxis, out var valueVector2);
        var valueNorth = valueVector2.y > inputThreshold ? GetFloatValue(usageNorth) : 0;
        var valueSouth = valueVector2.y < -inputThreshold ? GetFloatValue(usageSouth) : 0;
        var valueEast = valueVector2.x > inputThreshold ? GetFloatValue(usageNorth) : 0;
        var valueWest = valueVector2.x < -inputThreshold ? GetFloatValue(usageSouth) : 0;
        return new Vector2(valueEast - valueWest, valueNorth - valueSouth);
    }
}