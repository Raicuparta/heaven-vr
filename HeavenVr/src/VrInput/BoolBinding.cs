using System.Linq;
using HeavenVr.ModSettings;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public class BoolBinding : InputBinding<bool>
{
    private readonly InputFeatureUsage<float>? _floatUsage;
    private readonly InputFeatureUsage<bool>[] _usages;

    public BoolBinding(XRNode hand, params InputFeatureUsage<bool>[] usages) : base(hand)
    {
        _usages = usages;
        if (usages.Contains(CommonUsages.triggerButton)) _floatUsage = CommonUsages.trigger;
    }

    protected override bool GetValue()
    {
        // TODO get this only once;
        var device = InputManager.GetInputDevice(Hand);

        var sensitivity = (float)VrSettings.TriggerSensitivity.Value / VrSettings.MaxTriggerSensitivity;

        if (_floatUsage.HasValue && sensitivity > 0)
        {
            device.TryGetFeatureValue(_floatUsage.Value, out var floatValue);
            Value = floatValue > sensitivity;
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

    protected override string GetName()
    {
        return InputManager.GetUsageName(_usages[0], Hand);
    }
}