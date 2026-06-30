// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 24/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardRow.cs
// Summary: A single leaderboard row: highlights the player's own record,
//          displays three character icons, and shows the score.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardRow : MonoBehaviour, ISelectHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image[] icons;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private NewRankIndicator newRankIndicator;

    [SerializeField] private Color myRecordColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color defaultColor = Color.white;

    private LeaderboardNavigation _navigation;
    private Action<LeaderboardEntry> _onSelected;
    private LeaderboardEntry _entry;

    public LeaderboardEntry Entry => _entry;

    private void Awake()
    {
        _navigation = GetComponentInParent<LeaderboardNavigation>();
    }

    public void Highlight()
    {
        newRankIndicator.Highlight();
    }

    public void Initialize(bool isMyRecord, Sprite icon0, Sprite icon1, Sprite icon2, int score, int rank, Action<LeaderboardEntry> onSelected)
    {
        _entry = new LeaderboardEntry { score = score, isMyRecord = isMyRecord };
        _onSelected = onSelected;
        background.color = isMyRecord ? myRecordColor : defaultColor;
        SetIcon(icons[0], icon0);
        SetIcon(icons[1], icon1);
        SetIcon(icons[2], icon2);
        scoreText.text = score.ToString();
        rankText.text = rank > 0 ? $"#{rank}" : "#--";
    }

    public void SetRank(int rank)
    {
        rankText.text = rank > 0 ? $"#{rank}" : "#--";
    }

    private void SetIcon(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.SetNativeSize();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _onSelected?.Invoke(_entry);
        _navigation?.OnRowSelected(GetComponent<Button>());
    }
}

