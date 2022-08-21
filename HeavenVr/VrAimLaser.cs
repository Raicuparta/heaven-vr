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

    private void Start()
    {
        SetUpCrosshair();
    }

    private void SetUpCrosshair()
    {
        if (!RM.ui || !RM.ui.crosshair) return;

        var crosshair = RM.ui.crosshair.parent;
        
        crosshair.SetParent(transform, false);
        crosshair.localScale = Vector3.one * 0.1f;
        crosshair.localPosition = Vector3.forward;
        crosshair.localEulerAngles = Vector3.forward * -90f;
        LayerHelper.SetLayerRecursive(RM.ui.crosshair.parent.gameObject, GameLayer.Default);
    }
}