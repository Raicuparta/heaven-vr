using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr;

public class VrStage: MonoBehaviour
{
    public static VrStage Instance { get; private set; }
    
    private Camera playerCamera;
    private Vector3 previousForward;
    private Transform cameraHolder;
    private FirstPersonDrifter firstPersonDrifter;
    
    public static void Create(PlayerCamera playerCamera)
    {
        Instance = new GameObject("VrStage").AddComponent<VrStage>();
        Instance.playerCamera = playerCamera.PlayerCam;

        Instance.cameraHolder = playerCamera.transform.parent;

        Instance.transform.SetParent(Instance.cameraHolder, false);
        playerCamera.transform.SetParent(Instance.transform, false);

        Instance.firstPersonDrifter = playerCamera.GetComponentInParent<FirstPersonDrifter>();
        
        var poseDriver = playerCamera.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.UseRelativeTransform = true;

        Instance.previousForward = playerCamera.transform.forward;
    }

    private void Start()
    {
        UpdatePreviousForward();
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
        cameraHolder.Rotate(Vector3.up, angleDelta);
        
        transform.Rotate(Vector3.up, -angleDelta);

        UpdatePreviousForward();
    }
}