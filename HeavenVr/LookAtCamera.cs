using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SpatialTracking;

namespace HeavenVr;

public class LookAtCamera: MonoBehaviour
{
    private Camera camera;
    private TrackedPoseDriver poseDriver;

    public static void Create(Transform transform, Camera camera)
    {
        var instance = transform.gameObject.AddComponent<LookAtCamera>();
        instance.camera = camera;
        instance.poseDriver = camera.GetComponent<TrackedPoseDriver>();
    }

    private void Update()
    {
        // transform.LookAt(camera.transform, Vector3.up);
    }
}