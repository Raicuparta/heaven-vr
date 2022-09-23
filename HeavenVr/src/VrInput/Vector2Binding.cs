using System;
using HeavenVr.ModSettings;
using HeavenVr.Stage;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public class Vector2Binding : InputBinding<Vector2>
{
    private const float InputThreshold = 0.5f;
    private readonly InputFeatureUsage<Vector2> _usagePosition;
    private readonly InputFeatureUsage<bool> _usagePress;

    private XRNode Hand => VrHand.IsSwapSticksLeftPose(IsDominantHand) ? XRNode.LeftHand : XRNode.RightHand;

    public Vector2Binding(bool isDominantHand) : base(isDominantHand)
    {
        _usagePress = CommonUsages.primary2DAxisClick;
        _usagePosition = CommonUsages.primary2DAxis;
    }

    private static bool IsTouchAxisMode()
    {
        return VrSettings.AxisMode.Value switch
        {
            VrSettings.AxisModeOption.Auto => !InputManager.IsDevice("vive"),
            VrSettings.AxisModeOption.Click => false,
            VrSettings.AxisModeOption.Touch => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetFloatValue()
    {
        var device = InputManager.GetInputDevice(Hand);

        if (IsTouchAxisMode()) return 1;

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
        return value.sqrMagnitude > InputThreshold * InputThreshold;
    }

    protected override Vector2 GetValueAsVector2(Vector2 value)
    {
        return Value;
    }

    protected override string GetName()
    {
        return IsTouchAxisMode()
            ? InputManager.GetUsageName(_usagePosition, IsDominantHand)
            : InputManager.GetUsageName(_usagePress, IsDominantHand);
    }
}