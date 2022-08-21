using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public abstract class InputBinding<TValue>: IInputBinding
{
    public bool WasPressedThisFrame { get; private set; }
    public bool WasReleasedThisFrame { get; private set; }
    public bool IsPressed => GetValueAsBool(Value);
    public Vector2 Position => GetValueAsVector2(Value);

    protected readonly XRNode Hand;
    protected TValue Value;

    private TValue _previousValue;

    protected InputBinding(XRNode hand)
    {
        Hand = hand;
    }

    protected abstract bool GetValueAsBool(TValue value);
    protected abstract Vector2 GetValueAsVector2(TValue value);

    public virtual void Update()
    {
        Value = GetValue();
        WasPressedThisFrame = false;
        WasReleasedThisFrame = false;
        if (!GetValueAsBool(_previousValue) && GetValueAsBool(Value)) WasPressedThisFrame = true;
        if (GetValueAsBool(_previousValue) && !GetValueAsBool(Value)) WasReleasedThisFrame = true;
        _previousValue = Value;
    }

    protected abstract TValue GetValue();
}