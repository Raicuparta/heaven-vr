using System.Linq;
using HeavenVr.ModSettings;
using HeavenVr.Stage;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public class BoolBinding : InputBinding<bool>
{
    private readonly InputFeatureUsage<float>? _floatUsage;
    private readonly InputFeatureUsage<bool>[] _usages;

    private XRNode Hand => VrHand.IsLeftHandedLeftPose(IsDominantHand) ? XRNode.LeftHand : XRNode.RightHand;

    public BoolBinding(bool isDominantHand, params InputFeatureUsage<bool>[] usages) : base(isDominantHand)
    {
        _usages = usages;
        if (usages.Contains(CommonUsages.triggerButton)) _floatUsage = CommonUsages.trigger;
    }

    protected override bool GetValue()
    {
        // TODO get this only once;
        var device = InputManager.GetInputDevice(Hand);

        var sensitivity = 1 - VrSettings.TriggerSensitivity.Value / (VrSettings.MaxTriggerSensitivity * 1.1f);

        if (_floatUsage.HasValue && sensitivity < 1)
        {
            device.TryGetFeatureValue(_floatUsage.Value, out var floatValue);
            Value = floatValue >= sensitivity;
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
        return InputManager.GetUsageName(_usages[0], IsDominantHand);
    }
}