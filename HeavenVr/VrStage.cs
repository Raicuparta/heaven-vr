using System.Collections.Generic;
using HarmonyLib;
using LIV.AvatarTrackers;
using LIV.SDK.Unity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace HeavenVr;

public class VrStage: MonoBehaviour
{
    public static VrStage Instance { get; private set; }
    public Camera VrCamera { get; set; }
    public UiTarget UiTarget { get; set; }
    public float AngleDelta;
    public TrackedPoseDriver CameraPoseDriver;
    public VrHand DominantHand;
    public VrHand NonDominantHand;

    public VrAimLaser AimLaser;
    private VrAimLaser directionLaser;
    private Vector3 previousForward;
    private Transform stageParent;
    private int previousSelectableCount;
    private MouseLook mouseLook;
    private bool isHandOriented = false;
    private LIV.SDK.Unity.LIV liv;
    private Transform livStage;
    private PathfinderAvatarTrackers avatarTrackers;
    private FirstPersonDrifter firstPersonDrifter;
    private float animationSpeedMultiplier = 0.003f;
    private Transform runAnimationRotationTransform;
    public static SDKUniversalRenderFeature LivRenderFeature;
    
    public static void Create(Camera camera)
    {
        Instance = new GameObject("VrStage").AddComponent<VrStage>();

        Instance.previousForward = camera.transform.forward;
        Instance.UiTarget = UiTarget.Create(Instance);
        Instance.mouseLook = camera.transform.parent.GetComponentInParent<MouseLook>();

        Instance.VrCamera = camera;
        Instance.stageParent = camera.transform.parent;
        Instance.transform.SetParent(Instance.stageParent, false);
        camera.transform.SetParent(Instance.transform, false);
        camera.cullingMask |= 1 << LayerMask.NameToLayer("UI"); // TODO should have a separate UI camera;
        
        camera.transform.localEulerAngles = Vector3.up * camera.transform.localEulerAngles.y;
        Instance.CameraPoseDriver = camera.gameObject.AddComponent<TrackedPoseDriver>();
        Instance.CameraPoseDriver.UseRelativeTransform = true;

        
        LivRenderFeature = ScriptableObject.CreateInstance<SDKUniversalRenderFeature>();
    }

    private void Start()
    {
        DominantHand = VrHand.Create(transform, CameraPoseDriver, TrackedPoseDriver.TrackedPose.RightPose);
        NonDominantHand = VrHand.Create(transform, CameraPoseDriver, TrackedPoseDriver.TrackedPose.LeftPose);
        AimLaser = VrAimLaser.Create(DominantHand.transform);
        directionLaser = VrAimLaser.Create(NonDominantHand.transform);
        UpdatePreviousForward();
        Recenter();
    }

    private void SetUpLiv()
    {
        if (liv)
        {
            Destroy(liv.gameObject);
        }

        livStage = new GameObject("LivStage").transform;
        livStage.gameObject.SetActive(false);
        livStage.transform.SetParent(transform, false);
        livStage.transform.localPosition = CameraPoseDriver.originPose.position;
        livStage.transform.localRotation = CameraPoseDriver.originPose.rotation;

        liv = livStage.gameObject.AddComponent<LIV.SDK.Unity.LIV>();
        var camPrefab = new GameObject("LivCameraPrefab").AddComponent<Camera>();
        camPrefab.gameObject.SetActive(false);
        camPrefab.gameObject.AddComponent<UniversalAdditionalCameraData>();
        liv.MRCameraPrefab = camPrefab;
        liv.HMDCamera = VrCamera;
        liv.stage = livStage;
        liv.spectatorLayerMask = VrCamera.cullingMask;
        liv.fixPostEffectsAlpha = true;
        liv.excludeBehaviours = new []
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
            "CameraStackPriority",
        };
        livStage.gameObject.SetActive(true);

        var animationInstance = Instantiate(VrAssetLoader.RunAnimationPrefab, livStage, false);
        avatarTrackers = animationInstance.GetComponent<PathfinderAvatarTrackers>();
        runAnimationRotationTransform = avatarTrackers.transform.Find("Wrapper");
    }

    private void Recenter()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private Vector3 GetMovementDirection()
    {
        var trackedTransform = isHandOriented ? directionLaser.transform : VrCamera.transform;
        var trackedTransformOrigin = isHandOriented ? NonDominantHand.transform.parent : VrCamera.transform.parent;
        var forward = trackedTransformOrigin.InverseTransformDirection(trackedTransform.forward);
        forward.y = 0;
        return forward;
    }

    private void UpdatePreviousForward()
    {
        previousForward = GetMovementDirection();
    }

    public void UpdateRotation()
    {
        if (!RM.acceptInput || !RM.acceptInputPauseMenu || !RM.drifter) return;

        AngleDelta = Vector3.SignedAngle(previousForward, GetMovementDirection(), Vector3.up);
        
        stageParent.Rotate(Vector3.up, AngleDelta);
        transform.Rotate(Vector3.up, -AngleDelta);
        mouseLook.originalRotation *= Quaternion.Euler(0, AngleDelta, 0);

        if (runAnimationRotationTransform)
        {
            runAnimationRotationTransform.Rotate(Vector3.up, AngleDelta);
        }

        UpdatePreviousForward();
    }

    private void Update()
    {
        UpdateRotation();
        
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
        
        if (avatarTrackers && RM.drifter)
        {
            avatarTrackers.SetSpeed(RM.drifter.MovementVelocity.sqrMagnitude * animationSpeedMultiplier);
        }

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            SetUpLiv();
        }
    }
}