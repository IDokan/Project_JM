// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SlimeKingHardDisableAttack.cs
// Summary: Disables a 2x2 gem block at a randomly chosen column (0, 3, or 6); finds the lowest row with no overlap and a one-row gap on both sides.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlimeKingHardDisableAttack", menuName = "JM/Combat/BoardDisableLogic/Slime King Hard Disable Attack")]
public class SlimeKingHardDisableAttack : BoardDisableLogic
{
    private static readonly int[] ColumnOrigins = { 0, 3, 6 };

    private readonly List<Vector2Int> _indicesWillDisabled = new List<Vector2Int>();

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _indicesWillDisabled.Clear();

        IBoardInfo boardInfo = context.BoardInfo;
        int originCol = ColumnOrigins[GlobalRNG.Instance.NextInt(ColumnOrigins.Length)];
        int originRow = ComputeOriginRow(boardInfo, originCol);

        _indicesWillDisabled.Add(new Vector2Int(originRow,     originCol));
        _indicesWillDisabled.Add(new Vector2Int(originRow,     originCol + 1));
        _indicesWillDisabled.Add(new Vector2Int(originRow + 1, originCol));
        _indicesWillDisabled.Add(new Vector2Int(originRow + 1, originCol + 1));

        return _indicesWillDisabled;
    }

    public override IEnumerator Execute(BoardDisableContext context)
    {
        context.BoardInfo.DisableGems(_indicesWillDisabled);

        yield break;
    }

    private int ComputeOriginRow(IBoardInfo boardInfo, int originCol)
    {
        int maxOriginRow = boardInfo.Rows - 2;

        for (int row = 0; row <= maxOriginRow; row++)
        {
            if (IsBoxClear(boardInfo, row, originCol) && IsSpacingClear(boardInfo, row, originCol))
            {
                return row;
            }
        }

        // Fallback: ignore spacing, just avoid overlap.
        for (int row = 0; row <= maxOriginRow; row++)
        {
            if (IsBoxClear(boardInfo, row, originCol))
            {
                return row;
            }
        }

        return maxOriginRow;
    }

    private bool IsBoxClear(IBoardInfo boardInfo, int originRow, int originCol)
    {
        for (int r = originRow; r <= originRow + 1; r++)
        {
            for (int c = originCol; c <= originCol + 1; c++)
            {
                if (!boardInfo.CanBeDisable(new Vector2Int(r, c)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Checks the rows immediately below (originRow - 1) and above (originRow + 2) the box
    // are free so disabled gems don't sit adjacent and trigger auto-matching.
    private bool IsSpacingClear(IBoardInfo boardInfo, int originRow, int originCol)
    {
        int belowRow = originRow - 1;
        int aboveRow = originRow + 2;

        for (int c = originCol; c <= originCol + 1; c++)
        {
            if (belowRow >= 0 && !boardInfo.CanBeDisable(new Vector2Int(belowRow, c)))
            {
                return false;
            }

            if (aboveRow < boardInfo.Rows && !boardInfo.CanBeDisable(new Vector2Int(aboveRow, c)))
            {
                return false;
            }
        }

        return true;
    }
}
