// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialBoardHighlighter.cs
// Summary: Spawns a world-space highlight sprite over a specific board cell during a board action tutorial step.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;

public class TutorialBoardHighlighter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer highlightPrefab;

    private SpriteRenderer _activeHighlight;
    private Tween _pulseTween;

    public void ShowAt(Vector2Int cell, BoardManager board)
    {
        Hide();

        Vector2 localPos = board.GetGemLocation(cell.x, cell.y);
        Vector3 worldPos = board.transform.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));

        _activeHighlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity);

        _pulseTween = _activeHighlight.transform
            .DOScale(1.15f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Hide()
    {
        _pulseTween?.Kill();
        _pulseTween = null;

        if (_activeHighlight != null)
        {
            Destroy(_activeHighlight.gameObject);
            _activeHighlight = null;
        }
    }
}
