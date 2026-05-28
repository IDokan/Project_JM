// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RowColDisableAttack.cs
// Summary: Disables a rows×cols rectangle of gems at a randomly chosen valid origin, clamped to board bounds.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RowColDisableAttack", menuName = "JM/Combat/BoardDisableLogic/Row Col Disable Attack")]
public class RowColDisableAttack : BoardDisableLogic
{
    [SerializeField] private int rows = 3;
    [SerializeField] private int cols = 1;

    private const int MaxTries = 100;

    private readonly List<Vector2Int> _indicesWillDisabled = new List<Vector2Int>();

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _indicesWillDisabled.Clear();

        IBoardInfo boardInfo = context.BoardInfo;

        int effectiveRows = Mathf.Clamp(rows, 1, boardInfo.Rows);
        int effectiveCols = Mathf.Clamp(cols, 1, boardInfo.Cols);

        int originRowRange = boardInfo.Rows - effectiveRows + 1;
        int originColRange = boardInfo.Cols - effectiveCols + 1;

        int originRow;
        int originCol;

        // Large rectangles resolve immediately — no need to validate individual cells.
        if (effectiveRows >= 3 || effectiveCols >= 3)
        {
            originRow = GlobalRNG.Instance.NextInt(originRowRange);
            originCol = GlobalRNG.Instance.NextInt(originColRange);

            for (int r = 0; r < effectiveRows; r++)
            {
                for (int c = 0; c < effectiveCols; c++)
                {
                    _indicesWillDisabled.Add(new Vector2Int(originRow + r, originCol + c));
                }
            }
        }
        else
        {
            for (int tries = 0; tries < MaxTries; tries++)
            {
                originRow = GlobalRNG.Instance.NextInt(originRowRange);
                originCol = GlobalRNG.Instance.NextInt(originColRange);

                bool allValid = true;
                for (int r = 0; r < effectiveRows && allValid; r++)
                {
                    for (int c = 0; c < effectiveCols && allValid; c++)
                    {
                        if (!boardInfo.CanBeDisable(new Vector2Int(originRow + r, originCol + c)))
                        {
                            allValid = false;
                        }
                    }
                }

                if (allValid)
                {
                    for (int r = 0; r < effectiveRows; r++)
                    {
                        for (int c = 0; c < effectiveCols; c++)
                        {
                            _indicesWillDisabled.Add(new Vector2Int(originRow + r, originCol + c));
                        }
                    }
                    break;
                }
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
