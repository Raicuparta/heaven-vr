using UnityEngine;

namespace HeavenVr.VrInput;

public interface IInputBinding
{
    public bool WasPressedThisFrame { get; }
    public bool WasReleasedThisFrame { get; }
    public bool IsPressed { get; }
    public Vector2 Position { get; }
    public string Name { get; }
    public void Update();
}