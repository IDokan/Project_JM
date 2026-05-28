// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FoxThiefBoardDisableAttack.cs
// Summary: Disables two gem patterns — one immediately and one after a configurable delay.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoxThiefBoardDisableAttack", menuName = "JM/Combat/BoardDisableLogic/Fox Thief Board Disable Attack")]
public class FoxThiefBoardDisableAttack : BoardDisableLogic
{
    [SerializeField] private float delaySeconds = 1f;

    private static readonly Vector2Int[] ImmediatePattern =
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 1),
        new Vector2Int(2, 1),
    };

    private static readonly Vector2Int[] DelayedPattern =
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, 2),
    };

    private const int MaxTries = 100;

    private readonly List<Vector2Int> _immediateIndices = new List<Vector2Int>();
    private readonly List<Vector2Int> _delayedIndices = new List<Vector2Int>();
    private readonly List<Vector2Int> _previewIndices = new List<Vector2Int>();

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _immediateIndices.Clear();
        _delayedIndices.Clear();
        _previewIndices.Clear();

        IBoardInfo boardInfo = context.BoardInfo;

        TryPlacePattern(boardInfo, ImmediatePattern, _immediateIndices, null);
        TryPlacePattern(boardInfo, DelayedPattern, _delayedIndices, _immediateIndices);

        _previewIndices.AddRange(_immediateIndices);
        _previewIndices.AddRange(_delayedIndices);

        return _previewIndices;
    }

    public override IEnumerator Execute(BoardDisableContext context)
    {
        context.BoardInfo.DisableGems(_immediateIndices);

        yield return GlobalTimeManager.WaitForGlobalSeconds(delaySeconds);

        context.BoardInfo.DisableGems(_delayedIndices);
    }

    private void TryPlacePattern(IBoardInfo boardInfo, Vector2Int[] pattern, List<Vector2Int> result, List<Vector2Int> excluded)
    {
        int maxRowOffset = 0;
        int maxColOffset = 0;
        foreach (Vector2Int offset in pattern)
        {
            if (offset.x > maxRowOffset) { maxRowOffset = offset.x; }
            if (offset.y > maxColOffset) { maxColOffset = offset.y; }
        }

        int rowRange = boardInfo.Rows - maxRowOffset;
        int colRange = boardInfo.Cols - maxColOffset;

        for (int tries = 0; tries < MaxTries; tries++)
        {
            int originRow = GlobalRNG.Instance.NextInt(rowRange);
            int originCol = GlobalRNG.Instance.NextInt(colRange);

            bool allValid = true;
            foreach (Vector2Int offset in pattern)
            {
                Vector2Int candidate = new Vector2Int(originRow + offset.x, originCol + offset.y);

                if (!boardInfo.CanBeDisable(candidate))
                {
                    allValid = false;
                    break;
                }

                if (excluded != null && excluded.Contains(candidate))
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                foreach (Vector2Int offset in pattern)
                {
                    result.Add(new Vector2Int(originRow + offset.x, originCol + offset.y));
                }
                break;
            }
        }
    }
}
