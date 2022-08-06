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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace HeavenVr;

public class LaserInputModule : BaseInputModule
{
    private const float rayDistance = 30f;
    private Vector3 lastHeadPose;
    private PointerEventData pointerData;
    private InputDevice inputDevice;
    private bool previousClickValue; // TODO use existing binding code.

    public static void Create(EventSystem eventSystem)
    {
        if (eventSystem.GetComponent<LaserInputModule>()) return;

        eventSystem.gameObject.AddComponent<LaserInputModule>();
    }
    
    protected override void Start()
    {
        base.Start();
        inputDevice = GetInputDevice(XRNode.RightHand);
    }
        
    public static InputDevice GetInputDevice(XRNode hand)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(hand, devices);
        return devices.Count > 0 ? devices[0] : default;
    }
        
        
    public override void DeactivateModule()
    {
        base.DeactivateModule();
        if (pointerData != null)
        {
            HandlePendingClick();
            HandlePointerExitAndEnter(pointerData, null);
            pointerData = null;
        }

        eventSystem.SetSelectedGameObject(null, GetBaseEventData());
    }

    public override bool IsPointerOverGameObject(int pointerId)
    {
        return pointerData != null && pointerData.pointerEnter != null;
    }

    public override void Process()
    {
        Debug.Log("process");
            
        if (!VrStage.Instance || !VrStage.Instance.UiTarget || !VrStage.Instance.UiTarget.UiCamera || !VrStage.Instance.AimLaser) return;

        CastRay();
        UpdateCurrentObject();

        inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out var value);
            
        var clickDown = !previousClickValue && value;
        var clickUp = previousClickValue && !value;
        previousClickValue = value;

        if (!clickDown && value)
            HandleDrag();
        else if (!pointerData.eligibleForClick && clickDown)
            HandleTrigger();
        else if (clickUp)
            HandlePendingClick();
    }

    private void CastRay()
    {
        var isHit = Physics.Raycast(
            VrStage.Instance.AimLaser.transform.position,
            VrStage.Instance.AimLaser.transform.forward,
            out var hit,
            rayDistance,
            LayerHelper.GetMask(GameLayer.VrUi));

        // if (isHit)
        //     vrLaser.SetTarget(hit.point);
        // else
        //     vrLaser.SetTarget(null);

        if (isHit)
        {
            Debug.Log("hit " + hit.collider.name);
        }
            
        var pointerPosition = VrStage.Instance.UiTarget.UiCamera.WorldToScreenPoint(hit.point);

        if (pointerData == null)
        {
            pointerData = new PointerEventData(eventSystem);
            lastHeadPose = pointerPosition;
        }

        // Cast a ray into the scene
        pointerData.Reset();
        pointerData.position = pointerPosition;
        eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
        pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_RaycastResultCache.Clear();
        pointerData.delta = pointerPosition - lastHeadPose;
        lastHeadPose = hit.point;
    }

    private void UpdateCurrentObject()
    {
        // Send enter events and update the highlight.
        var go = pointerData.pointerCurrentRaycast.gameObject;
        HandlePointerExitAndEnter(pointerData, go);
        // Update the current selection, or clear if it is no longer the current object.
        var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
        if (selected == eventSystem.currentSelectedGameObject)
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
                ExecuteEvents.updateSelectedHandler);
        else
            eventSystem.SetSelectedGameObject(null, pointerData);
    }

    private void HandleDrag()
    {
        var moving = pointerData.IsPointerMoving();

        if (moving && pointerData.pointerDrag != null && !pointerData.dragging)
        {
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
                ExecuteEvents.beginDragHandler);
            pointerData.dragging = true;
        }

        if (!pointerData.dragging || !moving || pointerData.pointerDrag == null) return;

        ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
    }

    private void HandlePendingClick()
    {
        if (!pointerData.eligibleForClick) return;

        var go = pointerData.pointerCurrentRaycast.gameObject;

        // Send pointer up and click events.
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

        if (pointerData.pointerDrag != null)
            ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);

        if (pointerData.pointerDrag != null && pointerData.dragging)
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);

        // Clear the click state.
        pointerData.pointerPress = null;
        pointerData.rawPointerPress = null;
        pointerData.eligibleForClick = false;
        pointerData.clickCount = 0;
        pointerData.pointerDrag = null;
        pointerData.dragging = false;
    }

    private void HandleTrigger()
    {
        var go = pointerData.pointerCurrentRaycast.gameObject;

        // Send pointer down event.
        pointerData.pressPosition = pointerData.position;
        pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
        pointerData.pointerPress =
            ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
            ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

        // Save the drag handler as well
        pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
        if (pointerData.pointerDrag != null)
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);

        // Save the pending click state.
        pointerData.rawPointerPress = go;
        pointerData.eligibleForClick = true;
        pointerData.delta = Vector2.zero;
        pointerData.dragging = false;
        pointerData.useDragThreshold = true;
        pointerData.clickCount = 1;
        pointerData.clickTime = Time.unscaledTime;
    }

    public static void OnClickTest()
    {
        Debug.Log("Click");
    }
}