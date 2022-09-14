// Based on https://github.com/googlearchive/tango-examples-unity/blob/master/TangoWithCardboardExperiments/Assets/Cardboard/Scripts/GazeInputModule.cs

// The MIT License (MIT)
//
// Copyright (c) 2015, Unity Technologies & Google, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.

using HeavenVr.Helpers;
using HeavenVr.Stage;
using HeavenVr.VrInput;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenVr.Laser;

public class LaserInputModule : BaseInputModule
{
    private const float RayDistance = 30f;
    private const float ClickMovementTreshold = 10f;
    private Vector3 _lastHeadPose;
    private PointerEventData _pointerData;
    private Vector2 _previousClickPosition;

    public static void Create(EventSystem eventSystem)
    {
        if (eventSystem.GetComponent<LaserInputModule>()) return;
        
        eventSystem.gameObject.AddComponent<LaserInputModule>();
    }

    public override void DeactivateModule()
    {
        base.DeactivateModule();
        if (_pointerData != null)
        {
            HandlePendingClick();
            HandlePointerExitAndEnter(_pointerData, null);
            _pointerData = null;
        }

        eventSystem.SetSelectedGameObject(null, GetBaseEventData());
    }

    public override bool IsPointerOverGameObject(int pointerId)
    {
        return _pointerData != null && _pointerData.pointerEnter != null;
    }

    public override void Process()
    {
        if (!VrStage.Instance || !VrStage.Instance.aimLaser) return;

        CastRay();
        UpdateCurrentObject();

        var clickBinding = InputMap.GetBinding("Submit");
        if (clickBinding == null) return;
        
        if (!clickBinding.WasPressedThisFrame && clickBinding.IsPressed && IsMoving())
            HandleDrag();
        else if (!_pointerData.eligibleForClick && clickBinding.WasPressedThisFrame)
            HandleClick();
        else if (clickBinding.WasReleasedThisFrame)
            HandlePendingClick();
    }

    private void CastRay()
    {
        if (!VrStage.Instance || !VrStage.Instance.aimLaser) return;

        var isHit = Physics.Raycast(
            VrStage.Instance.aimLaser.transform.position,
            VrStage.Instance.aimLaser.transform.forward,
            out var hit,
            RayDistance,
            LayerHelper.GetMask(GameLayer.VrUi));

        VrStage.Instance.aimLaser.SetDistance(isHit ? hit.distance : RayDistance);
            
        var pointerPosition = Vector3.zero;
        if (isHit)
        {
            var renderTexture = VrAssetLoader.VrUiRenderTexture;
            var localPoint = hit.collider.transform.InverseTransformPoint(hit.point) + Vector3.one * 0.5f;
            var localTexturePoint = new Vector2(renderTexture.width, renderTexture.height);
            pointerPosition = new Vector2(localTexturePoint.x * localPoint.x, localTexturePoint.y * localPoint.y);
        }

        if (_pointerData == null)
        {
            _pointerData = new PointerEventData(eventSystem);
            _lastHeadPose = pointerPosition;
        }

        // Cast a ray into the scene
        _pointerData.Reset();
        _pointerData.position = pointerPosition;
        eventSystem.RaycastAll(_pointerData, m_RaycastResultCache);
        _pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_RaycastResultCache.Clear();
        _pointerData.delta = pointerPosition - _lastHeadPose;
        _lastHeadPose = hit.point;
    }

    private void UpdateCurrentObject()
    {
        // Send enter events and update the highlight.
        var go = _pointerData.pointerCurrentRaycast.gameObject;
        HandlePointerExitAndEnter(_pointerData, go);
    }

    private void HandleDrag()
    {
        var moving = _pointerData.IsPointerMoving();

        if (moving && _pointerData.pointerDrag != null && !_pointerData.dragging)
        {
            ExecuteEvents.Execute(_pointerData.pointerDrag, _pointerData,
                ExecuteEvents.beginDragHandler);
            _pointerData.dragging = true;
        }

        if (!_pointerData.dragging || !moving || _pointerData.pointerDrag == null) return;

        ExecuteEvents.Execute(_pointerData.pointerDrag, _pointerData, ExecuteEvents.dragHandler);
    }

    private bool IsMoving()
    {
        return _pointerData != null && Vector2.Distance(_previousClickPosition, _pointerData.position) > ClickMovementTreshold;
    }

    private void HandlePendingClick()
    {
        if (!_pointerData.eligibleForClick) return;

        var go = _pointerData.pointerCurrentRaycast.gameObject;

        // Send pointer up and click events.
        ExecuteEvents.Execute(_pointerData.pointerPress, _pointerData, ExecuteEvents.pointerUpHandler);
        if (!IsMoving())
        {
            ExecuteEvents.Execute(_pointerData.pointerPress, _pointerData, ExecuteEvents.pointerClickHandler);
        }

        if (_pointerData.pointerDrag != null)
            ExecuteEvents.ExecuteHierarchy(go, _pointerData, ExecuteEvents.dropHandler);

        if (_pointerData.pointerDrag != null && _pointerData.dragging)
            ExecuteEvents.Execute(_pointerData.pointerDrag, _pointerData, ExecuteEvents.endDragHandler);

        // Clear the click state.
        _pointerData.pointerPress = null;
        _pointerData.rawPointerPress = null;
        _pointerData.eligibleForClick = false;
        _pointerData.clickCount = 0;
        _pointerData.pointerDrag = null;
        _pointerData.dragging = false;
    }

    private void HandleClick()
    {
        _previousClickPosition = _pointerData.position;
        
        var go = _pointerData.pointerCurrentRaycast.gameObject;

        // Send pointer down event.
        _pointerData.pressPosition = _pointerData.position;
        _pointerData.pointerPressRaycast = _pointerData.pointerCurrentRaycast;
        _pointerData.pointerPress =
            ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

        // Save the drag handler as well
        _pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
        if (_pointerData.pointerDrag != null)
            ExecuteEvents.Execute(_pointerData.pointerDrag, _pointerData, ExecuteEvents.initializePotentialDrag);

        // Save the pending click state.
        _pointerData.rawPointerPress = go;
        _pointerData.eligibleForClick = true;
        _pointerData.delta = Vector2.zero;
        _pointerData.dragging = false;
        _pointerData.useDragThreshold = true;
        _pointerData.clickCount = 1;
        _pointerData.clickTime = Time.unscaledTime;
    }
}