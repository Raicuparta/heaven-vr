using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public abstract class InputBinding<TValue>: IInputBinding
{
    public bool WasPressedThisFrame => _wasPressedThisFrame;
    public bool WasReleasedThisFrame => _wasReleasedThisFrame;
    public bool IsPressed => GetValueAsBool(Value);
    public Vector2 Position => GetValueAsVector2(Value);

    protected readonly XRNode Hand;
    protected TValue Value;

    private TValue _previousValue;
    private bool _wasPressedThisFrame;
    private bool _wasReleasedThisFrame;

    protected InputBinding(XRNode hand)
    {
        Hand = hand;
    }

    protected abstract bool GetValueAsBool(TValue value);
    protected abstract Vector2 GetValueAsVector2(TValue value);

    public virtual void Update()
    {
        Value = GetValue();
        _wasPressedThisFrame = false;
        _wasReleasedThisFrame = false;
        if (!GetValueAsBool(_previousValue) && GetValueAsBool(Value)) _wasPressedThisFrame = true;
        if (GetValueAsBool(_previousValue) && !GetValueAsBool(Value)) _wasReleasedThisFrame = true;
        _previousValue = Value;
    }

    protected abstract TValue GetValue();
}