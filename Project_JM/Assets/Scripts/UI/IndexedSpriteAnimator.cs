// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IndexedSpriteAnimator.cs
// Summary: Advances a UI Image through a sprite array one frame per call.
//          Fades out over 1 second after 1 second of inactivity.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IndexedSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fadeDelay = 1f;
    [SerializeField] private float fadeDuration = 1f;

    private int _currentIndex;

    private void Start()
    {
        SetAlpha(0f);
    }

    public void NextFrame()
    {
        if (frames == null || frames.Length == 0) return;

        _currentIndex = (_currentIndex + 1) % frames.Length;
        image.sprite = frames[_currentIndex];

        image.DOKill();
        SetAlpha(1f);
        image.DOFade(0f, fadeDuration).SetDelay(fadeDelay).SetUpdate(true);
    }

    public void ResetFrame()
    {
        _currentIndex = 0;
        if (frames != null && frames.Length > 0)
            image.sprite = frames[0];

        image.DOKill();
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
