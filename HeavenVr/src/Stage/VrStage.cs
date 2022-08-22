using HeavenVr.Helpers;
using HeavenVr.Laser;
using HeavenVr.VrUi;
using LIV.AvatarTrackers;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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

    private const float AnimationSpeedMultiplier = 0.003f;
    private Transform _stageParent;
    private int _previousSelectableCount;
    private LIV.SDK.Unity.LIV _liv;
    private Transform _livStage;
    private PathfinderAvatarTrackers _avatarTrackers;
    private VrHand _dominantHand;
    private VrHand _nonDominantHand;

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
        instance.CameraPoseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
        instance.CameraPoseDriver.UseRelativeTransform = true;

        instance._dominantHand = VrHand.Create(instance.transform, instance.CameraPoseDriver, TrackedPoseDriver.TrackedPose.RightPose);
        instance._nonDominantHand = VrHand.Create(instance.transform, instance.CameraPoseDriver, TrackedPoseDriver.TrackedPose.LeftPose);
        instance.aimLaser = VrAimLaser.Create(instance._dominantHand.transform);

        instance.UiTarget = UiTarget.Create(instance, instance._nonDominantHand);
        mainCamera.transform.parent.GetComponentInParent<MouseLook>();
    }

    private void Start()
    {
        Recenter();
        movementDirectionPointer = _nonDominantHand.transform; // TODO add movement laser.
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
        _livStage.transform.localPosition = CameraPoseDriver.originPose.position;
        _livStage.transform.localRotation = CameraPoseDriver.originPose.rotation;

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
        _avatarTrackers.transform.Find("Wrapper");
    }

    private void Recenter()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private void Update()
    {
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
            _avatarTrackers.SetSpeed(RM.drifter.MovementVelocity.sqrMagnitude * AnimationSpeedMultiplier);
        }
        
        // For some reason, calling this on Start or Invoke crashes the game. So calling it in Update instead.
        SetUpLiv();
    }
}