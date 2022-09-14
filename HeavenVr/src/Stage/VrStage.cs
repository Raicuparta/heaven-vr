using HeavenVr.Helpers;
using HeavenVr.Laser;
using HeavenVr.Liv;
using HeavenVr.ModSettings;
using HeavenVr.Player;
using HeavenVr.VrUi;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;

namespace HeavenVr.Stage;

public class VrStage: MonoBehaviour
{
    public Camera VrCamera { get; private set; }
    public UiTarget UiTarget { get; private set; }
    public TrackedPoseDriver CameraPoseDriver { get; private set; }
    public static VrStage Instance { get; private set; }

    public VrAimLaser aimLaser;
    public Transform movementDirectionPointer;

    private Transform _stageParent;
    private int _previousSelectableCount;
    private VrHand _dominantHand;
    private VrHand _nonDominantHand;

    public static void Create(Camera mainCamera)
    {
        if (Instance)
        {
            Instance.VrCamera.transform.SetParent(Instance._stageParent, false);
            Destroy(Instance.VrCamera.GetComponent<TrackedPoseDriver>());
            Destroy(Instance.gameObject);
        }
        
        var instance = new GameObject("VrStage").AddComponent<VrStage>();
        Instance = instance;

        instance.VrCamera = mainCamera;
        instance._stageParent = mainCamera.transform.parent;
        instance.transform.SetParent(instance._stageParent, false);
        mainCamera.transform.SetParent(instance.transform, false);
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        mainCamera.cullingMask = LayerHelper.GetMask(GameLayer.VrUi, mainCamera.cullingMask);
        
        mainCamera.transform.localEulerAngles = Vector3.up * mainCamera.transform.localEulerAngles.y;
        instance.transform.localScale = Vector3.one * 2f;

        if (RM.drifter)
        {
            mainCamera.transform.position = RM.drifter.GetFeetPosition();
        }

        instance.CameraPoseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
        instance.CameraPoseDriver.UseRelativeTransform = true;

        instance._dominantHand = VrHand.Create(instance.transform, instance.CameraPoseDriver, TrackedPoseDriver.TrackedPose.RightPose);
        instance._nonDominantHand = VrHand.Create(instance.transform, instance.CameraPoseDriver, TrackedPoseDriver.TrackedPose.LeftPose);
        instance.aimLaser = VrAimLaser.Create(instance._dominantHand.transform);
        
        PlayerBodyIkController.Create(mainCamera.transform, instance._nonDominantHand.transform,  instance._dominantHand.transform);

        instance.UiTarget = UiTarget.Create(instance, instance._nonDominantHand);
        
        LivManager.Create(instance, instance.CameraPoseDriver);
    }

    private void Start()
    {
        SetUpRotationDummy();

        // TODO clean up with separate non-dominant hand script.
        movementDirectionPointer = _nonDominantHand.transform.Find("MovementDirection/Laser");
    }
    
    public Vector3 GetMovementDirection()
    {
        if (!movementDirectionPointer || !VrCamera) return Vector3.forward;
        
        var trackedTransform = VrSettings.ControllerBasedMovementDirection.Value ? movementDirectionPointer.transform : VrCamera.transform;
        var forward = trackedTransform.forward;
        forward.y = 0;
        return forward;
    }

    public void RecenterRotation()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private void RecenterPosition()
    {
        if (RM.drifter == null) return;
        
        transform.position -= VrCamera.transform.position - (RM.drifter.transform.position + Vector3.up * 1.456f);
    }

    private bool _isRecentered;

    private void Update()
    {
        UpdateRecenter();
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (PauseHelper.IsPaused()) return;

        var angle = Vector3.SignedAngle(transform.parent.forward, GetMovementDirection(), Vector3.up);
        
        RM.drifter.mouseLookX.AddFrameRotation(angle, 0);
        
        transform.Rotate(Vector3.up, -angle);
    }

    private void UpdateRecenter()
    {
        if (_isRecentered) return;

        RecenterPosition();
        _isRecentered = true;
    }

    // There's a lot of code that depends on this "cameraHolder", but needs it to be afttached to the player body.
    // There's also code that tries to modify the cameraHolder transform.
    // Since in VR the camera can be far from the body and can't be moved / rotated manually,
    // we replace the cameraHolder with a dummy object that's always attached.
    private void SetUpRotationDummy()
    {
        if (RM.drifter == null) return;
        
        var dummy = new GameObject("VrCameraRotationDummy").transform;
        dummy.SetParent(transform.parent, false);
        dummy.transform.localPosition = Vector3.zero;
        dummy.transform.localRotation = Quaternion.identity;
        RM.drifter.m_cameraHolder = dummy;
    }
}