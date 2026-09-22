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
    private MonoBehaviour _activeTrigger;
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
        bool isContentRefresh = _activeTrigger == trigger;
        _activeTrigger = trigger;
        titleText.text = title;
        descriptionText.text = description;

        /* Resolve the tooltip's size before positioning without forcing all canvases.
         * Consider batching localized content updates if profiling warrants it (#58). */
        titleText.ForceMeshUpdate();
        descriptionText.ForceMeshUpdate();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
        Position(source, placement, positionOffset);

        if (isContentRefresh)
        {
            return;
        }

        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
        }

        _showRoutine = StartCoroutine(ShowRoutine());
    }

    public void Show(
        WorldTooltipTrigger trigger,
        string title,
        string description,
        Bounds sourceBounds,
        Camera sourceCamera,
        TooltipPlacement placement,
        Vector2 positionOffset)
    {
        bool isContentRefresh = _activeTrigger == trigger;
        _activeTrigger = trigger;
        titleText.text = title;
        descriptionText.text = description;

        titleText.ForceMeshUpdate();
        descriptionText.ForceMeshUpdate();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
        Position(sourceBounds, sourceCamera, placement, positionOffset);

        if (isContentRefresh)
        {
            return;
        }

        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
        }

        _showRoutine = StartCoroutine(ShowRoutine());
    }

    public void Hide(MonoBehaviour trigger)
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
        Position(sourceBounds, placement, positionOffset);
    }

    private void Position(
        Bounds sourceBounds,
        Camera sourceCamera,
        TooltipPlacement placement,
        Vector2 positionOffset)
    {
        Vector3 screenMin = sourceCamera.WorldToScreenPoint(sourceBounds.min);
        Vector3 screenMax = sourceCamera.WorldToScreenPoint(sourceBounds.max);
        Camera canvasCamera = GetCanvasCamera();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _placementRectTransform,
            screenMin,
            canvasCamera,
            out Vector2 localMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _placementRectTransform,
            screenMax,
            canvasCamera,
            out Vector2 localMax);
        Bounds localBounds = new Bounds(
            (localMin + localMax) * 0.5f,
            new Vector3(
                Mathf.Abs(localMax.x - localMin.x),
                Mathf.Abs(localMax.y - localMin.y),
                0f));
        Position(localBounds, placement, positionOffset);
    }

    private void Position(Bounds sourceBounds, TooltipPlacement placement, Vector2 positionOffset)
    {
        Vector2 localAnchor;
        Vector2 direction;
        switch (placement)
        {
            case TooltipPlacement.Left:
                localAnchor = new Vector2(sourceBounds.min.x, sourceBounds.center.y);
                direction = Vector2.left;
                panelRectTransform.pivot = new Vector2(1f, 0.5f);
                break;
            case TooltipPlacement.Right:
                localAnchor = new Vector2(sourceBounds.max.x, sourceBounds.center.y);
                direction = Vector2.right;
                panelRectTransform.pivot = new Vector2(0f, 0.5f);
                break;
            case TooltipPlacement.Up:
                localAnchor = new Vector2(sourceBounds.center.x, sourceBounds.max.y);
                direction = Vector2.up;
                panelRectTransform.pivot = new Vector2(0.5f, 0f);
                break;
            case TooltipPlacement.Down:
                localAnchor = new Vector2(sourceBounds.center.x, sourceBounds.min.y);
                direction = Vector2.down;
                panelRectTransform.pivot = new Vector2(0.5f, 1f);
                break;
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

    private Camera GetCanvasCamera()
    {
        Canvas canvas = _placementRectTransform.GetComponentInParent<Canvas>();
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
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
