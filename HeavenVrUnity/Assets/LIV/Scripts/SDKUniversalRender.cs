#if LIV_UNIVERSAL_RENDER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LIV.SDK.Unity
{
    public partial class SDKRender : System.IDisposable
    {
        // Renders the clip plane in the foreground texture
        private SDKPass _clipPlanePass = null;
        // Renders the clipped opaque content in to the foreground texture alpha
        private SDKPass _combineAlphaPass = null;
        // Captures texture before post-effects
        private SDKPass _captureTexturePass = null;
        // Renders captured texture
        private SDKPass _applyTexturePass = null;
        // Renders background and foreground in single render
        private SDKPass _optimizedRenderingPass = null;

        private RenderPassEvent _clipPlaneRenderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        private RenderPassEvent _addAlphaRenderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        private RenderPassEvent _captureTextureRenderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        private RenderPassEvent _applyTextureRenderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        private RenderPassEvent _optimizedRenderingPassEvent = RenderPassEvent.AfterRenderingTransparents;

        // Clear material
        private Material _clipPlaneSimpleMaterial = null;
        // Transparent material for visual debugging
        private Material _clipPlaneSimpleDebugMaterial = null;
        // Tessellated height map clear material
        private Material _clipPlaneComplexMaterial = null;
        // Tessellated height map clear material for visual debugging
        private Material _clipPlaneComplexDebugMaterial = null;
        private Material _writeOpaqueToAlphaMaterial = null;
        private Material _combineAlphaMaterial = null;
        private Material _writeMaterial = null;
        private Material _forceForwardRenderingMaterial = null;

        private RenderTexture _backgroundRenderTexture = null;
        private RenderTexture _foregroundRenderTexture = null;
        private RenderTexture _optimizedRenderTexture = null;
        private RenderTexture _complexClipPlaneRenderTexture = null;

        private UniversalAdditionalCameraData _universalAdditionalCameraData = null;
        private RenderTargetIdentifier _cameraColorTextureIdentifier = new RenderTargetIdentifier("_CameraColorTexture");

        bool useDeferredRendering {
            get {
                return _cameraInstance.actualRenderingPath == RenderingPath.DeferredLighting ||
                _cameraInstance.actualRenderingPath == RenderingPath.DeferredShading;
            }
        }

        bool interlacedRendering {
            get {
                return SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.INTERLACED_RENDER);
            }
        }

        bool canRenderBackground {
            get {
                if (interlacedRendering)
                {
                    // Render only if frame is even 
                    if (Time.frameCount % 2 != 0) return false;
                }
                return SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.BACKGROUND_RENDER) && _backgroundRenderTexture != null;
            }
        }

        bool canRenderForeground {
            get {
                if (interlacedRendering)
                {
                    // Render only if frame is odd 
                    if (Time.frameCount % 2 != 1) return false;
                }
                return SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.FOREGROUND_RENDER) && _foregroundRenderTexture != null;
            }
        }

        bool canRenderOptimized {
            get {
                return SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.OPTIMIZED_RENDER) && _optimizedRenderTexture != null; ;
            }
        }

        public SDKRender(LIV liv)
        {
            _liv = liv;
            CreateAssets();
        }

        public void Render()
        {
            UpdateBridgeResolution();
            UpdateBridgeInputFrame();
            SDKUtils.ApplyUserSpaceTransform(this);
            UpdateTextures();
            InvokePreRender();
            if (canRenderBackground) RenderBackground();
            if (canRenderForeground) RenderForeground();
            if (canRenderOptimized) RenderOptimized();
            IvokePostRender();
            SDKUtils.CreateBridgeOutputFrame(this);
            SDKBridge.IssuePluginEvent();
        }

        // Default render without any special changes
        private void RenderBackground()
        {
            bool debugClipPlane = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.DEBUG_CLIP_PLANE);            
            bool debug = debugClipPlane;

            SDKUtils.SetCamera(_cameraInstance, _cameraInstance.transform, _inputFrame, localToWorldMatrix, spectatorLayerMask);
            _cameraInstance.targetTexture = _backgroundRenderTexture;

            RenderTexture tempRenderTexture = null;

            bool overridePostProcessing = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.OVERRIDE_POST_PROCESSING);
            if (overridePostProcessing)
            {
                tempRenderTexture = RenderTexture.GetTemporary(_backgroundRenderTexture.width, _backgroundRenderTexture.height, 0, _backgroundRenderTexture.format);
#if UNITY_EDITOR
                tempRenderTexture.name = "LIV.TemporaryRenderTexture";
#endif

                _captureTexturePass.commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tempRenderTexture);
                _applyTexturePass.commandBuffer.Blit(tempRenderTexture, BuiltinRenderTextureType.CurrentActive);

                SDKUniversalRenderFeature.AddPass(_captureTexturePass);
                SDKUniversalRenderFeature.AddPass(_applyTexturePass);
            }

            SDKShaders.StartRendering();
            SDKShaders.StartBackgroundRendering();
            InvokePreRenderBackground();
            SendTextureToBridge(_backgroundRenderTexture, TEXTURE_ID.BACKGROUND_COLOR_BUFFER_ID);
            if (debug) RenderDebugPreRender();
            _cameraInstance.Render();
            InvokePostRenderBackground();
            if (debug) RenderDebugPostRender(_backgroundRenderTexture);
            _cameraInstance.targetTexture = null;
            SDKShaders.StopBackgroundRendering();
            SDKShaders.StopRendering();

            if (overridePostProcessing)
            {
                _captureTexturePass.commandBuffer.Clear();
                _applyTexturePass.commandBuffer.Clear();
                RenderTexture.ReleaseTemporary(tempRenderTexture);
            }

            SDKUniversalRenderFeature.ClearPasses();
        }

        // Extract the image which is in front of our clip plane
        // The compositing is heavily relying on the alpha channel, therefore we want to make sure it does
        // not get corrupted by the postprocessing or any shader
        private void RenderForeground()
        {
            bool debugClipPlane = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.DEBUG_CLIP_PLANE);
            bool renderComplexClipPlane = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.COMPLEX_CLIP_PLANE);
            bool renderGroundClipPlane = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.GROUND_CLIP_PLANE);
            bool overridePostProcessing = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.OVERRIDE_POST_PROCESSING);
            bool fixPostEffectsAlpha = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.FIX_FOREGROUND_ALPHA) | _liv.fixPostEffectsAlpha;
            bool debug = debugClipPlane;

            MonoBehaviour[] behaviours = null;
            bool[] wasBehaviourEnabled = null;
            if (disableStandardAssets) SDKUtils.DisableStandardAssets(_cameraInstance, ref behaviours, ref wasBehaviourEnabled);

            // Capture camera defaults
            CameraClearFlags capturedClearFlags = _cameraInstance.clearFlags;
            Color capturedBgColor = _cameraInstance.backgroundColor;
            Color capturedFogColor = RenderSettings.fogColor;

            // Make sure that fog does not corrupt alpha channel
            RenderSettings.fogColor = new Color(capturedFogColor.r, capturedFogColor.g, capturedFogColor.b, 0f);
            SDKUtils.SetCamera(_cameraInstance, _cameraInstance.transform, _inputFrame, localToWorldMatrix, spectatorLayerMask);
            _cameraInstance.clearFlags = CameraClearFlags.Color;
            _cameraInstance.backgroundColor = Color.clear;
            _cameraInstance.targetTexture = _foregroundRenderTexture;

            RenderTexture capturedAlphaRenderTexture = RenderTexture.GetTemporary(_foregroundRenderTexture.width, _foregroundRenderTexture.height, 0, _foregroundRenderTexture.format);
#if UNITY_EDITOR
            capturedAlphaRenderTexture.name = "LIV.CapturedAlphaRenderTexture";
#endif

            // Render opaque pixels into alpha channel
            _clipPlanePass.commandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _writeOpaqueToAlphaMaterial, 0, 0);

            // Render clip plane
            Matrix4x4 clipPlaneTransform = localToWorldMatrix * (Matrix4x4)_inputFrame.clipPlane.transform;
            _clipPlanePass.commandBuffer.DrawMesh(_clipPlaneMesh, clipPlaneTransform,
                GetClipPlaneMaterial(debugClipPlane, renderComplexClipPlane, ColorWriteMask.All, ref _clipPlaneMaterialProperty), 0, 0, _clipPlaneMaterialProperty);

            // Render ground clip plane
            if (renderGroundClipPlane)
            {
                Matrix4x4 groundClipPlaneTransform = localToWorldMatrix * (Matrix4x4)_inputFrame.groundClipPlane.transform;
                _clipPlanePass.commandBuffer.DrawMesh(_clipPlaneMesh, groundClipPlaneTransform,
                    GetClipPlaneMaterial(debugClipPlane, false, ColorWriteMask.All, ref _groundPlaneMaterialProperty), 0, 0, _groundPlaneMaterialProperty);
            }

            // Copy alpha in to texture
            _clipPlanePass.commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, capturedAlphaRenderTexture);
            _clipPlanePass.commandBuffer.SetRenderTarget(_cameraColorTextureIdentifier);
            SDKUniversalRenderFeature.AddPass(_clipPlanePass);

            // Fix alpha corruption by post processing
            RenderTexture tempRenderTexture = null;
            if (overridePostProcessing || fixPostEffectsAlpha)
            {
                tempRenderTexture = RenderTexture.GetTemporary(_foregroundRenderTexture.width, _foregroundRenderTexture.height, 0, _foregroundRenderTexture.format);
#if UNITY_EDITOR
                tempRenderTexture.name = "LIV.TemporaryRenderTexture";
#endif
                _captureTexturePass.commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tempRenderTexture);
                SDKUniversalRenderFeature.AddPass(_captureTexturePass);

                _writeMaterial.SetInt(SDKShaders.LIV_COLOR_MASK, overridePostProcessing ? (int)ColorWriteMask.All : (int)ColorWriteMask.Alpha);
                _applyTexturePass.commandBuffer.Blit(tempRenderTexture, BuiltinRenderTextureType.CurrentActive, _writeMaterial);
                SDKUniversalRenderFeature.AddPass(_applyTexturePass);
            }

            // Combine captured alpha with result alpha
            _combineAlphaMaterial.SetInt(SDKShaders.LIV_COLOR_MASK, (int)ColorWriteMask.Alpha);
            _combineAlphaMaterial.mainTexture = capturedAlphaRenderTexture;
            _combineAlphaPass.commandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _combineAlphaMaterial);
            SDKUniversalRenderFeature.AddPass(_combineAlphaPass);

            if (useDeferredRendering) SDKUtils.ForceForwardRendering(cameraInstance, _clipPlaneMesh, _forceForwardRenderingMaterial);

            SDKShaders.StartRendering();
            SDKShaders.StartForegroundRendering();
            InvokePreRenderForeground();
            SendTextureToBridge(_foregroundRenderTexture, TEXTURE_ID.FOREGROUND_COLOR_BUFFER_ID);
            if (debug) RenderDebugPreRender();
            _cameraInstance.Render();            
            InvokePostRenderForeground();
            if (debug) RenderDebugPostRender(_foregroundRenderTexture);
            _cameraInstance.targetTexture = null;
            SDKShaders.StopForegroundRendering();
            SDKShaders.StopRendering();

            if (overridePostProcessing || fixPostEffectsAlpha)
            {
                _captureTexturePass.commandBuffer.Clear();
                _applyTexturePass.commandBuffer.Clear();

                RenderTexture.ReleaseTemporary(tempRenderTexture);
            }

            RenderTexture.ReleaseTemporary(capturedAlphaRenderTexture);

            _clipPlanePass.commandBuffer.Clear();
            _combineAlphaPass.commandBuffer.Clear();

            SDKUniversalRenderFeature.ClearPasses();

            // Revert camera defaults
            _cameraInstance.clearFlags = capturedClearFlags;
            _cameraInstance.backgroundColor = capturedBgColor;
            RenderSettings.fogColor = capturedFogColor;

            SDKUtils.RestoreStandardAssets(ref behaviours, ref wasBehaviourEnabled);
        }

        // Renders a single camera in a single texture with occlusion only from opaque objects.
        // This is the most performant option for mixed reality.
        // It does not support any transparency in the foreground layer.
        private void RenderOptimized()
        {
            bool debugClipPlane = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.DEBUG_CLIP_PLANE);
            bool renderComplexClipPlane = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.COMPLEX_CLIP_PLANE);
            bool renderGroundClipPlane = SDKUtils.FeatureEnabled(inputFrame.features, FEATURES.GROUND_CLIP_PLANE);
            bool debug = debugClipPlane;

            SDKUtils.SetCamera(_cameraInstance, _cameraInstance.transform, _inputFrame, localToWorldMatrix, spectatorLayerMask);
            _cameraInstance.targetTexture = _optimizedRenderTexture;

            RenderTexture capturedAlphaRenderTexture = RenderTexture.GetTemporary(_optimizedRenderTexture.width, _optimizedRenderTexture.height, 0, _optimizedRenderTexture.format);
#if UNITY_EDITOR
            capturedAlphaRenderTexture.name = "LIV.CapturedAlphaRenderTexture";
#endif
            // Clear alpha channel
            _writeMaterial.SetInt(SDKShaders.LIV_COLOR_MASK, (int)ColorWriteMask.Alpha);
            _optimizedRenderingPass.commandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _writeMaterial);

            // Render opaque pixels into alpha channel            
            _writeOpaqueToAlphaMaterial.SetInt(SDKShaders.LIV_COLOR_MASK, (int)ColorWriteMask.Alpha);
            _optimizedRenderingPass.commandBuffer.DrawMesh(_clipPlaneMesh, Matrix4x4.identity, _writeOpaqueToAlphaMaterial, 0, 0);

            // Render clip plane            
            Matrix4x4 clipPlaneTransform = localToWorldMatrix * (Matrix4x4)_inputFrame.clipPlane.transform;
            _optimizedRenderingPass.commandBuffer.DrawMesh(_clipPlaneMesh, clipPlaneTransform,
                GetClipPlaneMaterial(debugClipPlane, renderComplexClipPlane, ColorWriteMask.Alpha, ref _clipPlaneMaterialProperty), 0, 0, _clipPlaneMaterialProperty);

            // Render ground clip plane            
            if (renderGroundClipPlane)
            {
                Matrix4x4 groundClipPlaneTransform = localToWorldMatrix * (Matrix4x4)_inputFrame.groundClipPlane.transform;
                _optimizedRenderingPass.commandBuffer.DrawMesh(_clipPlaneMesh, groundClipPlaneTransform,
                    GetClipPlaneMaterial(debugClipPlane, false, ColorWriteMask.Alpha, ref _groundPlaneMaterialProperty), 0, 0, _groundPlaneMaterialProperty);
            }

            // Copy alpha in to texture
            _optimizedRenderingPass.commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, capturedAlphaRenderTexture);
            _optimizedRenderingPass.commandBuffer.SetRenderTarget(_cameraColorTextureIdentifier);
            SDKUniversalRenderFeature.AddPass(_optimizedRenderingPass);

            if (useDeferredRendering) SDKUtils.ForceForwardRendering(cameraInstance, _clipPlaneMesh, _forceForwardRenderingMaterial);

            // TODO: this is just proprietary
            SDKShaders.StartRendering();
            SDKShaders.StartBackgroundRendering();
            InvokePreRenderBackground();
            SendTextureToBridge(_optimizedRenderTexture, TEXTURE_ID.OPTIMIZED_COLOR_BUFFER_ID);
            if (debug) RenderDebugPreRender();
            _cameraInstance.Render();

            // Recover alpha
            RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
            RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
            Graphics.Blit(capturedAlphaRenderTexture, _optimizedRenderTexture, _writeMaterial);
            Graphics.SetRenderTarget(activeColorBuffer, activeDepthBuffer);
            if (debug) RenderDebugPostRender(_optimizedRenderTexture);

            InvokePostRenderBackground();
            _cameraInstance.targetTexture = null;
            SDKShaders.StopBackgroundRendering();
            SDKShaders.StopRendering();

            RenderTexture.ReleaseTemporary(capturedAlphaRenderTexture);

            _optimizedRenderingPass.commandBuffer.Clear();
            _combineAlphaPass.commandBuffer.Clear();

            SDKUniversalRenderFeature.ClearPasses();
        }

        private void CreateAssets()
        {
            bool cameraReferenceEnabled = cameraReference.enabled;
            if (cameraReferenceEnabled)
            {
                cameraReference.enabled = false;
            }
            bool cameraReferenceActive = cameraReference.gameObject.activeSelf;
            if (cameraReferenceActive)
            {
                cameraReference.gameObject.SetActive(false);
            }

            GameObject cloneGO = (GameObject)Object.Instantiate(cameraReference.gameObject, _liv.stage);
            _cameraInstance = (Camera)cloneGO.GetComponent("Camera");

            SDKUtils.CleanCameraBehaviours(_cameraInstance, _liv.excludeBehaviours);

            if (cameraReferenceActive != cameraReference.gameObject.activeSelf)
            {
                cameraReference.gameObject.SetActive(cameraReferenceActive);
            }
            if (cameraReferenceEnabled != cameraReference.enabled)
            {
                cameraReference.enabled = cameraReferenceEnabled;
            }

            _cameraInstance.name = "LIV Camera";
            if (_cameraInstance.tag == "MainCamera")
            {
                _cameraInstance.tag = "Untagged";
            }

            _cameraInstance.transform.localScale = Vector3.one;
            _cameraInstance.rect = new Rect(0, 0, 1, 1);
            _cameraInstance.depth = 0;
#if UNITY_5_4_OR_NEWER
            _cameraInstance.stereoTargetEye = StereoTargetEyeMask.None;
#endif
#if UNITY_5_6_OR_NEWER
            _cameraInstance.allowMSAA = false;
#endif
            _cameraInstance.enabled = false;
            _cameraInstance.gameObject.SetActive(true);
            _universalAdditionalCameraData = _cameraInstance.GetComponent<UniversalAdditionalCameraData>();
            
            _clipPlaneMesh = new Mesh();
            SDKUtils.CreateClipPlane(_clipPlaneMesh, 10, 10, true, 1000f);

            _quadMesh = new Mesh();
            SDKUtils.CreateQuad(_quadMesh);

            _boxMesh = new Mesh();
            SDKUtils.CreateBox(_boxMesh, Vector3.one);
            Debug.Log("render 1");

            _clipPlaneSimpleMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_CLIP_PLANE_SIMPLE_SHADER));
            _clipPlaneSimpleDebugMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_CLIP_PLANE_SIMPLE_DEBUG_SHADER));
            _clipPlaneComplexMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_CLIP_PLANE_COMPLEX_SHADER));
            _clipPlaneComplexDebugMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_CLIP_PLANE_COMPLEX_DEBUG_SHADER));
            _writeOpaqueToAlphaMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_WRITE_OPAQUE_TO_ALPHA_SHADER));
            _combineAlphaMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_COMBINE_ALPHA_SHADER));
            _writeMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_WRITE_SHADER));
            _forceForwardRenderingMaterial = new Material(SDKShaders.GetShader(SDKShaders.LIV_FORCE_FORWARD_RENDERING_SHADER));

            Debug.Log("render 2");
            
            _clipPlanePass = new SDKPass();
            _clipPlanePass.renderPassEvent = _clipPlaneRenderPassEvent;
            _clipPlanePass.commandBuffer = new CommandBuffer();
            Debug.Log("render 3");

            _combineAlphaPass = new SDKPass();
            _combineAlphaPass.renderPassEvent = _addAlphaRenderPassEvent;
            _combineAlphaPass.commandBuffer = new CommandBuffer();
            Debug.Log("render 4");

            _captureTexturePass = new SDKPass();
            _captureTexturePass.renderPassEvent = _captureTextureRenderPassEvent;
            _captureTexturePass.commandBuffer = new CommandBuffer();
            Debug.Log("render 5");

            _applyTexturePass = new SDKPass();
            _applyTexturePass.renderPassEvent = _applyTextureRenderPassEvent;
            _applyTexturePass.commandBuffer = new CommandBuffer();
            Debug.Log("render 6");

            _optimizedRenderingPass = new SDKPass();
            _optimizedRenderingPass.renderPassEvent = _optimizedRenderingPassEvent;
            _optimizedRenderingPass.commandBuffer = new CommandBuffer();
                        Debug.Log("render 7");

            _clipPlaneMaterialProperty = new MaterialPropertyBlock();
            _clipPlaneMaterialProperty.SetColor(SDKShaders.LIV_COLOR, SDKShaders.GREEN_COLOR);
            _groundPlaneMaterialProperty = new MaterialPropertyBlock();
            _groundPlaneMaterialProperty.SetColor(SDKShaders.LIV_COLOR, SDKShaders.BLUE_COLOR);
            _hmdMaterialProperty = new MaterialPropertyBlock();
            _hmdMaterialProperty.SetColor(SDKShaders.LIV_COLOR, SDKShaders.RED_COLOR);
            Debug.Log("render 8");

            _universalAdditionalCameraData.antialiasing = AntialiasingMode.None;
            _universalAdditionalCameraData.antialiasingQuality = AntialiasingQuality.Low;
            _universalAdditionalCameraData.dithering = false;
            
#if UNITY_EDITOR
            _quadMesh.name = "LIV.quad";
            _clipPlaneMesh.name = "LIV.clipPlane";
            _clipPlaneSimpleMaterial.name = "LIV.clipPlaneSimple";
            _clipPlaneSimpleDebugMaterial.name = "LIV.clipPlaneSimpleDebug";
            _clipPlaneComplexMaterial.name = "LIV.clipPlaneComplex";
            _clipPlaneComplexDebugMaterial.name = "LIV.clipPlaneComplexDebug";
            _writeOpaqueToAlphaMaterial.name = "LIV.writeOpaqueToAlpha";
            _combineAlphaMaterial.name = "LIV.combineAlpha";
            _writeMaterial.name = "LIV.write";
            _forceForwardRenderingMaterial.name = "LIV.forceForwardRendering";
            _clipPlanePass.commandBuffer.name = "LIV.renderClipPlanes";
            _combineAlphaPass.commandBuffer.name = "LIV.foregroundCombineAlpha";
            _captureTexturePass.commandBuffer.name = "LIV.captureTexture";
            _applyTexturePass.commandBuffer.name = "LIV.applyTexture";
            _optimizedRenderingPass.commandBuffer.name = "LIV.optimizedRendering";
#endif
        }

        private void DestroyAssets()
        {
            if (_cameraInstance != null)
            {
                Object.Destroy(_cameraInstance.gameObject);
                _cameraInstance = null;
            }

            SDKUtils.DestroyObject<Mesh>(ref _quadMesh);
            SDKUtils.DestroyObject<Mesh>(ref _clipPlaneMesh);
            SDKUtils.DestroyObject<Mesh>(ref _boxMesh);

            SDKUtils.DestroyObject<Material>(ref _clipPlaneSimpleMaterial);
            SDKUtils.DestroyObject<Material>(ref _clipPlaneSimpleDebugMaterial);
            SDKUtils.DestroyObject<Material>(ref _clipPlaneComplexMaterial);
            SDKUtils.DestroyObject<Material>(ref _clipPlaneComplexDebugMaterial);
            SDKUtils.DestroyObject<Material>(ref _writeOpaqueToAlphaMaterial);
            SDKUtils.DestroyObject<Material>(ref _combineAlphaMaterial);
            SDKUtils.DestroyObject<Material>(ref _writeMaterial);
            SDKUtils.DestroyObject<Material>(ref _forceForwardRenderingMaterial);

            SDKUtils.DisposeObject<CommandBuffer>(ref _clipPlanePass.commandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _combineAlphaPass.commandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _captureTexturePass.commandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _applyTexturePass.commandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _optimizedRenderingPass.commandBuffer);

            SDKUtils.DestroyTexture(ref _backgroundRenderTexture);
            SDKUtils.DestroyTexture(ref _foregroundRenderTexture);
            SDKUtils.DestroyTexture(ref _optimizedRenderTexture);
            SDKUtils.DestroyTexture(ref _complexClipPlaneRenderTexture);
        }

        public void Dispose()
        {
            ReleaseBridgePoseControl();
            DestroyAssets();
            DisposeDebug();            
        }
    }
}
#endif