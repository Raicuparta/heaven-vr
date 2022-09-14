using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using HeavenVr.Weapons;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr.Stage;

public class VrHand : MonoBehaviour
{
    private GameObject _movementDirection;
    private TrackedPoseDriver.TrackedPose _pose;

    public static VrHand Create(Transform parent, TrackedPoseDriver cameraPose, TrackedPoseDriver.TrackedPose pose)
    {
        // TODO clean this up. Separate in dominant vs non-dominant.
        if (pose == TrackedPoseDriver.TrackedPose.RightPose)
        {
            var instance = Instantiate(VrAssetLoader.RightHandPrefab).AddComponent<VrHand>();
            instance.transform.SetParent(parent, false);

            instance._pose = pose;

            var poseDriver = instance.GetComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, pose);
            poseDriver.UseRelativeTransform = true;
            poseDriver.originPose = cameraPose.originPose;

            WeaponSwapper.Create(instance.transform.Find("Wrapper").gameObject);

            return instance;
        }
        else
        {
            var instance = new GameObject($"VrHand-{pose}").AddComponent<VrHand>();
            instance.transform.SetParent(parent, false);

            instance._pose = pose;

            var poseDriver = instance.gameObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, pose);
            poseDriver.UseRelativeTransform = true;
            poseDriver.originPose = cameraPose.originPose;

            instance._movementDirection = Instantiate(VrAssetLoader.MovementDirectionPrefab, instance.transform, false);
            instance._movementDirection.name = "MovementDirection"; // TODO don't rely on names.

            return instance;
        }
    }

    private void Update()
    {
        if (_pose == TrackedPoseDriver.TrackedPose.LeftPose)
            _movementDirection.SetActive(VrSettings.ControllerBasedMovementDirection.Value && !PauseHelper.IsPaused());
    }
}