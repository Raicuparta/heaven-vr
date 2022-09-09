using System;
using HeavenVr.Stage;
using UnityEngine;

namespace HeavenVr.VrUi;

public class VrUi: MonoBehaviour
{
    private Canvas _canvas;
    
    public static void Create(Transform target)
    {
        if (target.name.StartsWith("com.sinai") || target.GetComponent<VrUi>()) return;

        var instance = target.gameObject.AddComponent<VrUi>();
        instance.gameObject.layer = LayerMask.NameToLayer("UI");
        
        instance._canvas = target.GetComponent<Canvas>();
        instance._canvas.renderMode = RenderMode.ScreenSpaceCamera;
    }

    private void Start()
    {
        MoveOverlaysToVrUi();
    }

    private void MoveOverlaysToVrUi()
    {
        if (!GetComponent<PlayerOverlayUI>()) return;

        var overlays = transform.Find("GameplayOverlays");

        if (overlays == null) return;
        transform.Find("GameplayOverlays").localScale = Vector3.one * 0.5f;
    }

    private void Update()
    {
        // TODO do this more efficiently, not on update.
        if (!_canvas || !VrStage.Instance || !VrStage.Instance.UiTarget || !VrStage.Instance.UiTarget.UiCamera) return;
        
        _canvas.worldCamera = VrStage.Instance.UiTarget.UiCamera;
        _canvas.scaleFactor = 0.5f;
        _canvas.planeDistance = 1;
    }
}