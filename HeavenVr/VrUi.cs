using System;
using UnityEngine;

namespace HeavenVr;

public class VrUi: MonoBehaviour
{
    private Canvas canvas;
    
    public static void Create(Transform target, float zOffset = 0)
    {
        if (target.name.StartsWith("com.sinai")) return;

        var instance = target.gameObject.AddComponent<VrUi>();
        instance.gameObject.layer = LayerMask.NameToLayer("UI");
                
        instance.canvas = target.GetComponent<Canvas>();
        instance.canvas.scaleFactor = 0.5f;
    }

    public void Update()
    {
        // TODO do this more efficiently, not on update.
        if (!VrStage.Instance || !VrStage.Instance.UiCamera) return;
        
        canvas.worldCamera = VrStage.Instance.UiCamera;
        canvas.scaleFactor = 0.5f;
    }
}