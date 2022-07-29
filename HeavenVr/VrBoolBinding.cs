using UnityEngine.XR;

namespace HeavenVr;

public class VrBoolBinding: VrInputBinding<bool>
{
    private readonly InputFeatureUsage<bool> usage;
    private bool previousValue;
    public bool WasReleasedThisFrame;
    public bool WasPressedThisFrame;
    
    public VrBoolBinding(string name, XRNode hand, InputFeatureUsage<bool> usage) : base(hand)
    {
        this.usage = usage;
        VrInputMap.BoolInputMap[name] = this;
    }

    public override void Update()
    {
        base.Update();
        WasPressedThisFrame = false;
        WasReleasedThisFrame = false;
        if (!previousValue && Value) WasPressedThisFrame = true;
        if (previousValue && !Value) WasReleasedThisFrame = true;
        previousValue = Value;
    }

    public override bool GetValue()
    {
        VrInputManager.GetInputDevice(Hand).TryGetFeatureValue(usage, out Value);
        return Value;
    }
}