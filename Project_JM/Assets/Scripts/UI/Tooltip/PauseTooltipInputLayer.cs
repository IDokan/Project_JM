// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 28/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseTooltipInputLayer.cs
// Summary: Routes paused pointer inspection to tooltips ahead of the outside-click catcher.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class PauseTooltipInputLayer : BaseRaycaster,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private Button backgroundCatcher;
    [SerializeField] private GraphicRaycaster menuRaycaster;

    private static bool _isResolving;
    private readonly List<RaycastResult> _hits = new List<RaycastResult>();
    private readonly Dictionary<int, PointerEventData> _pointers = new Dictionary<int, PointerEventData>();
    private readonly Dictionary<int, RaycastResult> _hoverTargets = new Dictionary<int, RaycastResult>();
    private readonly Dictionary<int, RaycastResult> _pressTargets = new Dictionary<int, RaycastResult>();
    private readonly List<int> _pointerIds = new List<int>();

    public override Camera eventCamera => menuRaycaster != null ? menuRaycaster.eventCamera : null;
    public override int sortOrderPriority => menuRaycaster != null ? menuRaycaster.sortOrderPriority : int.MinValue;
    public override int renderOrderPriority => menuRaycaster != null ? menuRaycaster.renderOrderPriority : int.MinValue;

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (!TryGetTarget(eventData, out RaycastResult target, out RaycastResult catcher))
        {
            return;
        }

        // Match the catcher's canvas priorities and put only this tooltip hit one
        // step ahead. A panel or another modal above the catcher wins unchanged.
        catcher.gameObject = gameObject;
        catcher.module = this;
        catcher.depth += 1;
        catcher.index = resultAppendList.Count;
        resultAppendList.Add(catcher);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointers[eventData.pointerId] = eventData;
        RefreshPointer(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverTargets.TryGetValue(eventData.pointerId, out RaycastResult target))
        {
            SendExit(target, eventData);
            _hoverTargets.Remove(eventData.pointerId);
        }
        _pointers.Remove(eventData.pointerId);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RefreshPointer(eventData);
        if (_hoverTargets.TryGetValue(eventData.pointerId, out RaycastResult target))
        {
            _pressTargets[eventData.pointerId] = target;
            SendPress(target, eventData, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pressTargets.TryGetValue(eventData.pointerId, out RaycastResult target))
        {
            SendPress(target, eventData, false);
            _pressTargets.Remove(eventData.pointerId);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Own the click without executing Buttons or gameplay handlers beneath it.
        eventData.Use();
    }

    private void LateUpdate()
    {
        // A single proxy may cover adjacent sources. Re-resolve even with a
        // stationary pointer so moving/disabled sources and new modals are handled.
        _pointerIds.Clear();
        _pointerIds.AddRange(_pointers.Keys);
        foreach (int pointerId in _pointerIds)
        {
            if (_pointers.TryGetValue(pointerId, out PointerEventData pointer))
            {
                RefreshPointer(pointer);
            }
        }
    }

    protected override void OnDisable()
    {
        foreach (KeyValuePair<int, RaycastResult> target in _hoverTargets)
        {
            if (_pointers.TryGetValue(target.Key, out PointerEventData pointer))
            {
                SendExit(target.Value, pointer);
            }
        }
        _hoverTargets.Clear();
        _pressTargets.Clear();
        _pointers.Clear();
        base.OnDisable();
    }

    private void RefreshPointer(PointerEventData pointer)
    {
        bool hasTarget = TryGetTarget(pointer, out RaycastResult next, out RaycastResult catcher);
        _hoverTargets.TryGetValue(pointer.pointerId, out RaycastResult previous);
        if (previous.gameObject == next.gameObject)
        {
            return;
        }

        SendExit(previous, pointer);
        _hoverTargets.Remove(pointer.pointerId);
        if (hasTarget)
        {
            _hoverTargets[pointer.pointerId] = next;
            SendEnter(next, pointer);
        }
    }

    private bool TryGetTarget(PointerEventData pointer, out RaycastResult target, out RaycastResult catcher)
    {
        target = default;
        catcher = default;
        if (_isResolving || pauseMenu == null || !pauseMenu.IsPaused ||
            !pauseMenu.isActiveAndEnabled || backgroundCatcher == null ||
            menuRaycaster == null || EventSystem.current == null)
        {
            return false;
        }

        // Resolve the normal hit order with all inspection layers excluded.
        // This runs from a separate BaseRaycaster, never recursively from a
        // Graphic.Raycast callback (GraphicRaycaster has shared scratch buffers).
        _isResolving = true;
        try
        {
            EventSystem.current.RaycastAll(pointer, _hits);
        }
        finally
        {
            _isResolving = false;
        }

        if (_hits.Count < 2 || _hits[0].gameObject != backgroundCatcher.gameObject)
        {
            return false;
        }

        // Never search through another blocking UI surface to reach a tooltip.
        RaycastResult candidate = _hits[1];
        if (GetTrigger(candidate) == null)
        {
            return false;
        }

        catcher = _hits[0];
        target = candidate;
        return true;
    }

    private static MonoBehaviour GetTrigger(RaycastResult hit)
    {
        if (hit.gameObject == null)
        {
            return null;
        }

        TooltipTrigger uiTrigger = hit.gameObject.GetComponentInParent<TooltipTrigger>();
        if (uiTrigger != null && uiTrigger.isActiveAndEnabled)
        {
            return uiTrigger;
        }

        WorldTooltipTrigger worldTrigger = hit.gameObject.GetComponentInParent<WorldTooltipTrigger>();
        return worldTrigger != null && worldTrigger.isActiveAndEnabled ? worldTrigger : null;
    }

    private static void SendEnter(RaycastResult hit, PointerEventData pointer)
    {
        MonoBehaviour trigger = GetTrigger(hit);
        RaycastResult previous = pointer.pointerCurrentRaycast;
        pointer.pointerCurrentRaycast = hit;
        try
        {
            if (trigger is IPointerEnterHandler handler)
            {
                handler.OnPointerEnter(pointer);
            }
        }
        finally
        {
            pointer.pointerCurrentRaycast = previous;
        }
    }

    private static void SendExit(RaycastResult hit, PointerEventData pointer)
    {
        if (GetTrigger(hit) is IPointerExitHandler handler)
        {
            handler.OnPointerExit(pointer);
        }
    }

    private static void SendPress(RaycastResult hit, PointerEventData pointer, bool pressed)
    {
        MonoBehaviour trigger = GetTrigger(hit);
        RaycastResult previous = pointer.pointerCurrentRaycast;
        pointer.pointerCurrentRaycast = hit;
        try
        {
            if (pressed && trigger is IPointerDownHandler downHandler)
            {
                downHandler.OnPointerDown(pointer);
            }
            else if (!pressed && trigger is IPointerUpHandler upHandler)
            {
                upHandler.OnPointerUp(pointer);
            }
        }
        finally
        {
            pointer.pointerCurrentRaycast = previous;
        }
    }
}
