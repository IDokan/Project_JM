// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 15/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TooltipTrigger.cs
// Summary: Opens contextual help from mouse hover or a held touch on a combat HUD region.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(RectTransform))]
public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Content")]
    [SerializeField] private string title;
    [SerializeField, TextArea(2, 4)] private string description;

    [Header("Presentation")]
    [SerializeField] private TooltipPlacement placement;
    [SerializeField] private Vector2 positionOffset;

    private RectTransform _rectTransform;
    private int _touchPointerId = int.MinValue;
    private TooltipPresenter _presenter;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _presenter = FindFirstObjectByType<TooltipPresenter>(FindObjectsInactive.Include);
    }

    private void OnDisable()
    {
        _touchPointerId = int.MinValue;
        if (_presenter != null)
        {
            _presenter.Hide(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsMouse(eventData))
        {
            return;
        }

        Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsMouse(eventData))
        {
            Hide();
            return;
        }

        if (eventData.pointerId == _touchPointerId)
        {
            _touchPointerId = int.MinValue;
            Hide();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsTouch(eventData))
        {
            return;
        }

        _touchPointerId = eventData.pointerId;
        Show();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != _touchPointerId)
        {
            return;
        }

        _touchPointerId = int.MinValue;
        Hide();
    }

    private void Show()
    {
        if (_presenter == null)
        {
            _presenter = FindFirstObjectByType<TooltipPresenter>(FindObjectsInactive.Include);
        }

        if (_presenter != null)
        {
            _presenter.Show(this, title, description, _rectTransform, placement, positionOffset);
        }
    }

    private void Hide()
    {
        if (_presenter != null)
        {
            _presenter.Hide(this);
        }
    }

    private static bool IsMouse(PointerEventData eventData)
    {
        return eventData is ExtendedPointerEventData extendedEventData &&
            extendedEventData.pointerType == UIPointerType.MouseOrPen;
    }

    private static bool IsTouch(PointerEventData eventData)
    {
        return eventData is ExtendedPointerEventData extendedEventData &&
            extendedEventData.pointerType == UIPointerType.Touch;
    }
}
