// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 15/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TooltipPresenter.cs
// Summary: Displays, positions, and animates the shared combat HUD tooltip.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TooltipPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Placement")]
    [SerializeField] private float sourceSpacing = 16f;
    [SerializeField] private float canvasEdgePadding = 16f;

    [Header("Animation")]
    [SerializeField] private float showDuration = 0.16f;
    [SerializeField, Range(0.5f, 1f)] private float startScale = 0.92f;

    private CanvasGroup _canvasGroup;
    private Coroutine _showRoutine;
    private TooltipTrigger _activeTrigger;
    private RectTransform _placementRectTransform;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _placementRectTransform = (RectTransform)panelRectTransform.parent;

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        panelRectTransform.localScale = Vector3.one;
    }

    private void OnDisable()
    {
        Hide(_activeTrigger);
    }

    public void Show(
        TooltipTrigger trigger,
        string title,
        string description,
        RectTransform source,
        TooltipPlacement placement,
        Vector2 positionOffset)
    {
        _activeTrigger = trigger;
        titleText.text = title;
        descriptionText.text = description;

        Canvas.ForceUpdateCanvases();
        Position(source, placement, positionOffset);

        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
        }

        _showRoutine = StartCoroutine(ShowRoutine());
    }

    public void Hide(TooltipTrigger trigger)
    {
        if (_activeTrigger != trigger)
        {
            return;
        }

        _activeTrigger = null;
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        _canvasGroup.alpha = 0f;
        panelRectTransform.localScale = Vector3.one;
    }

    private IEnumerator ShowRoutine()
    {
        float elapsed = 0f;
        _canvasGroup.alpha = 0f;
        panelRectTransform.localScale = Vector3.one * startScale;

        while (elapsed < showDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = showDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / showDuration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            _canvasGroup.alpha = easedProgress;
            panelRectTransform.localScale = Vector3.LerpUnclamped(
                Vector3.one * startScale,
                Vector3.one,
                easedProgress);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        panelRectTransform.localScale = Vector3.one;
        _showRoutine = null;
    }

    private void Position(RectTransform source, TooltipPlacement placement, Vector2 positionOffset)
    {
        Bounds sourceBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            _placementRectTransform,
            source);
        Vector2 localAnchor;
        Vector2 direction;
        switch (placement)
        {
            case TooltipPlacement.DownLeft:
                localAnchor = new Vector2(sourceBounds.min.x, sourceBounds.min.y);
                direction = new Vector2(-1f, -1f);
                panelRectTransform.pivot = new Vector2(1f, 1f);
                break;
            case TooltipPlacement.UpRight:
                localAnchor = new Vector2(sourceBounds.max.x, sourceBounds.max.y);
                direction = new Vector2(1f, 1f);
                panelRectTransform.pivot = new Vector2(0f, 0f);
                break;
            case TooltipPlacement.UpLeft:
                localAnchor = new Vector2(sourceBounds.min.x, sourceBounds.max.y);
                direction = new Vector2(-1f, 1f);
                panelRectTransform.pivot = new Vector2(1f, 0f);
                break;
            default:
                localAnchor = new Vector2(sourceBounds.max.x, sourceBounds.min.y);
                direction = new Vector2(1f, -1f);
                panelRectTransform.pivot = new Vector2(0f, 1f);
                break;
        }

        panelRectTransform.anchoredPosition = localAnchor +
            direction * sourceSpacing + positionOffset;
        ClampToCanvas();
    }

    private void ClampToCanvas()
    {
        Rect canvasRect = _placementRectTransform.rect;
        Rect panelRect = panelRectTransform.rect;
        Vector2 pivot = panelRectTransform.pivot;
        Vector2 position = panelRectTransform.anchoredPosition;

        float minX = canvasRect.xMin + canvasEdgePadding + panelRect.width * pivot.x;
        float maxX = canvasRect.xMax - canvasEdgePadding - panelRect.width * (1f - pivot.x);
        float minY = canvasRect.yMin + canvasEdgePadding + panelRect.height * pivot.y;
        float maxY = canvasRect.yMax - canvasEdgePadding - panelRect.height * (1f - pivot.y);

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        panelRectTransform.anchoredPosition = position;
    }
}
