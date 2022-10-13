using System;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using HeavenVr.Weapons;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr.Stage;

public class VrHand : MonoBehaviour
{
    private const float SmoothingScaler = 0.9f;
    private const float NonDominantSmoothing = 0.1f;

    private GameObject _movementDirection;
    private bool _isDominant;
    private TrackedPoseDriver _poseDriver;

    public static VrHand Create(Transform parent, TrackedPoseDriver cameraPose, bool isDominant)
    {
        VrHand instance;

        // TODO clean this up. Separate in dominant vs non-dominant.
        if (isDominant)
        {
            instance = Instantiate(VrAssetLoader.DominantHandPrefab).AddComponent<VrHand>();
            WeaponSwapper.Create(instance.transform.Find("Wrapper").gameObject);
        }
        else
        {
            // TODO there should also be a non-dominant hand prefab.
            instance = new GameObject($"VrHand-NonDominant").AddComponent<VrHand>();
            instance._movementDirection = Instantiate(VrAssetLoader.MovementDirectionPrefab, instance.transform, false);
            instance._movementDirection.name = "MovementDirection"; // TODO don't rely on names.
        }
        
        var existingPoseDriver = instance.GetComponent<TrackedPoseDriver>();
        if (existingPoseDriver)
        {
            Destroy(existingPoseDriver);
        }
        
        instance.transform.SetParent(parent, false);
        instance._isDominant = isDominant;

        var poseDriverObject = new GameObject(isDominant ? "DominantPoseDriver" : "NonDominantPoseDriver");
        poseDriverObject.transform.SetParent(parent, false);
        instance._poseDriver = poseDriverObject.AddComponent<TrackedPoseDriver>();
        instance._poseDriver.UseRelativeTransform = true;
        instance._poseDriver.originPose = cameraPose.originPose;

        return instance;
    }

    private void OnEnable()
    {
        VrSettings.LeftHandedMode.SettingChanged += OnHandednessChanged;
    }

    private void Start()
    {
        UpdatePose();
    }

    private void OnDisable()
    {
        VrSettings.LeftHandedMode.SettingChanged -= OnHandednessChanged;
    }

    private void OnHandednessChanged(object sender, EventArgs e)
    {
        UpdatePose();
    }

    private void UpdatePose()
    {
        _poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, IsLeftHandedLeftPose(_isDominant)
            ? TrackedPoseDriver.TrackedPose.LeftPose
            : TrackedPoseDriver.TrackedPose.RightPose);
    }

    private void Update()
    {
        UpdateHandedness();
        UpdateTransform();
    }
    
    private void UpdateHandedness()
    {
        if (_isDominant) return;

        _movementDirection.SetActive(VrSettings.ControllerBasedMovementDirection.Value && !PauseHelper.IsPaused());
    }
    
    private void UpdateTransform()
    {
        var smoothing = _isDominant ? 1 - VrSettings.AimSmoothing.Value * SmoothingScaler : NonDominantSmoothing;
        
        var targetPosition = _poseDriver.transform.localPosition;
        var distance = Vector3.Distance(transform.localPosition, targetPosition);
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition,
            distance * smoothing);

        var targetRotation = _poseDriver.transform.localRotation;
        
        if (!_isDominant && VrSettings.ControllerBasedMovementDirection.Value && !PauseHelper.IsPaused())
        {
            // We don't want smoothing to affect controller-based movement direction.
            // TODO: it would be cleaner to move the controller-based direction pointer to outside the hand.
            transform.localRotation = targetRotation;
            return;
        }
        
        var angleDelta = Quaternion.Angle(transform.localRotation, targetRotation);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation,
            angleDelta * smoothing);
    }

    public static bool IsLeftHandedLeftPose(bool isDominantHand)
    {
        return isDominantHand ? VrSettings.LeftHandedMode.Value : !VrSettings.LeftHandedMode.Value;
    }

    public static bool IsSwapSticksLeftPose(bool isDominantHand)
    {
        return isDominantHand ? VrSettings.SwapSticks.Value : !VrSettings.SwapSticks.Value;
    }
}