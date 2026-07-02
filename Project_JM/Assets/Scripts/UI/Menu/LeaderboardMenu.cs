// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardMenu.cs
// Summary: Fetches and populates leaderboard rows on open; scope button toggles between
//          the player's own scores and the weekly global ranking.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardMenu : Menu
{
    [Header("Refs")]
    [SerializeField] private LeaderboardNavigation leaderboardNavigation;
    [SerializeField] private LeaderboardRow rowPrefab;
    [SerializeField] private RectTransform content;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button scopeButton;
    [SerializeField] private Image scopeButtonImage;
    [SerializeField] private Button pageUpButton;
    [SerializeField] private Button pageDownButton;
    [SerializeField] private CanvasGroup noInternetOverlay;
    [SerializeField] private CanvasGroup noLocalRecordsOverlay;

    [Header("Page scroll")]
    [SerializeField] private float pageScrollDuration = 0.3f;

    [Header("Icons")]
    [SerializeField] private LeaderboardIconContainer iconContainer;
    [SerializeField] private Sprite globalIconSprite;
    [SerializeField] private Sprite portraitIconSprite;

    private bool _showingGlobal;
    private bool _hasLiveEntry;
    private LeaderboardEntry _liveEntry;
    private List<LeaderboardRow> _rows = new List<LeaderboardRow>();
    private LeaderboardEntry? _lastSelectedEntry;
    private Button _initialButton;

    protected override void OnEnable()
    {
        base.OnEnable();
        scopeButton.onClick.AddListener(OnScopeClicked);
        pageUpButton.onClick.AddListener(PageUp);
        pageDownButton.onClick.AddListener(PageDown);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        scopeButton.onClick.RemoveListener(OnScopeClicked);
        pageUpButton.onClick.RemoveListener(PageUp);
        pageDownButton.onClick.RemoveListener(PageDown);
    }

    protected override void OnShowComplete()
    {
        base.OnShowComplete();
        Selectable target = _initialButton != null ? (Selectable)_initialButton : GetFirstSelectable();
        if (target != null)
        {
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }
    }

    public override void Show(Selectable returnTo)
    {
        base.Show(returnTo);
        _showingGlobal = false;
        _lastSelectedEntry = null;

        LeaderboardManager.Instance.FetchGlobalScores(() => RefreshRows());
        RefreshRows();
    }

    private void OnScopeClicked()
    {
        bool previousScope = _showingGlobal;
        _showingGlobal = !_showingGlobal;

        if (_showingGlobal)
        {
            LeaderboardManager.Instance.FetchGlobalScores(() => RefreshRows(previousScope));
        }

        RefreshRows(previousScope);
    }

    private void RefreshRows(bool? previousScope = null)
    {
        ClearRows();
        UpdateScopeButtonIcon();
        UpdateNoInternetOverlay();

        if (_showingGlobal)
        {
            noLocalRecordsOverlay.alpha = 0f;

            if (!LeaderboardManager.Instance.HasCachedGlobalData)
            {
                return;
            }

            PopulateRows(BuildBlendedEntries(), previousScope);
        }
        else
        {
            List<LeaderboardEntry> local = BuildLocalEntries();
            noLocalRecordsOverlay.alpha = local.Count == 0 ? 1f : 0f;
            PopulateRows(local, previousScope);
        }
    }

    private void UpdateScopeButtonIcon()
    {
        scopeButtonImage.sprite = _showingGlobal ? globalIconSprite : portraitIconSprite;
    }

    private void UpdateNoInternetOverlay()
    {
        noInternetOverlay.alpha = _showingGlobal && !LeaderboardManager.Instance.HasCachedGlobalData ? 1f : 0f;
    }

    public void ShowWithLiveEntry(Selectable returnTo, int score, int icon0, int icon1, int icon2)
    {
        _hasLiveEntry = true;
        _liveEntry = new LeaderboardEntry { score = score, icon0 = icon0, icon1 = icon1, icon2 = icon2, isMyRecord = true, isLiveEntry = true };
        Show(returnTo);
    }

    private List<LeaderboardEntry> BuildBlendedEntries()
    {
        IReadOnlyList<LeaderboardEntry> global = LeaderboardManager.Instance.GetGlobalEntries();
        List<LeaderboardEntry> result = new List<LeaderboardEntry>(global);

        HashSet<int> myGlobalScores = new HashSet<int>();
        foreach (LeaderboardEntry entry in result)
        {
            if (entry.isMyRecord) { myGlobalScores.Add(entry.score); }
        }

        foreach (LeaderboardEntry local in LeaderboardManager.Instance.GetMyScores())
        {
            if (!myGlobalScores.Contains(local.score))
            {
                result.Add(local);
                myGlobalScores.Add(local.score);
            }
        }

        if (_hasLiveEntry)
        {
            result.Add(_liveEntry);
        }

        result.Sort((a, b) => b.score.CompareTo(a.score));
        return result;
    }

    private List<LeaderboardEntry> BuildLocalEntries()
    {
        List<LeaderboardEntry> result = LeaderboardManager.Instance.GetMyScores();

        if (_hasLiveEntry)
        {
            result.Add(_liveEntry);
            result.Sort((a, b) => b.score.CompareTo(a.score));
        }

        return result;
    }

    private void ClearRows()
    {
        foreach (LeaderboardRow row in _rows)
        {
            row.gameObject.SetActive(false);
            Destroy(row.gameObject);
        }
        _rows.Clear();
    }

    private void PopulateRows(List<LeaderboardEntry> entries, bool? previousScope = null)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            int rank = i < LeaderboardManager.GlobalFetchLimit ? i + 1 : 0;

            LeaderboardRow row = Instantiate(rowPrefab, content);
            row.Initialize(
                entries[i].isMyRecord,
                GetSprite(entries[i].icon0),
                GetSprite(entries[i].icon1),
                GetSprite(entries[i].icon2),
                entries[i].score,
                rank,
                OnRowEntrySelected
            );
            if (entries[i].isLiveEntry) { row.Highlight(); }
            _rows.Add(row);
        }

        int initialIndex = FindInitialSelectionIndex(entries, previousScope);
        bool centerScroll = previousScope != null;
        leaderboardNavigation.SetupNavigation(_rows, initialIndex, centerScroll);
        _initialButton = _rows.Count > 0 ? _rows[Mathf.Clamp(initialIndex, 0, _rows.Count - 1)].GetComponent<Button>() : null;

        // Re-select after global fetch rebuilds rows mid-session; OnShowComplete handles the initial open case.
        // Skip when previousScope != null: scope button triggered the rebuild, so focus stays on it.
        if (canvasGroup.interactable && previousScope == null)
        {
            Selectable target = _initialButton != null ? (Selectable)_initialButton : GetFirstSelectable();
            if (target != null)
            {
                EventSystem.current.SetSelectedGameObject(target.gameObject);
            }
        }
    }

    private void OnRowEntrySelected(LeaderboardEntry entry)
    {
        _lastSelectedEntry = entry;
    }

    private int FindInitialSelectionIndex(List<LeaderboardEntry> entries, bool? previousScope)
    {
        if (_lastSelectedEntry == null || entries.Count == 0)
        {
            if (_hasLiveEntry)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].isLiveEntry) { return i; }
                }
            }
            return 0;
        }

        LeaderboardEntry last = _lastSelectedEntry.Value;

        if (previousScope == null)
        {
            // Same-scope refresh: restore exact score, fallback to closest
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].score == last.score)
                {
                    return i;
                }
            }
            return FindClosestScoreIndex(entries, last.score);
        }

        bool wasShowingGlobal = previousScope.Value;

        if (!wasShowingGlobal)
        {
            // Local -> Global: find exact score match
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].score == last.score)
                {
                    return i;
                }
            }
            return 0;
        }

        // Global -> Local
        if (last.isMyRecord)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].score == last.score)
                {
                    return i;
                }
            }
        }

        return FindClosestScoreIndex(entries, last.score);
    }

    private int FindClosestScoreIndex(List<LeaderboardEntry> entries, int targetScore)
    {
        int closestIndex = 0;
        int closestDiff = int.MaxValue;
        for (int i = 0; i < entries.Count; i++)
        {
            int diff = entries[i].score >= targetScore
                ? entries[i].score - targetScore
                : targetScore - entries[i].score;
            if (diff < closestDiff)
            {
                closestDiff = diff;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private void PageUp()
    {
        float target = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + GetPageStep());
        UpdateLastSelectedEntry(target);
        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(target, pageScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject);
    }

    private void PageDown()
    {
        float target = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - GetPageStep());
        UpdateLastSelectedEntry(target);
        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(target, pageScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject);
    }

    private void UpdateLastSelectedEntry(float targetNormalizedPos)
    {
        if (_rows.Count == 0) { return; }

        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight <= 0f) { return; }

        float scrollTopFromContentTop = (1f - targetNormalizedPos) * scrollableHeight;
        float viewportCenterFromContentTop = scrollTopFromContentTop + viewportHeight * 0.5f;

        LeaderboardRow result = null;
        float closestDist = float.MaxValue;

        foreach (LeaderboardRow row in _rows)
        {
            Vector2 localCenter = scrollRect.content.InverseTransformPoint(row.transform.position);
            float rowDistFromContentTop = -localCenter.y;
            float dist = Mathf.Abs(rowDistFromContentTop - viewportCenterFromContentTop);
            if (dist < closestDist)
            {
                closestDist = dist;
                result = row;
            }
        }

        if (result == null) { result = _rows[_rows.Count / 2]; }

        _lastSelectedEntry = result.Entry;
        leaderboardNavigation.OnRowSelected(result.GetComponent<Button>());
    }

    private float GetPageStep()
    {
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight <= 0f)
        {
            return 0f;
        }

        return viewportHeight / scrollableHeight;
    }

    private Sprite GetSprite(int id)
    {
        if (iconContainer == null)
        {
            return null;
        }

        return iconContainer.GetSprite(id);
    }
}


