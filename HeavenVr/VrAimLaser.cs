using UnityEngine;

namespace HeavenVr;

public class VrAimLaser: MonoBehaviour
{
    private LineRenderer line;
    private const string lineShaderName = "Universal Render Pipeline/Simple Lit";
    private const float rayDistance = 300f;

    public static VrAimLaser Create(Transform parent)
    {
        var instance = parent.Find("Laser").GetOrAddComponent<VrAimLaser>();
        LayerHelper.SetLayerRecursive(instance.gameObject, GameLayer.VrUi);

        return instance;
    }
}