using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HeavenVr;

// This is an invisible object that's always(ish) somewhere in front of the camera.
// To be used as the position for UI elements that need to be visible or interacted with.
public class UiTarget : MonoBehaviour
{
    private const float rotationSmoothTime = 0.3f;
    private Vector3 previousForward;
    private Quaternion rotationVelocity;
    private Quaternion targetRotation;
    private Transform targetTransform;
    private readonly float minAngleDelta = 45f;
    private VrStage stage;
    private GameObject vrUiQuad;
    public Camera UiCamera { get; private set; }
    private static readonly Vector3 uiQuadSize = new(4f, 2.25f, 1);

    public static UiTarget Create(VrStage stage)
    {
        var instance = new GameObject(nameof(UiTarget)).AddComponent<UiTarget>();
        instance.transform.SetParent(stage.transform, false);
        instance.targetTransform = new GameObject("InteractiveUiTargetTransform").transform;
        instance.targetTransform.SetParent(instance.transform, false);
        instance.targetTransform.localPosition = new Vector3(0f, -1f, 3f);
        instance.stage = stage;
                
        instance.UiCamera = new GameObject("VrUiCamera").AddComponent<Camera>();
        instance.UiCamera.transform.SetParent(instance.targetTransform, false);
        instance.UiCamera.transform.localPosition = Vector3.forward * -4f;
        instance.UiCamera.orthographic = true;
        instance.UiCamera.clearFlags = CameraClearFlags.Depth;
        instance.UiCamera.cullingMask = LayerMask.GetMask("UI");;
        instance.UiCamera.targetTexture = instance.GetUiRenderTexture();
        instance.UiCamera.orthographicSize = uiQuadSize.y;
        return instance;
    }
    
    public void Start()
    {
        previousForward = GetCameraForward();
    }

    public RenderTexture GetUiRenderTexture()
    {
        if (!vrUiQuad)
        {
            vrUiQuad = Instantiate(VrAssetLoader.VrUiQuadPrefab, targetTransform, false);
            LayerHelper.SetLayer(vrUiQuad, GameLayer.VrUi);
            vrUiQuad.transform.localPosition = Vector3.zero;
            vrUiQuad.transform.localRotation = Quaternion.identity;
            vrUiQuad.transform.localScale = uiQuadSize;
            // VrMaterialHelper.MakeMaterialDrawOnTop(vrUiQuad.GetComponent<Renderer>().material);
            // vrUiQuad.GetComponent<Renderer>().material.shader = Shader.Find("NW/Particles/AlphaBlendDrawOnTop");
        }
        return vrUiQuad.GetComponent<Renderer>().material.mainTexture as RenderTexture;
    }

    private void Update()
    {
        UpdateTransform();
    }

    private Vector3 GetCameraForward()
    {
        return !stage.VrCamera ? Vector3.forward : MathHelper.GetProjectedForward(stage.VrCamera.transform);
    }

    private void UpdateTransform()
    {
        if (!stage.VrCamera) return;

        var cameraForward = GetCameraForward();
        var unsignedAngleDelta = Vector3.Angle(previousForward, cameraForward);
        targetTransform.localRotation = stage.CameraPoseDriver.originPose.rotation;

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

        transform.position = stage.VrCamera.transform.position;
    }
}