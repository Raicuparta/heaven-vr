using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr;

public class VrAimLaser: MonoBehaviour
{
    public static Transform Laser { get; private set; }
    private LineRenderer line;

    public static void Create(Transform parent)
    {
        var instance = new GameObject("VrAimLaser").AddComponent<VrAimLaser>();
        instance.transform.SetParent(parent, false);
    }
    
    private void Awake()
    {
        var material = FindObjectOfType<LineRenderer>().material;
        line = new GameObject("VrLaserLine").AddComponent<LineRenderer>();
        Laser = line.transform;
        Laser.SetParent(transform, false);
        Laser.localEulerAngles = Vector3.right * 60f;
        line.useWorldSpace = false;
        line.startWidth = 0.05f;
        line.endWidth = 0.01f;
        line.SetPositions(new []{ Vector3.zero, Vector3.forward * 100f });
        line.material = material;

        var poseDriver = gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            TrackedPoseDriver.TrackedPose.RightPose);
    }
}