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
    }

    private void Start()
    {
        canvas.renderMode = RenderMode.WorldSpace;
        transform.localScale = Vector3.one * 0.001f;
        InvokeRepeating(nameof(UpdatePosition), 0, 5f);
    }

    private void UpdatePosition()
    {
        transform.position = VrStage.Instance.transform.position + VrStage.Instance.transform.forward;
    }
}