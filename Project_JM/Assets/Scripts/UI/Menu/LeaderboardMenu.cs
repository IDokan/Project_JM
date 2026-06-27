// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardMenu.cs
// Summary: Fetches and populates leaderboard rows on open; scope button toggles between
//          the player's own scores and the weekly global ranking.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardMenu : Menu
{
    [Header("Refs")]
    [SerializeField] private LeaderboardManager leaderboardManager;
    [SerializeField] private LeaderboardNavigation leaderboardNavigation;
    [SerializeField] private LeaderboardRow rowPrefab;
    [SerializeField] private RectTransform content;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button scopeButton;
    [SerializeField] private Image scopeButtonImage;
    [SerializeField] private Button pageUpButton;
    [SerializeField] private Button pageDownButton;
    [SerializeField] private CanvasGroup noInternetOverlay;

    [Header("Page scroll")]
    [SerializeField] private float pageScrollDuration = 0.3f;

    [Header("Icons")]
    [SerializeField] private Sprite[] iconSprites;
    [SerializeField] private Sprite globalIconSprite;
    [SerializeField] private Sprite portraitIconSprite;

    private bool _showingGlobal;
    private bool _isFetchingGlobal;
    private List<LeaderboardEntry> _cachedGlobalEntries;
    private Dictionary<int, int> _globalRankByScore;
    private List<LeaderboardRow> _rows = new List<LeaderboardRow>();
    private LeaderboardEntry? _lastSelectedEntry;

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

    // TODO: remove before commit ??for testing only
    protected void Start()
    {
        StartCoroutine(TestDelayedOpen());
    }

    private IEnumerator TestDelayedOpen()
    {
        yield return new WaitForSecondsRealtime(4f);

        _showingGlobal = false;
        _cachedGlobalEntries = null;

        if (_cachedGlobalEntries == null && !_isFetchingGlobal)
        {
            _isFetchingGlobal = true;
            leaderboardManager.FetchGlobalScores(OnGlobalScoresFetched);
        }

        yield return new WaitForSecondsRealtime(2f);
        RefreshRows();
    }

    public override void Show(Selectable returnTo)
    {
        base.Show(returnTo);
        _showingGlobal = false;
        _lastSelectedEntry = null;

        if (_cachedGlobalEntries == null && !_isFetchingGlobal)
        {
            _isFetchingGlobal = true;
            leaderboardManager.FetchGlobalScores(OnGlobalScoresFetched);
        }

        RefreshRows();
    }

    private void OnGlobalScoresFetched(List<LeaderboardEntry> entries)
    {
        _isFetchingGlobal = false;

        if (entries == null)
        {
            RefreshRows();
            return;
        }

        _cachedGlobalEntries = entries;
        _globalRankByScore = new Dictionary<int, int>();

        int rank = 1;
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0 && entries[i].score < entries[i - 1].score)
            {
                rank = i + 1;
            }

            if (!_globalRankByScore.ContainsKey(entries[i].score))
            {
                _globalRankByScore[entries[i].score] = rank;
            }
        }

        RefreshRows();
    }

    private void OnScopeClicked()
    {
        bool previousScope = _showingGlobal;
        _showingGlobal = !_showingGlobal;
        RefreshRows(previousScope);
    }

    private void RefreshRows(bool? previousScope = null)
    {
        ClearRows();
        UpdateScopeButtonIcon();
        UpdateNoInternetOverlay();

        if (_showingGlobal)
        {
            if (_cachedGlobalEntries == null)
            {
                return;
            }

            PopulateRows(_cachedGlobalEntries, previousScope);
        }
        else
        {
            PopulateRows(leaderboardManager.GetMyScores(), previousScope);
        }
    }

    private void UpdateScopeButtonIcon()
    {
        scopeButtonImage.sprite = _showingGlobal ? globalIconSprite : portraitIconSprite;
    }

    private void UpdateNoInternetOverlay()
    {
        noInternetOverlay.alpha = _showingGlobal && _cachedGlobalEntries == null ? 1f : 0f;
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
            LeaderboardRow row = Instantiate(rowPrefab, content);
            row.Initialize(
                entries[i].isMyRecord,
                GetSprite(entries[i].icon0),
                GetSprite(entries[i].icon1),
                GetSprite(entries[i].icon2),
                entries[i].score,
                GetGlobalRank(entries[i].score),
                OnRowEntrySelected
            );
            _rows.Add(row);
        }

        int initialIndex = FindInitialSelectionIndex(entries, previousScope);
        leaderboardNavigation.SetupNavigation(_rows, initialIndex);
    }

    private void OnRowEntrySelected(LeaderboardEntry entry)
    {
        _lastSelectedEntry = entry;
    }

    private int FindInitialSelectionIndex(List<LeaderboardEntry> entries, bool? previousScope)
    {
        if (_lastSelectedEntry == null || entries.Count == 0)
        {
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
        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(target, pageScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject)
            .OnComplete(() => UpdateLastSelectedEntry(findTop: true));
    }

    private void PageDown()
    {
        float target = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - GetPageStep());
        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(target, pageScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject)
            .OnComplete(() => UpdateLastSelectedEntry(findTop: false));
    }

    private void UpdateLastSelectedEntry(bool findTop)
    {
        if (_rows.Count == 0)
        {
            return;
        }

        Rect viewportRect = scrollRect.viewport.rect;
        float targetY = findTop
            ? scrollRect.viewport.TransformPoint(new Vector2(0f, viewportRect.yMax)).y
            : scrollRect.viewport.TransformPoint(new Vector2(0f, viewportRect.yMin)).y;

        LeaderboardRow nearest = _rows[0];
        float nearestDist = float.MaxValue;

        foreach (LeaderboardRow row in _rows)
        {
            float dist = Mathf.Abs(row.transform.position.y - targetY);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = row;
            }
        }

        _lastSelectedEntry = nearest.Entry;
        leaderboardNavigation.OnRowSelected(nearest.GetComponent<Button>());
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

    private int GetGlobalRank(int score)
    {
        if (_globalRankByScore == null)
        {
            return 0;
        }

        if (_globalRankByScore.TryGetValue(score, out int rank))
        {
            return rank;
        }

        // Score not in top GlobalFetchLimit ??rank is unknown
        return 0;
    }

    private Sprite GetSprite(int index)
    {
        if (iconSprites == null || index < 0 || index >= iconSprites.Length)
        {
            return null;
        }

        return iconSprites[index];
    }
}


