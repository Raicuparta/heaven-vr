using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;

namespace HeavenVr;

public class VrAimLaser: MonoBehaviour
{
    public static Transform Laser { get; private set; }
    private LineRenderer line;
    private const string lineShaderName = "Legacy Shaders/Particles/Alpha Blended";
    private const float rayDistance = 300f;
    private IVrInputBinding clickBinding;
    private Selectable selectedSelectable;

    public static VrAimLaser Create(Transform parent, TrackedPoseDriver cameraPose)
    {
        var instance = new GameObject("VrAimLaser").AddComponent<VrAimLaser>();
        instance.transform.SetParent(parent, false);
        
        var poseDriver = instance.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            TrackedPoseDriver.TrackedPose.RightPose);
        poseDriver.UseRelativeTransform = true;
        poseDriver.originPose = cameraPose.originPose;
        instance.clickBinding = VrInputMap.GetBinding("MenuTabRight");

        return instance;
    }
    
    private void Awake()
    {
        line = new GameObject("VrLaserLine").AddComponent<LineRenderer>();
        Laser = line.transform;
        Laser.SetParent(transform, false);
        Laser.localEulerAngles = Vector3.right * 60f;
        line.useWorldSpace = false;
        line.startWidth = 0.01f;
        line.endWidth = 0f;
        line.SetPositions(new []{ Vector3.zero, Vector3.forward * 100f });
        line.material.shader = Shader.Find(lineShaderName);
        // Using Ignore Raycast layer because it's visible in map camera and player camera.
        // Might be better to use a custom layer, but seems like they're all being used.
        line.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Update()
    {
        CastRay();
    }

    private void CastRay()
    {
        var isHit = Physics.Raycast(
            VrAimLaser.Laser.position,
            VrAimLaser.Laser.forward,
            out var hit,
            rayDistance); // TODO clean up layers

        if (isHit)
        {
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
    }
}