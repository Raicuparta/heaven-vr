using UnityEngine;

namespace HeavenVr;

public class LookAtCamera: MonoBehaviour
{
    private Camera camera;

    public static void Create(Transform transform, Camera camera)
    {
        var instance = transform.gameObject.AddComponent<LookAtCamera>();
        instance.camera = camera;
    }

    private void LateUpdate()
    {
        transform.LookAt(camera.transform, Vector3.up);
        transform.Rotate(Vector3.up * 180f);
    }
}