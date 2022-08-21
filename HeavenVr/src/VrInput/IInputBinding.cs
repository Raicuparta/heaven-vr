using UnityEngine;

namespace HeavenVr.VrInput;

public interface IInputBinding
{
    public void Update();
    public bool WasPressedThisFrame { get; }
    public bool WasReleasedThisFrame { get; }
    public bool IsPressed { get; }
    public Vector2 Position { get; }
}