using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace HeavenVr;

public class VrStage: MonoBehaviour
{
    public static VrStage Instance { get; private set; }
    
    private Camera playerCamera;
    private Vector3 previousForward;
    private Transform stageParent;
    
    public static void Create(PlayerCamera playerCamera)
    {
        Instance = new GameObject("VrStage").AddComponent<VrStage>();

        Instance.playerCamera = playerCamera.PlayerCam;
        Instance.stageParent = playerCamera.transform.parent;
        Instance.transform.SetParent(Instance.stageParent, false);
        playerCamera.transform.SetParent(Instance.transform, false);
        
        var poseDriver = playerCamera.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.UseRelativeTransform = true;

        Instance.previousForward = playerCamera.transform.forward;
    }

    private void Start()
    {
        UpdatePreviousForward();
        VrAimLaser.Create(transform);
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
        var forward = playerCamera.transform.parent.InverseTransformDirection(playerCamera.transform.forward);
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