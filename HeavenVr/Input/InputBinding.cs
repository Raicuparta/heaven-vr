using UnityEngine;
using UnityEngine.XR;

namespace HeavenVr.Input;

public abstract class InputBinding<TValue>: IVrInputBinding
{
    public bool WasPressedThisFrame => wasPressedThisFrame;
    public bool WasReleasedThisFrame => wasReleasedThisFrame;
    public bool IsPressed => GetValueAsBool(Value);
    public Vector2 Position => GetValueAsVector2(Value);

    protected readonly XRNode Hand;
    protected TValue Value;

    private TValue previousValue;
    private bool wasPressedThisFrame;
    private bool wasReleasedThisFrame;

    protected InputBinding(XRNode hand)
    {
        Hand = hand;
    }

    protected abstract bool GetValueAsBool(TValue value);
    protected abstract Vector2 GetValueAsVector2(TValue value);

    public virtual void Update()
    {
        Value = GetValue();
        wasPressedThisFrame = false;
        wasReleasedThisFrame = false;
        if (!GetValueAsBool(previousValue) && GetValueAsBool(Value)) wasPressedThisFrame = true;
        if (GetValueAsBool(previousValue) && !GetValueAsBool(Value)) wasReleasedThisFrame = true;
        previousValue = Value;
    }

    protected abstract TValue GetValue();
}