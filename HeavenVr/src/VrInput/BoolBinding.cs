using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public class BoolBinding: InputBinding<bool>
{
    private readonly InputFeatureUsage<bool>[] _usages;
    private readonly InputFeatureUsage<float>? _floatUsage;

    private float _sensitivity = 1; // TODO sensitivity option;
    
    public BoolBinding(XRNode hand, params InputFeatureUsage<bool>[] usages) : base(hand)
    {
        this._usages = usages;
        if (usages.Contains(CommonUsages.triggerButton))
        {
            this._floatUsage = CommonUsages.trigger;
        }
    }
    
    protected override bool GetValue()
    {
        // TODO get this only once;
        var device = InputManager.GetInputDevice(Hand);

        if (_floatUsage.HasValue && _sensitivity > 0)
        {
            device.TryGetFeatureValue(_floatUsage.Value, out var floatValue);
            Value = floatValue > _sensitivity;
        }
        else
        {
            Value = false;
            foreach (var usage in _usages)
            {
                device.TryGetFeatureValue(usage, out var boolValue);
                Value = Value || boolValue;
            }
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