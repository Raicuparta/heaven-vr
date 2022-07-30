using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace HeavenVr;

public class VrStage: MonoBehaviour
{
    public static VrStage Instance { get; private set; }
    
    private Camera camera;
    private Vector3 previousForward;
    private Transform stageParent;
    private TrackedPoseDriver cameraPoseDriver;
    
    public static void Create(Camera camera)
    {
        Instance = new GameObject("VrStage").AddComponent<VrStage>();

        Instance.camera = camera;
        Instance.stageParent = camera.transform.parent;
        Instance.transform.SetParent(Instance.stageParent, false);
        camera.transform.SetParent(Instance.transform, false);
        
        Instance.cameraPoseDriver = camera.gameObject.AddComponent<TrackedPoseDriver>();
        Instance.cameraPoseDriver.UseRelativeTransform = true;

        Instance.previousForward = camera.transform.forward;
    }

    private void Start()
    {
        UpdatePreviousForward();
        VrAimLaser.Create(transform, cameraPoseDriver);
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
        var forward = camera.transform.parent.InverseTransformDirection(camera.transform.forward);
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
}