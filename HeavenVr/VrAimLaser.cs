using UnityEngine;

namespace HeavenVr;

public class VrAimLaser: MonoBehaviour
{
    private LineRenderer line;
    private const string lineShaderName = "Universal Render Pipeline/Simple Lit";
    private const float rayDistance = 300f;

    public static VrAimLaser Create(Transform parent)
    {
        var instance = new GameObject("VrAimLaser").AddComponent<VrAimLaser>();
        instance.transform.SetParent(parent, false);

        return instance;
    }
    
    private void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        transform.SetParent(transform, false);
        transform.localEulerAngles = Vector3.right * 60f;
        line.useWorldSpace = false;
        line.startWidth = 0.01f;
        line.endWidth = 0f;
        line.SetPositions(new []{ Vector3.zero, Vector3.forward * rayDistance });
        line.material.shader = Shader.Find(lineShaderName);
        // Using Ignore Raycast layer because it's visible in map camera and player camera.
        // Might be better to use a custom layer, but seems like they're all being used.
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
}