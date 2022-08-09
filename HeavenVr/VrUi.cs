using UnityEngine;

namespace HeavenVr;

public class VrUi: MonoBehaviour
{
    private Canvas canvas;
    
    public static void Create(Transform target, float zOffset = 0)
    {
        if (target.name.StartsWith("com.sinai") || target.GetComponent<VrUi>()) return;

        var instance = target.gameObject.AddComponent<VrUi>();
        instance.gameObject.layer = LayerMask.NameToLayer("UI");
        
        instance.canvas = target.GetComponent<Canvas>();
    }

    public void Update()
    {
        // TODO do this more efficiently, not on update.
        if (!canvas || !VrStage.Instance || !VrStage.Instance.UiTarget || !VrStage.Instance.UiTarget.UiCamera) return;
        
        canvas.worldCamera = VrStage.Instance.UiTarget.UiCamera;
        canvas.scaleFactor = 0.5f;
        canvas.planeDistance = 1;
    }
}