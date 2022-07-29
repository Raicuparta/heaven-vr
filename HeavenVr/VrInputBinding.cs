using UnityEngine.XR;

namespace HeavenVr;

public abstract class VrInputBinding<T>: IVrInputBinding
{
    protected readonly XRNode Hand;
    protected T Value;
    
    protected VrInputBinding(XRNode hand)
    {
        Hand = hand;
    }

    public virtual void Update()
    {
        Value = GetValue();
    }

    public abstract T GetValue();
}