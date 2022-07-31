using UnityEngine;

namespace HeavenVr;

// This is an invisible object that's always(ish) somewhere in front of the camera.
// To be used as the position for UI elements that need to be visible or interacted with.
public class UiTarget : MonoBehaviour
{
    private const float rotationSmoothTime = 0.3f;
    private Vector3 previousForward;
    private Quaternion rotationVelocity;
    private Quaternion targetRotation;
    public Transform TargetTransform { get; private set; }
    private readonly float minAngleDelta = 45f;
    private VrStage stage;

    public static UiTarget Create(VrStage stage)
    {
        var instance = new GameObject(nameof(UiTarget)).AddComponent<UiTarget>();
        instance.transform.SetParent(stage.transform, false);
        instance.TargetTransform = new GameObject("InteractiveUiTargetTransform").transform;
        instance.TargetTransform.SetParent(instance.transform, false);
        instance.TargetTransform.localPosition = Vector3.forward * 3f;
        instance.stage = stage;
        return instance;
    }
    
    public void Start()
    {
        previousForward = GetCameraForward();
    }

    private void Update()
    {
        UpdateTransform();
    }

    private Vector3 GetCameraForward()
    {
        return !stage.Camera ? Vector3.forward : MathHelper.GetProjectedForward(stage.Camera.transform);
    }

    private void UpdateTransform()
    {
        if (!stage.Camera) return;

        var cameraForward = GetCameraForward();
        var unsignedAngleDelta = Vector3.Angle(previousForward, cameraForward);
        TargetTransform.localRotation = stage.CameraPoseDriver.originPose.rotation;

        if (unsignedAngleDelta > minAngleDelta)
        {
            targetRotation = Quaternion.LookRotation(cameraForward, stage.CameraPoseDriver.transform.parent.rotation * stage.CameraPoseDriver.originPose.rotation * Vector3.up);
            previousForward = cameraForward;
        }

        transform.rotation = MathHelper.SmoothDamp(
            transform.rotation,
            targetRotation,
            ref rotationVelocity,
            rotationSmoothTime);

        transform.position = stage.Camera.transform.position;
    }
}