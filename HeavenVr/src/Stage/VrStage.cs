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
        RecenterRotation();
        movementDirectionPointer = _nonDominantHand.transform; // TODO add movement laser.
    }
    
    public Vector3 GetMovementDirection()
    {
        if (!movementDirectionPointer || !VrCamera) return Vector3.forward;
        
        // TODO use a laser for the movement direction.
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
        if (!_isRecentered)
        {
            RecenterPosition();
            _isRecentered = true;
        }
        
        // TODO is this still needed?
        if (_previousSelectableCount == Selectable.allSelectableCount) return;

        foreach (var selectable in Selectable.allSelectablesArray)
        {
            if (selectable.GetComponent<BoxCollider>()) continue;
            var collider = selectable.gameObject.AddComponent<BoxCollider>();
            var rectSize = selectable.GetComponent<RectTransform>().sizeDelta;
            collider.size = new Vector3(rectSize.x, rectSize.y, 0.1f);
        }

        _previousSelectableCount = Selectable.allSelectableCount;
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