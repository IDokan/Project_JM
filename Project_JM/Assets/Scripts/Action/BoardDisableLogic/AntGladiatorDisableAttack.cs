// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AntGladiatorDisableAttack.cs
// Summary: Disables a full column with rows 2 and 4 always shifted ±1, plus an optional extra shift on row 1 or 5.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AntGladiatorDisableAttack", menuName = "JM/Combat/BoardDisableLogic/Ant Gladiator Disable Attack")]
public class AntGladiatorDisableAttack : BoardDisableLogic
{
    private const int ExtraNone = 0;
    private const int ExtraTwistRow1 = 1;
    private const int ExtraTwistRow5 = 2;
    private const int ExtraOptionCount = 3;

    private readonly List<Vector2Int> _previewedIndices = new List<Vector2Int>();

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _previewedIndices.Clear();

        IBoardInfo boardInfo = context.BoardInfo;
        int rows = boardInfo.Rows;

        int shift2 = GlobalRNG.Instance.NextInt(2) == 0 ? -1 : 1;
        int shift4 = GlobalRNG.Instance.NextInt(2) == 0 ? -1 : 1;
        int extraOption = GlobalRNG.Instance.NextInt(ExtraOptionCount);

        int baseCol = ChooseBaseColumn(boardInfo, shift2, shift4, extraOption);

        for (int row = 0; row < rows; row++)
        {
            int col = GetColumnForRow(row, baseCol, shift2, shift4, extraOption);
            _previewedIndices.Add(new Vector2Int(row, col));
        }

        return _previewedIndices;
    }

    public override IEnumerator Execute(BoardDisableContext context)
    {
        context.BoardInfo.DisableGems(_previewedIndices);

        yield break;
    }

    private int ChooseBaseColumn(IBoardInfo board, int shift2, int shift4, int extraOption)
    {
        var noOverlapColumns = new List<int>();
        var validColumns = new List<int>();

        for (int col = 0; col < board.Cols; col++)
        {
            if (!ShiftsAreInBounds(col, board, shift2, shift4))
            {
                continue;
            }

            validColumns.Add(col);

            if (!HasOverlap(board, col, shift2, shift4, extraOption))
            {
                noOverlapColumns.Add(col);
            }
        }

        if (noOverlapColumns.Count > 0)
        {
            return noOverlapColumns[GlobalRNG.Instance.NextInt(noOverlapColumns.Count)];
        }

        return validColumns[GlobalRNG.Instance.NextInt(validColumns.Count)];
    }

    private bool ShiftsAreInBounds(int baseCol, IBoardInfo board, int shift2, int shift4)
    {
        int twisted2 = baseCol + shift2;
        int twisted4 = baseCol + shift4;
        return twisted2 >= 0 && twisted2 < board.Cols && twisted4 >= 0 && twisted4 < board.Cols;
    }

    private bool HasOverlap(IBoardInfo board, int baseCol, int shift2, int shift4, int extraOption)
    {
        for (int row = 0; row < board.Rows; row++)
        {
            int col = GetColumnForRow(row, baseCol, shift2, shift4, extraOption);
            if (!board.CanBeDisable(new Vector2Int(row, col)))
            {
                return true;
            }
        }

        return false;
    }

    private int GetColumnForRow(int row, int baseCol, int shift2, int shift4, int extraOption)
    {
        if (row == 2)
        {
            return baseCol + shift2;
        }

        if (row == 4)
        {
            return baseCol + shift4;
        }

        if (row == 1 && extraOption == ExtraTwistRow1)
        {
            return baseCol + shift2;
        }

        if (row == 5 && extraOption == ExtraTwistRow5)
        {
            return baseCol + shift4;
        }

        return baseCol;
    }
}
