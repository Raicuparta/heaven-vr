using HeavenVr.Helpers;
using HeavenVr.Stage;
using UnityEngine;

namespace HeavenVr.VrUi;

public class UiTarget : MonoBehaviour
{
    private const float RotationSmoothTime = 0.3f;
    private Vector3 _previousForward;
    private Quaternion _rotationVelocity;
    private Quaternion _targetRotation;
    private Transform _targetTransform;
    private const float MinAngleDelta = 45f;
    private VrStage _stage;
    private GameObject _vrUiQuad;
    public Camera UiCamera { get; private set; }
    private VrHand _hand;
    public static Camera PlayerHudCamera; // TODO no static

    public static UiTarget Create(VrStage stage, VrHand hand)
    {
        var instance = new GameObject(nameof(UiTarget)).AddComponent<UiTarget>();
        instance.transform.SetParent(stage.transform, false);
        instance._targetTransform = new GameObject("InteractiveUiTargetTransform").transform;
        instance._targetTransform.SetParent(instance.transform, false);
        instance._targetTransform.localPosition = new Vector3(0f, 0, 2f);
        instance._stage = stage;
        instance._hand = hand;
                
        instance.UiCamera = new GameObject("VrUiCamera").AddComponent<Camera>();
        // instance.UiCamera.transform.SetParent(instance.targetTransform, false);
        instance.UiCamera.transform.localPosition = Vector3.forward * -4f;

        if (stage.gameObject.GetComponentInParent<MenuScreenMapAesthetics>())
        {
            instance.UiCamera.transform.localPosition = new Vector3(-0.0436f, 1.0611f, 0.0982f);
            instance.UiCamera.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            instance.UiCamera.orthographicSize = 2f;
        }
        
        instance.UiCamera.orthographic = true;
        instance.UiCamera.cullingMask = LayerHelper.GetMask(GameLayer.UI, GameLayer.Map);
        instance.UiCamera.targetTexture = instance.GetUiRenderTexture();
        instance.UiCamera.depth = 10;
        return instance;
    }
    
    public void Start()
    {
        _previousForward = GetCameraForward();
    }

    private RenderTexture GetUiRenderTexture()
    {
        if (!_vrUiQuad)
        {
            _vrUiQuad = Instantiate(VrAssetLoader.VrUiQuadPrefab, _hand.transform, false);
            LayerHelper.SetLayerRecursive(_vrUiQuad, GameLayer.VrUi);
            _vrUiQuad.transform.localPosition = new Vector3(0.3f, 0f, 0f);
            _vrUiQuad.transform.localEulerAngles = new Vector3(-90f, 180f, 0f);
            _vrUiQuad.transform.localScale = Vector3.one * 0.1f;
        }
        return _vrUiQuad.GetComponentInChildren<Renderer>().material.mainTexture as RenderTexture;
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
        return !_stage.VrCamera ? Vector3.forward : MathHelper.GetProjectedForward(_stage.VrCamera.transform);
    }

    private void UpdateTransform()
    {
        if (!_stage.VrCamera) return;

        var cameraForward = GetCameraForward();
        var unsignedAngleDelta = Vector3.Angle(_previousForward, cameraForward);
        _targetTransform.localRotation = _stage.CameraPoseDriver.originPose.rotation;

        if (unsignedAngleDelta > MinAngleDelta)
        {
            _targetRotation = Quaternion.LookRotation(cameraForward, _stage.CameraPoseDriver.transform.parent.rotation * _stage.CameraPoseDriver.originPose.rotation * Vector3.up);
            _previousForward = cameraForward;
        }

        transform.rotation = MathHelper.SmoothDamp(
            transform.rotation,
            _targetRotation,
            ref _rotationVelocity,
            RotationSmoothTime);

        transform.position = _stage.VrCamera.transform.position;
    }
}