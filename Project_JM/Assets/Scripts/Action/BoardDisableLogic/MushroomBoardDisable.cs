// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MushroomBoardDisable.cs
// Summary: Disables three gem cells in a fixed mushroom-cap pattern around a random center.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MushroomBoardDisable", menuName = "JM/Combat/BoardDisableLogic/Mushroom Board Disable")]
public class MushroomBoardDisable : BoardDisableLogic
{
    // Pattern offsets relative to center (row, col). Center itself is not disabled.
    // Visually: left cap, top, right cap — mushroom shape.
    private static readonly Vector2Int[] MushroomPattern =
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
    };

    // Pre-computed from pattern: center must stay within these offsets from the board edge.
    private const int MinRowOffset = -1;
    private const int MaxRowOffset = 1;
    private const int MinColOffset = 0;
    private const int MaxColOffset = 1;

    private const int MaxTries = 100;

    private readonly List<Vector2Int> _indicesWillDisabled = new List<Vector2Int>();

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _indicesWillDisabled.Clear();

        IBoardInfo boardInfo = context.BoardInfo;

        // Clamp center range so no pattern cell can ever land outside the board.
        int rowMin = -MinRowOffset;
        int rowMax = boardInfo.Rows - 1 - MaxRowOffset;
        int colMin = -MinColOffset;
        int colMax = boardInfo.Cols - 1 - MaxColOffset;

        int rowRange = rowMax - rowMin + 1;
        int colRange = colMax - colMin + 1;

        for (int tries = 0; tries < MaxTries; tries++)
        {
            int row = rowMin + GlobalRNG.Instance.NextInt(rowRange);
            int col = colMin + GlobalRNG.Instance.NextInt(colRange);
            Vector2Int center = new Vector2Int(row, col);

            bool allValid = true;
            foreach (Vector2Int offset in MushroomPattern)
            {
                if (!boardInfo.CanBeDisable(center + offset))
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                foreach (Vector2Int offset in MushroomPattern)
                {
                    _indicesWillDisabled.Add(center + offset);
                }
                break;
            }
        }

        return _indicesWillDisabled;
    }

    public override IEnumerator Execute(BoardDisableContext context)
    {
        context.BoardInfo.DisableGems(_indicesWillDisabled);

        yield break;
    }
}
