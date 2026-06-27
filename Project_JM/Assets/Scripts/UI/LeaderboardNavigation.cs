// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardNavigation.cs
// Summary: Manages explicit gamepad/keyboard navigation between the title button,
//          scope button, page buttons, and leaderboard rows. Call SetupNavigation after rows are built.
//          Also owns smooth scroll-to-row on selection.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardNavigation : MonoBehaviour
{
    [SerializeField] private Button titleButton;
    [SerializeField] private Button scopeButton;
    [SerializeField] private Button pageUpButton;
    [SerializeField] private Button pageDownButton;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private float rowScrollDuration = 0.3f;

    private Button _lastSelectedRightButton;

    public void SetupNavigation(List<LeaderboardRow> rows, int initialIndex = 0, bool centerScroll = false)
    {
        _lastSelectedRightButton = scopeButton;

        SetStaticNavigation(rows);

        if (rows.Count > 0)
        {
            int clampedIndex = Mathf.Clamp(initialIndex, 0, rows.Count - 1);
            Button initialButton = rows[clampedIndex].GetComponent<Button>();
            SetRowNavigation(initialButton);
            if (centerScroll)
            {
                ScrollToRowCentered(initialButton.transform as RectTransform);
            }
            else
            {
                ScrollToRow(initialButton.transform as RectTransform);
            }
        }
    }

    public void OnRightPanelButtonSelected(Button button)
    {
        _lastSelectedRightButton = button;
    }

    public void OnRowSelected(Button rowButton)
    {
        SetRowNavigation(rowButton);
        ScrollToRow(rowButton.transform as RectTransform);
    }

    private void SetRowNavigation(Button rowButton)
    {
        Navigation titleNav = titleButton.navigation;
        titleNav.selectOnDown = rowButton;
        titleButton.navigation = titleNav;

        Navigation scopeNav = scopeButton.navigation;
        scopeNav.selectOnLeft = rowButton;
        scopeButton.navigation = scopeNav;

        Navigation pageUpNav = pageUpButton.navigation;
        pageUpNav.selectOnLeft = rowButton;
        pageUpButton.navigation = pageUpNav;

        Navigation pageDownNav = pageDownButton.navigation;
        pageDownNav.selectOnLeft = rowButton;
        pageDownButton.navigation = pageDownNav;

        Navigation rowNav = rowButton.navigation;
        rowNav.selectOnRight = _lastSelectedRightButton;
        rowButton.navigation = rowNav;
    }

    private void SetStaticNavigation(List<LeaderboardRow> rows)
    {
        Navigation titleNav = new Navigation();
        titleNav.mode = Navigation.Mode.Explicit;
        titleNav.selectOnRight = scopeButton;
        titleButton.navigation = titleNav;

        Navigation scopeNav = new Navigation();
        scopeNav.mode = Navigation.Mode.Explicit;
        scopeNav.selectOnUp = titleButton;
        scopeNav.selectOnDown = pageUpButton;
        scopeButton.navigation = scopeNav;

        Navigation pageUpNav = new Navigation();
        pageUpNav.mode = Navigation.Mode.Explicit;
        pageUpNav.selectOnUp = scopeButton;
        pageUpNav.selectOnDown = pageDownButton;
        pageUpButton.navigation = pageUpNav;

        Navigation pageDownNav = new Navigation();
        pageDownNav.mode = Navigation.Mode.Explicit;
        pageDownNav.selectOnUp = pageUpButton;
        pageDownButton.navigation = pageDownNav;

        for (int i = 0; i < rows.Count; i++)
        {
            Button button = rows[i].GetComponent<Button>();
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = i == 0 ? titleButton : rows[i - 1].GetComponent<Button>();
            nav.selectOnDown = i < rows.Count - 1 ? rows[i + 1].GetComponent<Button>() : null;
            button.navigation = nav;
        }
    }

    private void ScrollToRow(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight <= 0f)
        {
            return;
        }

        Vector2 itemLocalCenter = scrollRect.content.InverseTransformPoint(target.position);
        float distanceFromTop = -itemLocalCenter.y;
        float rowTop = distanceFromTop - target.rect.height;
        float rowBottom = distanceFromTop + target.rect.height;

        float currentScrollTop = scrollableHeight * (1f - scrollRect.verticalNormalizedPosition);
        float currentScrollBottom = currentScrollTop + viewportHeight;

        float normalizedTarget;

        if (rowTop < currentScrollTop)
        {
            // row is (at least partially) above the viewport
            normalizedTarget = 1f - Mathf.Clamp01(rowTop / scrollableHeight);
        }
        else if (rowBottom > currentScrollBottom)
        {
            // row is (at least partially) below the viewport
            normalizedTarget = 1f - Mathf.Clamp01((rowBottom - viewportHeight) / scrollableHeight);
        }
        else
        {
            // row is fully visible - do nothing
            return;
        }

        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(normalizedTarget, rowScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject);
    }

    private void ScrollToRowCentered(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight <= 0f)
        {
            return;
        }

        Vector2 itemLocalCenter = scrollRect.content.InverseTransformPoint(target.position);
        float distanceFromTop = -itemLocalCenter.y;
        float centeredScrollTop = distanceFromTop - viewportHeight * 0.5f;
        float normalizedTarget = 1f - Mathf.Clamp01(centeredScrollTop / scrollableHeight);

        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(normalizedTarget, rowScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject);
    }
}
