using HeavenVr.Stage;
using LIV.SDK.Unity;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SpatialTracking;

namespace HeavenVr.Liv;

public class LivManager : MonoBehaviour
{
    private readonly string[] _excludeBehaviours =
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

    private LIV.SDK.Unity.LIV _liv;
    private TrackedPoseDriver _poseDriver;

    public static void Create(VrStage stage, TrackedPoseDriver poseDriver)
    {
        var instance = new GameObject("LivManager").AddComponent<LivManager>();
        instance.transform.SetParent(stage.transform, false);
        instance._poseDriver = poseDriver;
    }

    private void SetUp()
    {
        if (_liv) return;

        SetUpTransform();
        SetUpLivComponent();
    }

    private void Awake()
    {
        SDKShaders.LoadFromAssetBundle(VrAssetLoader.LivShadersBundle);
    }

    private void Update()
    {
        // For some reason, calling this on Start or Invoke crashes the game. So calling it in Update instead.
        SetUp();
    }

    private void SetUpTransform()
    {
        transform.localPosition = _poseDriver.originPose.position;
        transform.localRotation = _poseDriver.originPose.rotation;
    }

    private void SetUpLivComponent()
    {
        gameObject.SetActive(false);
        var mainCamera = _poseDriver.GetComponent<Camera>();
        _liv = gameObject.AddComponent<LIV.SDK.Unity.LIV>();
        _liv.MRCameraPrefab = GetCameraPrefab();
        _liv.HMDCamera = mainCamera;
        _liv.stage = transform;
        _liv.spectatorLayerMask = mainCamera.cullingMask;
        _liv.fixPostEffectsAlpha = true;
        _liv.excludeBehaviours = _excludeBehaviours;
        gameObject.SetActive(true);
    }

    private static Camera GetCameraPrefab()
    {
        var cameraPrefab = new GameObject("LivCameraPrefab").AddComponent<Camera>();
        cameraPrefab.gameObject.SetActive(false);
        cameraPrefab.gameObject.AddComponent<UniversalAdditionalCameraData>();
        cameraPrefab.nearClipPlane = 0.03f;
        return cameraPrefab;
    }
}