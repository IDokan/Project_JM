// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardMenu.cs
// Summary: Fetches and populates leaderboard rows on open; scope button toggles between
//          the player's own scores and the weekly global ranking.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

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
    [SerializeField] private Button pageUpButton;
    [SerializeField] private Button pageDownButton;

    [Header("Page scroll")]
    [SerializeField] private float pageScrollDuration = 0.3f;

    [Header("Icons")]
    [SerializeField] private Sprite[] iconSprites;

    private bool _showingGlobal;
    private List<LeaderboardEntry> _cachedGlobalEntries;
    private Dictionary<int, int> _globalRankByScore;
    private List<LeaderboardRow> _rows = new List<LeaderboardRow>();

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

    // TODO: remove before commit — for testing only
    protected void Start()
    {
        StartCoroutine(TestDelayedOpen());
    }

    private IEnumerator TestDelayedOpen()
    {
        yield return new WaitForSecondsRealtime(2f);

        _showingGlobal = false;
        _cachedGlobalEntries = null;

        leaderboardManager.FetchGlobalScores(OnGlobalScoresFetched);

        RefreshRows();
    }

    public override void Show(Selectable returnTo)
    {
        base.Show(returnTo);
        _showingGlobal = false;
        _cachedGlobalEntries = null;
        _globalRankByScore = null;

        leaderboardManager.FetchGlobalScores(OnGlobalScoresFetched);

        RefreshRows();
    }

    private void OnGlobalScoresFetched(List<LeaderboardEntry> entries)
    {
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
        _showingGlobal = !_showingGlobal;
        RefreshRows();
    }

    private void RefreshRows()
    {
        ClearRows();

        if (_showingGlobal)
        {
            if (_cachedGlobalEntries != null)
            {
                PopulateRows(_cachedGlobalEntries);
            }
        }
        else
        {
            PopulateRows(leaderboardManager.GetMyScores());
        }
    }

    private void ClearRows()
    {
        foreach (LeaderboardRow row in _rows)
        {
            Destroy(row.gameObject);
        }
        _rows.Clear();
    }

    private void PopulateRows(List<LeaderboardEntry> entries)
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
                leaderboardNavigation.OnRowSelected
            );
            _rows.Add(row);
        }

        leaderboardNavigation.SetupNavigation(_rows);
    }

    private void PageUp()
    {
        float target = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + GetPageStep());
        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(target, pageScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject);
    }

    private void PageDown()
    {
        float target = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - GetPageStep());
        scrollRect.DOKill();
        scrollRect.DOVerticalNormalizedPos(target, pageScrollDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(scrollRect.gameObject);
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

        // Score not in top GlobalFetchLimit — rank is unknown
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
