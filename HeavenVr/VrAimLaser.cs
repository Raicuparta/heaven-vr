using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavenVr;

public class VrAimLaser: MonoBehaviour
{
    private LineRenderer line;
    private const string lineShaderName = "Legacy Shaders/Particles/Alpha Blended";
    private const float rayDistance = 300f;
    private IVrInputBinding clickBinding;
    private Selectable selectedSelectable;

    public static VrAimLaser Create(Transform parent)
    {
        var instance = new GameObject("VrAimLaser").AddComponent<VrAimLaser>();
        instance.transform.SetParent(parent, false);
        instance.clickBinding = VrInputMap.GetBinding("MenuTabRight");

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

    private void Update()
    {
        CastRay();
    }

    private void CastRay()
    {
        var isHit = Physics.Raycast(
            transform.position,
            transform.forward,
            out var hit,
            rayDistance); // TODO clean up layers

        if (isHit)
        {
            line.SetPosition(1, Vector3.forward * hit.distance);

            var selectable = hit.collider.GetComponent<Selectable>();
            if (selectable)
            {
                var pointerData = new PointerEventData(EventSystem.current);
                selectable.Select();
                if (selectable != selectedSelectable)
                {
                    if (selectedSelectable)
                    {
                        selectedSelectable.OnPointerExit(pointerData);
                    }
                    selectable.OnPointerEnter(pointerData);
                    selectedSelectable = selectable;
                }
                if (clickBinding.WasPressedThisFrame)
                {
                    selectable.OnPointerDown(pointerData);
                }
            }
        }
        else
        {
            line.SetPosition(1, Vector3.forward * rayDistance);
        }
    }
}