// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 22/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: WorldTooltipTrigger.cs
// Summary: Opens contextual help when a physics-raycasted world object is hovered.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class WorldTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Content")]
    [SerializeField] private LocalizedString localizedTitle = new LocalizedString();
    [SerializeField] private LocalizedString localizedDescription = new LocalizedString();

    [Header("Presentation")]
    [SerializeField] private TooltipPresenter presenter;
    [SerializeField] private TooltipPlacement placement = TooltipPlacement.Up;
    [SerializeField] private Vector2 positionOffset;

    private Collider2D _collider;
    private Camera _sourceCamera;
    private string _title;
    private string _description;
    private bool _isHovered;
    private bool _titleReady;
    private bool _descriptionReady;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (presenter == null)
        {
            Debug.LogError(
                $"[{nameof(WorldTooltipTrigger)}] A {nameof(TooltipPresenter)} reference is required.",
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
        _isHovered = false;
        _sourceCamera = null;
        if (presenter != null)
        {
            presenter.Hide(this);
        }
    }

    private void LateUpdate()
    {
        RefreshContent();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsMouse(eventData))
        {
            return;
        }

        _sourceCamera = eventData.enterEventCamera;
        _isHovered = _sourceCamera != null;
        RefreshContent();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsMouse(eventData))
        {
            return;
        }

        _isHovered = false;
        _sourceCamera = null;
        if (presenter != null)
        {
            presenter.Hide(this);
        }
    }

    public void SetPresenter(TooltipPresenter value)
    {
        if (presenter == value)
        {
            return;
        }

        if (presenter != null)
        {
            presenter.Hide(this);
        }

        presenter = value;
        RefreshContent();
    }

    private void HandleTitleChanged(string value)
    {
        _title = value;
        _titleReady = true;
        RefreshContent();
    }

    private void HandleDescriptionChanged(string value)
    {
        _description = value;
        _descriptionReady = true;
        RefreshContent();
    }

    private void HandleSelectedLocaleChanged(Locale _)
    {
        ResetContent();
    }

    private void RefreshContent()
    {
        if (!_isHovered || !_titleReady || !_descriptionReady ||
            presenter == null || _sourceCamera == null)
        {
            return;
        }

        presenter.Show(
            this,
            _title,
            _description,
            _collider.bounds,
            _sourceCamera,
            placement,
            positionOffset);
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
}
