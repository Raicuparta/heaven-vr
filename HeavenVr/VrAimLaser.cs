using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr;

public class VrAimLaser: MonoBehaviour
{
    public static Transform Laser { get; private set; }
    private LineRenderer line;
    private const string lineShaderName = "Legacy Shaders/Particles/Alpha Blended";

    public static void Create(Transform parent, TrackedPoseDriver cameraPose)
    {
        var instance = new GameObject("VrAimLaser").AddComponent<VrAimLaser>();
        instance.transform.SetParent(parent, false);
        
        var poseDriver = instance.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            TrackedPoseDriver.TrackedPose.RightPose);
        poseDriver.UseRelativeTransform = true;
        poseDriver.originPose = cameraPose.originPose;
    }
    
    private void Awake()
    {
        line = new GameObject("VrLaserLine").AddComponent<LineRenderer>();
        Laser = line.transform;
        Laser.SetParent(transform, false);
        Laser.localEulerAngles = Vector3.right * 60f;
        line.useWorldSpace = false;
        line.startWidth = 0.01f;
        line.endWidth = 0f;
        line.SetPositions(new []{ Vector3.zero, Vector3.forward * 100f });
        line.material.shader = Shader.Find(lineShaderName);


    }
}