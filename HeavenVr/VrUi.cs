using UnityEngine;

namespace HeavenVr;

public class VrUi: MonoBehaviour
{
    private Transform target;
    private float zOffset;

    public static void Create(Transform target, float zOffset = 0)
    {
        if (target.name.StartsWith("com.sinai")) return;

        var instance = target.gameObject.AddComponent<VrUi>();
        instance.target = target;
        instance.gameObject.layer = LayerMask.NameToLayer("UI");
        instance.zOffset = zOffset;
        
        var canvas = target.GetComponent<Canvas>();
        if (canvas)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            instance.transform.localScale = Vector3.one * 0.0025f;
        }
    }

    private void Update()
    {
        if (!VrStage.Instance || !VrStage.Instance.UiTarget) return;

        transform.position = VrStage.Instance.UiTarget.TargetTransform.position;
        if (zOffset > 0)
        {
            transform.localPosition += Vector3.forward * zOffset;
        }
        transform.rotation = VrStage.Instance.UiTarget.TargetTransform.rotation;
    }
}