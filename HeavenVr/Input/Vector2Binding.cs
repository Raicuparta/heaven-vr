using System;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.Input;

public class Vector2Binding: InputBinding<Vector2>
{
    private readonly InputFeatureUsage<bool> _usagePress;
    private readonly InputFeatureUsage<Vector2> _usagePosition;
    private const float InputThreshold = 0.5f;

    public Vector2Binding(XRNode hand) : base(hand)
    {
        _usagePress = CommonUsages.primary2DAxisClick;
        _usagePosition = CommonUsages.primary2DAxis;
    }

    private static bool IsTouchAxisMode(InputDevice device)
    {
        return VrSettings.AxisMode.Value switch
        {
            // TODO use events instead of checking this every time.
            VrSettings.AxisModeOption.Auto => device.name.IndexOf("vive", StringComparison.OrdinalIgnoreCase) < 0,
            VrSettings.AxisModeOption.Click => false,
            VrSettings.AxisModeOption.Touch => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetFloatValue()
    {
        // TODO no need to fetch the device every time.
        var device = InputManager.GetInputDevice(Hand);
        
        if (IsTouchAxisMode(device)) return 1;
        
        device.TryGetFeatureValue(_usagePress, out var value);
        return value ? 1 : 0;
    }

    protected override Vector2 GetValue()
    {
        InputManager.GetInputDevice(Hand).TryGetFeatureValue(_usagePosition, out var valueVector2);
        var valueNorth = valueVector2.y > InputThreshold ? GetFloatValue() : 0;
        var valueSouth = valueVector2.y < -InputThreshold ? GetFloatValue() : 0;
        var valueEast = valueVector2.x > InputThreshold ? GetFloatValue() : 0;
        var valueWest = valueVector2.x < -InputThreshold ? GetFloatValue() : 0;
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