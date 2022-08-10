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
    private VrHand hand;
    public static Camera PlayerHudCamera; // TODO no static

    public static UiTarget Create(VrStage stage, VrHand hand)
    {
        Debug.Log($"Create {hand == null}");
        
        var instance = new GameObject(nameof(UiTarget)).AddComponent<UiTarget>();
        instance.transform.SetParent(stage.transform, false);
        instance.targetTransform = new GameObject("InteractiveUiTargetTransform").transform;
        instance.targetTransform.SetParent(instance.transform, false);
        instance.targetTransform.localPosition = new Vector3(0f, 0, 2f);
        instance.stage = stage;
        instance.hand = hand;
                
        instance.UiCamera = new GameObject("VrUiCamera").AddComponent<Camera>();
        // instance.UiCamera.transform.SetParent(instance.targetTransform, false);
        instance.UiCamera.transform.localPosition = Vector3.forward * -4f;
        instance.UiCamera.orthographic = true;
        instance.UiCamera.cullingMask = LayerHelper.GetMask(GameLayer.UI, GameLayer.Map);;
        instance.UiCamera.targetTexture = instance.GetUiRenderTexture();
        instance.UiCamera.depth = 10;
        return instance;
    }
    
    public void Start()
    {
        previousForward = GetCameraForward();
    }

    private RenderTexture GetUiRenderTexture()
    {
        if (!vrUiQuad)
        {
            vrUiQuad = Instantiate(VrAssetLoader.VrUiQuadPrefab, hand.transform, false);
            LayerHelper.SetLayerRecursive(vrUiQuad, GameLayer.VrUi);
            vrUiQuad.transform.localPosition = new Vector3(0.3f, 0f, 0f);
            vrUiQuad.transform.localEulerAngles = new Vector3(-90f, 180f, 0f);
            vrUiQuad.transform.localScale = Vector3.one * 0.1f;
            LookAtCamera.Create(vrUiQuad.transform, stage.VrCamera);
            // VrMaterialHelper.MakeMaterialDrawOnTop(vrUiQuad.GetComponentInChildren<Renderer>().material);
            // vrUiQuad.GetComponentInChildren<Renderer>().material.shader = Shader.Find("NW/Particles/AlphaBlendDrawOnTop");
        }
        return vrUiQuad.GetComponentInChildren<Renderer>().material.mainTexture as RenderTexture;
    }

    private void Update()
    {
        UpdateTransform();
        UpdateClearFlags();
    }

    private void UpdateClearFlags()
    {
        UiCamera.clearFlags = PlayerHudCamera ? CameraClearFlags.Nothing : CameraClearFlags.Depth;
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