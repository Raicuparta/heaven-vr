using HeavenVr.Helpers;
using HeavenVr.Laser;
using HeavenVr.ModSettings;
using HeavenVr.VrUi;
using LIV.AvatarTrackers;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace HeavenVr.Stage;

public class VrStage: MonoBehaviour
{
    // public static VrStage Instance { get; private set; }
    public Camera VrCamera { get; set; }
    public UiTarget UiTarget { get; set; }
    [FormerlySerializedAs("AngleDelta")] public float angleDelta;
    [FormerlySerializedAs("CameraPoseDriver")] public TrackedPoseDriver cameraPoseDriver;
    [FormerlySerializedAs("DominantHand")] public VrHand dominantHand;
    [FormerlySerializedAs("NonDominantHand")] public VrHand nonDominantHand;
    public static VrStage Instance;

    [FormerlySerializedAs("AimLaser")] public VrAimLaser aimLaser;
    // private VrAimLaser directionLaser;
    private Vector3 _previousForward;
    private Transform _stageParent;
    private int _previousSelectableCount;
    private MouseLook _mouseLook;
    private LIV.SDK.Unity.LIV _liv;
    private Transform _livStage;
    private PathfinderAvatarTrackers _avatarTrackers;
    private readonly float _animationSpeedMultiplier = 0.003f;
    private Transform _runAnimationRotationTransform;
    
    public static void Create(Camera mainCamera)
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        
        var instance = new GameObject("VrStage").AddComponent<VrStage>();
        Instance = instance;

        instance.VrCamera = mainCamera;
        instance._stageParent = mainCamera.transform.parent;
        instance.transform.SetParent(instance._stageParent, false);
        mainCamera.transform.SetParent(instance.transform, false);
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI")); // TODO should have a separate UI camera;
        mainCamera.cullingMask = LayerHelper.GetMask(GameLayer.VrUi, mainCamera.cullingMask);
        
        mainCamera.transform.localEulerAngles = Vector3.up * mainCamera.transform.localEulerAngles.y;
        instance.transform.localScale = Vector3.one * 2f;

        if (RM.drifter)
        {
            mainCamera.transform.position = RM.drifter.GetFeetPosition();
        }
        instance.cameraPoseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
        instance.cameraPoseDriver.UseRelativeTransform = true;

        instance.dominantHand = VrHand.Create(instance.transform, instance.cameraPoseDriver, TrackedPoseDriver.TrackedPose.RightPose);
        instance.nonDominantHand = VrHand.Create(instance.transform, instance.cameraPoseDriver, TrackedPoseDriver.TrackedPose.LeftPose);
        instance.aimLaser = VrAimLaser.Create(instance.dominantHand.transform);

        instance._previousForward = mainCamera.transform.forward;
        instance.UiTarget = UiTarget.Create(instance, instance.nonDominantHand);
        instance._mouseLook = mainCamera.transform.parent.GetComponentInParent<MouseLook>();
    }

    private void Start()
    {
        // directionLaser = VrAimLaser.Create(NonDominantHand.transform);
        UpdatePreviousForward();
        Recenter();
    }

    // TODO move to LivManager
    private void SetUpLiv()
    {
        if (_liv)
        {
            return;
        }

        _livStage = new GameObject("LivStage").transform;
        _livStage.gameObject.SetActive(false);
        _livStage.transform.SetParent(transform, false);
        _livStage.transform.localPosition = cameraPoseDriver.originPose.position;
        _livStage.transform.localRotation = cameraPoseDriver.originPose.rotation;

        _liv = _livStage.gameObject.AddComponent<LIV.SDK.Unity.LIV>();
        var camPrefab = new GameObject("LivCameraPrefab").AddComponent<Camera>();
        camPrefab.gameObject.SetActive(false);
        camPrefab.gameObject.AddComponent<UniversalAdditionalCameraData>();
        _liv.MRCameraPrefab = camPrefab;
        _liv.HMDCamera = VrCamera;
        _liv.stage = _livStage;
        _liv.spectatorLayerMask = VrCamera.cullingMask;
        _liv.fixPostEffectsAlpha = true;
        _liv.excludeBehaviours = new []
        {
            "CameraDistanceCullingSettings",
            "PlayerCamera",
            "FlareLayer",
            "AmplifyOcclusionEffect",
            "CameraFOVManager",
            "UnityEngine.Rendering.PostProcessing.PostProcessLayer",
            "UnityEngine.Rendering.PostProcessing.PostProcessVolume",
            "UnityEngine.Rendering.Volume",
            "StreamingController",
            "MouseLook",
            "HeadBob",
            "ScannerEffect",
            "CameraStackPriority",
            "UnityEngine.SpatialTracking.TrackedPoseDriver",
            "ShakePosition",
            "CameraStackPriority"
        };
        _livStage.gameObject.SetActive(true);

        var animationInstance = Instantiate(VrAssetLoader.RunAnimationPrefab, _livStage, false);
        _avatarTrackers = animationInstance.GetComponent<PathfinderAvatarTrackers>();
        _runAnimationRotationTransform = _avatarTrackers.transform.Find("Wrapper");
    }

    private void Recenter()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private Vector3 GetMovementDirection()
    {
        // TODO use a laser for the movement direction.
        var trackedTransform = VrSettings.ControllerBasedMovementDirection.Value ? nonDominantHand.transform : VrCamera.transform;
        var trackedTransformOrigin = VrSettings.ControllerBasedMovementDirection.Value ? nonDominantHand.transform.parent : VrCamera.transform.parent;
        var forward = trackedTransformOrigin.InverseTransformDirection(trackedTransform.forward);
        forward.y = 0;
        return forward;
    }

    private void UpdatePreviousForward()
    {
        _previousForward = GetMovementDirection();
    }

    public void UpdateRotation()
    {
        if (!RM.acceptInput || !RM.acceptInputPauseMenu || !RM.drifter) return;

        angleDelta = Vector3.SignedAngle(_previousForward, GetMovementDirection(), Vector3.up);
        
        _stageParent.Rotate(Vector3.up, angleDelta);
        transform.Rotate(Vector3.up, -angleDelta);
        _mouseLook.originalRotation *= Quaternion.Euler(0, angleDelta, 0);

        if (_runAnimationRotationTransform)
        {
            _runAnimationRotationTransform.Rotate(Vector3.up, angleDelta);
        }

        UpdatePreviousForward();
    }

    private void Update()
    {
        UpdateRotation();
        
        if (_previousSelectableCount != Selectable.allSelectableCount)
        {
            foreach (var selectable in Selectable.allSelectablesArray)
            {
                if (selectable.GetComponent<BoxCollider>()) continue;
                var collider = selectable.gameObject.AddComponent<BoxCollider>();
                var rectSize = selectable.GetComponent<RectTransform>().sizeDelta;
                collider.size = new Vector3(rectSize.x, rectSize.y, 0.1f);
            }

            _previousSelectableCount = Selectable.allSelectableCount;
        }
        
        if (_avatarTrackers && RM.drifter)
        {
            _avatarTrackers.SetSpeed(RM.drifter.MovementVelocity.sqrMagnitude * _animationSpeedMultiplier);
        }
        
        // For some reason, calling this on Start or Invoke crashes the game. So calling it in Update instead.
        SetUpLiv();
    }
}