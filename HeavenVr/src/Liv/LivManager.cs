using HeavenVr.Stage;
using LIV.AvatarTrackers;
using LIV.SDK.Unity;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SpatialTracking;

namespace HeavenVr.Liv;

public class LivManager: MonoBehaviour
{
	private const float AnimationSpeedMultiplier = 0.003f;
	private readonly string[] _excludeBehaviours = {
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
	private PathfinderAvatarTrackers _avatarTrackers;
	private TrackedPoseDriver _poseDriver;

	public static void Create(VrStage stage, TrackedPoseDriver poseDriver)
	{
		var instance = new GameObject("LivManager").AddComponent<LivManager>();
		instance.transform.SetParent(stage.transform, false);
		instance._poseDriver = poseDriver;
	}

	private void Awake()
	{
		SDKShaders.LoadFromAssetBundle(VrAssetLoader.LivShadersBundle);
	}

	private void Update()
	{
		UpdateTrackers();

		// For some reason, calling this on Start or Invoke crashes the game. So calling it in Update instead.
		SetUp();
	}

	private void UpdateTrackers()
	{
		if (_avatarTrackers && RM.drifter)
		{
			_avatarTrackers.SetSpeed(RM.drifter.MovementVelocity.sqrMagnitude * AnimationSpeedMultiplier);
		}
	}

	private void SetUp()
	{
        if (_liv)
        {
            return;
        }

        SetUpTransform();
        SetUpLivComponent();
        SetUpAnimation();
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

	private Camera GetCameraPrefab()
	{
		var cameraPrefab = new GameObject("LivCameraPrefab").AddComponent<Camera>();
		cameraPrefab.gameObject.SetActive(false);
		cameraPrefab.gameObject.AddComponent<UniversalAdditionalCameraData>();
		cameraPrefab.nearClipPlane = 0.03f;
		return cameraPrefab;
	}

	private void SetUpAnimation()
	{
		var animationInstance = Instantiate(VrAssetLoader.RunAnimationPrefab, transform, false);
		_avatarTrackers = animationInstance.GetComponent<PathfinderAvatarTrackers>();
		_avatarTrackers.transform.Find("Wrapper");
	}
}