using HeavenVr.Stage;
using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.VrInput;

public abstract class InputBinding<TValue> : IInputBinding
{
    protected readonly bool IsDominantHand;

    private TValue _previousValue;
    protected TValue Value;

    protected InputBinding(bool isDominantHand)
    {
        IsDominantHand = isDominantHand;
    }

    public bool WasPressedThisFrame { get; private set; }
    public bool WasReleasedThisFrame { get; private set; }
    public bool IsPressed => GetValueAsBool(Value);
    public Vector2 Position => GetValueAsVector2(Value);
    public string Name => GetName();

    public virtual void Update()
    {
        Value = GetValue();
        WasPressedThisFrame = false;
        WasReleasedThisFrame = false;

        if (!GetValueAsBool(_previousValue) && GetValueAsBool(Value)) WasPressedThisFrame = true;
        if (GetValueAsBool(_previousValue) && !GetValueAsBool(Value)) WasReleasedThisFrame = true;

        _previousValue = Value;
    }

    protected abstract bool GetValueAsBool(TValue value);
    protected abstract Vector2 GetValueAsVector2(TValue value);

    protected abstract TValue GetValue();
    protected abstract string GetName();
}