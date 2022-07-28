using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr;

public class VrAimLaser: MonoBehaviour
{
    public static VrAimLaser Instance { get; set; }
    private LineRenderer line;

    public static void Create(Transform parent)
    {
        Instance = new GameObject("VrAimLaser").AddComponent<VrAimLaser>();
        Instance.transform.SetParent(parent, false);
    }
    
    private void Awake()
    {
        var material = FindObjectOfType<LineRenderer>().material;
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.SetPositions(new []{ Vector3.zero, Vector3.forward * 100f });
        line.material = material;

        var poseDriver = gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            TrackedPoseDriver.TrackedPose.RightPose);
    }
}