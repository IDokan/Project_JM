// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 24/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardRow.cs
// Summary: A single leaderboard row: highlights the player's own record,
//          displays three character icons, and shows the score.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

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

    [SerializeField] private Color myRecordColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color defaultColor = Color.white;

    private LeaderboardNavigation _navigation;
    private System.Action<Button> _onSelectedCallback;

    private void Awake()
    {
        _navigation = GetComponentInParent<LeaderboardNavigation>();
    }

    public void Initialize(bool isMyRecord, Sprite icon0, Sprite icon1, Sprite icon2, int score, int rank, System.Action<Button> onSelected = null)
    {
        background.color = isMyRecord ? myRecordColor : defaultColor;
        icons[0].sprite = icon0;
        icons[1].sprite = icon1;
        icons[2].sprite = icon2;
        scoreText.text = score.ToString();
        rankText.text = rank > 0 ? $"#{rank}" : "#--";
        _onSelectedCallback = onSelected;
    }

    public void SetRank(int rank)
    {
        rankText.text = rank > 0 ? $"#{rank}" : "#--";
    }

    public void OnSelect(BaseEventData eventData)
    {
        _navigation?.OnRowSelected(GetComponent<Button>());
        _onSelectedCallback?.Invoke(GetComponent<Button>());
    }
}
