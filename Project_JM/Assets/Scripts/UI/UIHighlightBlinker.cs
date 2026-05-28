// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIHighlightBlinker.cs
// Summary: Loops a fade blink on a CanvasGroup while active; resets to invisible on disable.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIHighlightBlinker : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.15f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 1f;
        _canvasGroup.DOFade(0f, fadeDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
    }
}
