using UnityEngine;

namespace HeavenVr;

public class VrUi: MonoBehaviour
{
    private Canvas canvas;

    public static void Create(Canvas canvas)
    {
        if (canvas.name.StartsWith("com.sinai")) return;

        var instance = canvas.gameObject.AddComponent<VrUi>();
        instance.canvas = canvas;
        instance.gameObject.layer = LayerMask.NameToLayer("UI");
    }

    private void Start()
    {
        canvas.renderMode = RenderMode.WorldSpace;
        transform.localScale = Vector3.one * 0.001f;
    }

    private void Update()
    {
        if (!VrStage.Instance || !VrStage.Instance.UiTarget) return;

        transform.position = VrStage.Instance.UiTarget.TargetTransform.position;
        transform.rotation = VrStage.Instance.UiTarget.TargetTransform.rotation;
    }
}