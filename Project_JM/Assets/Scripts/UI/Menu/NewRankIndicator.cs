// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: NewRankIndicator.cs
// Summary: Fades in when the run's score earns a new local leaderboard rank; fades out on click.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NewRankIndicator : MonoBehaviour
{
    [SerializeField] private Image indicatorImage;
    [SerializeField] private Button indicatorButton;

    [SerializeField] private float blinkHalfDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private void Awake()
    {
        Color color = indicatorImage.color;
        color.a = 0f;
        indicatorImage.color = color;
    }

    protected void OnEnable() => indicatorButton.onClick.AddListener(OnClicked);
    protected void OnDisable() => indicatorButton.onClick.RemoveListener(OnClicked);

    public void Highlight()
    {
        indicatorImage.DOKill();
        indicatorImage.DOFade(1f, blinkHalfDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(indicatorImage.gameObject);
    }

    public void Hide()
    {
        indicatorImage.DOKill();
        indicatorImage.DOFade(0f, fadeOutDuration).SetUpdate(true).SetLink(indicatorImage.gameObject);
    }

    private void OnClicked()
    {
        Hide();
    }

}
