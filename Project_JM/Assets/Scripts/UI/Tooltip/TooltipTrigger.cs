// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 15/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TooltipTrigger.cs
// Summary: Opens contextual help from mouse hover or a held touch on a combat HUD region.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[RequireComponent(typeof(RectTransform))]
public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Content")]
    [SerializeField] private LocalizedString localizedTitle = new LocalizedString();
    [SerializeField] private LocalizedString localizedDescription = new LocalizedString();

    [Header("Presentation")]
    [SerializeField] private TooltipPresenter presenter;
    [SerializeField] private TooltipPlacement placement;
    [SerializeField] private Vector2 positionOffset;

    private RectTransform _rectTransform;
    private int _touchPointerId = int.MinValue;
    private string _title;
    private string _description;
    private bool _showRequested;
    private bool _titleReady;
    private bool _descriptionReady;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (presenter == null)
        {
            Debug.LogError(
                $"[{nameof(TooltipTrigger)}] A {nameof(TooltipPresenter)} reference is required.",
                this);
        }
    }

    private void OnEnable()
    {
        ResetContent();
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
        SubscribeToContent();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;
        UnsubscribeFromContent();
        _showRequested = false;
        _touchPointerId = int.MinValue;
        if (presenter != null)
        {
            presenter.Hide(this);
        }
    }

    public void SetContent(LocalizedString title, LocalizedString description)
    {
        if (ReferenceEquals(localizedTitle, title) &&
            ReferenceEquals(localizedDescription, description))
        {
            return;
        }

        if (isActiveAndEnabled)
        {
            UnsubscribeFromContent();
        }

        localizedTitle = title ?? new LocalizedString();
        localizedDescription = description ?? new LocalizedString();
        ResetContent();
        Hide();

        if (isActiveAndEnabled)
        {
            SubscribeToContent();
        }
    }

    public void Dismiss()
    {
        Hide();
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
        _showRequested = true;
        RefreshContent();
    }

    private void HandleTitleChanged(string value)
    {
        _title = value;
        _titleReady = true;
        RefreshContent();
    }

    private void HandleSelectedLocaleChanged(Locale _)
    {
        ResetContent();
    }

    private void HandleDescriptionChanged(string value)
    {
        _description = value;
        _descriptionReady = true;
        RefreshContent();
    }

    private void RefreshContent()
    {
        if (!_showRequested || !_titleReady || !_descriptionReady)
        {
            return;
        }

        if (presenter != null)
        {
            presenter.Show(
                this,
                _title,
                _description,
                _rectTransform,
                placement,
                positionOffset);
        }
    }

    private void Hide()
    {
        _showRequested = false;
        if (presenter != null)
        {
            presenter.Hide(this);
        }
    }

    private void ResetContent()
    {
        _titleReady = false;
        _descriptionReady = false;
    }

    private void SubscribeToContent()
    {
        localizedTitle.StringChanged += HandleTitleChanged;
        localizedDescription.StringChanged += HandleDescriptionChanged;
    }

    private void UnsubscribeFromContent()
    {
        localizedTitle.StringChanged -= HandleTitleChanged;
        localizedDescription.StringChanged -= HandleDescriptionChanged;
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
