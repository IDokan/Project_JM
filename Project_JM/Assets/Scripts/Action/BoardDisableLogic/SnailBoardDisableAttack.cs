// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 19/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SnailBoardDisableAttack.cs
// Summary: Disables 9 gems — 8 border cells of a 4x4 ring plus one inner cell that
//          cycles CCW through the 2x2 box on each call.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SnailBoardDisableAttack", menuName = "JM/Combat/BoardDisableLogic/Snail Board Disable Attack")]
public class SnailBoardDisableAttack : BoardDisableLogic
{
    // Ring offsets in row-major order (ascending row, then col) for cache-friendly iteration.
    // BasePattern spans [0, 3] on both axes from the ring origin.
    private static readonly Vector2Int[] BasePattern =
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, 2),
        new Vector2Int(1, 0),
        new Vector2Int(1, 3),
        new Vector2Int(2, 0),
        new Vector2Int(2, 3),
        new Vector2Int(3, 1),
        new Vector2Int(3, 2),
    };

    // One inner cell disabled per call, cycling CCW: bottom-left → bottom-right → top-right → top-left.
    private static readonly Vector2Int[] InnerDotIndices =
    {
        new Vector2Int(1, 1),  // bottom-left
        new Vector2Int(1, 2),  // bottom-right
        new Vector2Int(2, 2),  // top-right
        new Vector2Int(2, 1),  // top-left
    };

    private const int PatternMaxOffset = 3;
    private const int MaxTries = 10;

    private readonly List<Vector2Int> _indicesWillDisabled = new List<Vector2Int>();
    private int _innerDotIndex;

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _indicesWillDisabled.Clear();

        IBoardInfo boardInfo = context.BoardInfo;
        int originRowMax = boardInfo.Rows - 1 - PatternMaxOffset;
        int originColMax = boardInfo.Cols - 1 - PatternMaxOffset;

        int originRow = 0;
        int originCol = 0;

        for (int tries = 0; tries < MaxTries; tries++)
        {
            originRow = GlobalRNG.Instance.NextInt(originRowMax + 1);
            originCol = GlobalRNG.Instance.NextInt(originColMax + 1);

            bool allValid = true;
            foreach (Vector2Int offset in BasePattern)
            {
                if (!boardInfo.CanBeDisable(new Vector2Int(originRow + offset.x, originCol + offset.y)))
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                break;
            }
        }

        foreach (Vector2Int offset in BasePattern)
        {
            _indicesWillDisabled.Add(new Vector2Int(originRow + offset.x, originCol + offset.y));
        }

        Vector2Int innerDot = InnerDotIndices[_innerDotIndex];
        _indicesWillDisabled.Add(new Vector2Int(originRow + innerDot.x, originCol + innerDot.y));
        _innerDotIndex = (_innerDotIndex + 1) % 4;

        return _indicesWillDisabled;
    }

    public override IEnumerator Execute(BoardDisableContext context)
    {
        context.BoardInfo.DisableGems(_indicesWillDisabled);

        yield break;
    }
}
