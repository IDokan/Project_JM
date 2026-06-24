// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 24/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ScrollRectAutoScroll.cs
// Summary: Automatically scrolls a ScrollRect to keep the currently selected UI element in view.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectAutoScroll : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    public void OnItemSelected(RectTransform target)
    {
        if (!target.IsChildOf(content))
        {
            return;
        }

        ScrollToItem(target);
    }

    private void ScrollToItem(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight <= 0f)
        {
            return;
        }

        // Item center in Content's local space; y is negative growing downward
        Vector2 itemLocalCenter = content.InverseTransformPoint(target.position);
        float distanceFromTop = -itemLocalCenter.y;
        float targetScrollTop = distanceFromTop - viewportHeight * 0.5f;

        scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(targetScrollTop / scrollableHeight);
    }
}
