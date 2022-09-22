using System;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using HeavenVr.Weapons;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr.Stage;

public class VrHand : MonoBehaviour
{
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
            instance._poseDriver = instance.GetComponent<TrackedPoseDriver>();
        }
        else
        {
            instance = new GameObject($"VrHand-NonDominant").AddComponent<VrHand>();
            instance._movementDirection = Instantiate(VrAssetLoader.MovementDirectionPrefab, instance.transform, false);
            instance._movementDirection.name = "MovementDirection"; // TODO don't rely on names.
            instance._poseDriver = instance.gameObject.AddComponent<TrackedPoseDriver>();
        }
        
        instance.transform.SetParent(parent, false);
        instance._isDominant = isDominant;

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

    private TrackedPoseDriver.TrackedPose GetPose()
    {
        var isLeftPose = _isDominant ? VrSettings.LeftHandedMode.Value : !VrSettings.LeftHandedMode.Value;
        
        return isLeftPose ? TrackedPoseDriver.TrackedPose.LeftPose : TrackedPoseDriver.TrackedPose.RightPose;
    }

    private void UpdatePose()
    {
        _poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, GetPose());;
    }

    private void Update()
    {
        if (!_isDominant)
        {
            _movementDirection.SetActive(VrSettings.ControllerBasedMovementDirection.Value && !PauseHelper.IsPaused());
        }
    }
}