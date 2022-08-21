using HeavenVr.Weapons;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr.Stage;

public class VrHand: MonoBehaviour
{
    public static VrHand Create(Transform parent, TrackedPoseDriver cameraPose, TrackedPoseDriver.TrackedPose pose)
    {
        if (pose == TrackedPoseDriver.TrackedPose.RightPose)
        {
            var instance = Instantiate(VrAssetLoader.RightHandPrefab).AddComponent<VrHand>();
            instance.transform.SetParent(parent, false);

            var poseDriver = instance.GetComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, pose);
            poseDriver.UseRelativeTransform = true;
            poseDriver.originPose = cameraPose.originPose;
            
            WeaponSwapper.Create(instance.gameObject);
            
            return instance;
        }
        else
        {
            var instance = new GameObject($"VrHand-{pose}").AddComponent<VrHand>();
            instance.transform.SetParent(parent, false);
            
            var poseDriver = instance.gameObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, pose);
            poseDriver.UseRelativeTransform = true;
            poseDriver.originPose = cameraPose.originPose;

            return instance;
        }
    }
}