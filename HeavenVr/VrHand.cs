﻿using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr;

public class VrHand: MonoBehaviour
{
    public static VrHand Create(Transform parent, TrackedPoseDriver cameraPose, TrackedPoseDriver.TrackedPose pose)
    {
        var instance = new GameObject($"VrHand-{pose}").AddComponent<VrHand>();
        instance.transform.SetParent(parent, false);
        
        var poseDriver = instance.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            pose);
        poseDriver.UseRelativeTransform = true;
        poseDriver.originPose = cameraPose.originPose;

        return instance;
    }
}