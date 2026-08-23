// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/05/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIFollowPoint.cs
// Summary: A script to move UI to follow point of BasePlayerController.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class UIFollowPoint : MonoBehaviour
{
    [SerializeField] protected RectTransform uiTransform;
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Camera uiCamera;
    [SerializeField] protected BasePlayerController playerController;

    // Landscape offset; also the default when no orientation-specific value applies.
    [SerializeField] protected Vector2 offset;
    [SerializeField] protected Vector2 portraitOffset;
    [SerializeField] protected Vector2 padModeAnchoredPosition = Vector2.zero;

    protected Vector2 _activeOffset;

    protected virtual void Reset()
    {
        uiTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    protected virtual void Awake()
    {
        if (uiTransform == null)
        {
            uiTransform = GetComponent<RectTransform>();
        }
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        bool isPortrait = Screen.height > Screen.width;
        _activeOffset = isPortrait ? portraitOffset : offset;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (playerController == null || playerController.IsPadMode)
        {
            uiTransform.anchoredPosition = padModeAnchoredPosition;
            return;
        }

        Vector2 screenPoint = playerController.GetCurrentFollowPoint();
        MoveToScreenPoint(screenPoint + _activeOffset);
    }

    protected void MoveToScreenPoint(Vector2 screenPoint)
    {
        if (uiTransform == null || canvas == null)
        {
            return;
        }

        RectTransform canvasTransform = canvas.transform as RectTransform;

        if (canvasTransform == null)
        {
            return;
        }

        Vector2 localPoint;
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera,
            out localPoint);

        if (success == false)
        {
            return;
        }

        uiTransform.anchoredPosition = ClampAnchoredPosition(canvasTransform, localPoint);
    }

    // Since old codes that clamp the UI using GetWorldCorners it not appropriate from canvas using Screen Space - Camera,
    // Created a whole new function that manually calculates each corners. 
    protected Vector2 ClampAnchoredPosition(RectTransform canvasRectTransform, Vector2 targetAnchoredPos)
    {
        Vector2 canvasSize = canvasRectTransform.rect.size;
        Vector2 uiSize = uiTransform.rect.size;

        Vector2 pivot = uiTransform.pivot;

        float minX = -canvasSize.x * 0.5f + uiSize.x * pivot.x;
        float maxX = canvasSize.x * 0.5f - uiSize.x * (1f - pivot.x);

        float minY = -canvasSize.y * 0.5f + uiSize.y * pivot.y;
        float maxY = canvasSize.y * 0.5f - uiSize.y * (1f - pivot.y);

        targetAnchoredPos.x = Mathf.Clamp(targetAnchoredPos.x, minX, maxX);
        targetAnchoredPos.y = Mathf.Clamp(targetAnchoredPos.y, minY, maxY);

        return targetAnchoredPos;
    }
}
