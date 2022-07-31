using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;

namespace HeavenVr;

public class VrStage: MonoBehaviour
{
    public static VrStage Instance { get; private set; }
    public Camera Camera { get; set; }
    public UiTarget UiTarget { get; set; }

    
    private Vector3 previousForward;
    private Transform stageParent;
    public TrackedPoseDriver CameraPoseDriver;
    private int previousSelectableCount;
    
    public static void Create(Camera camera)
    {
        Instance = new GameObject("VrStage").AddComponent<VrStage>();

        Instance.Camera = camera;
        Instance.stageParent = camera.transform.parent;
        Instance.transform.SetParent(Instance.stageParent, false);
        camera.transform.SetParent(Instance.transform, false);
        camera.cullingMask |= 1 << LayerMask.NameToLayer("UI"); // TODO should have a separate UI camera;
        
        camera.transform.localEulerAngles = Vector3.up * camera.transform.localEulerAngles.y;
        Instance.CameraPoseDriver = camera.gameObject.AddComponent<TrackedPoseDriver>();
        Instance.CameraPoseDriver.UseRelativeTransform = true;

        Instance.previousForward = camera.transform.forward;

        Instance.UiTarget = UiTarget.Create(Instance);
    }

    private void Start()
    {
        UpdatePreviousForward();
        VrAimLaser.Create(transform, CameraPoseDriver);
        Recenter();
    }

    private void Recenter()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private Vector3 GetProjectedForward()
    {
        var forward = Camera.transform.parent.InverseTransformDirection(Camera.transform.forward);
        forward.y = 0;
        return forward;
    }

    private void UpdatePreviousForward()
    {
        previousForward = GetProjectedForward();
    }

    public void UpdateRotation()
    {
        var angleDelta = Vector3.SignedAngle(previousForward, GetProjectedForward(), Vector3.up);
        stageParent.Rotate(Vector3.up, angleDelta);
        
        transform.Rotate(Vector3.up, -angleDelta);

        UpdatePreviousForward();
    }

    private void Update()
    {
        if (previousSelectableCount != Selectable.allSelectableCount)
        {
            foreach (var selectable in Selectable.allSelectablesArray)
            {
                if (selectable.GetComponent<BoxCollider>()) continue;
                var collider = selectable.gameObject.AddComponent<BoxCollider>();
                var rectSize = selectable.GetComponent<RectTransform>().sizeDelta;
                collider.size = new Vector3(rectSize.x, rectSize.y, 0.1f);
            }

            previousSelectableCount = Selectable.allSelectableCount;
        }
    }
}