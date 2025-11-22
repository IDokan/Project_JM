// SPDX-License-Identifier: MIT
// Copyright (c) 11/12/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RandomBoardDisable.cs
// Summary: A board disable logic only for test purpose.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomBoardDisable", menuName = "JM/Combat/BoardDisableLogic/Random Board Disable")]
public class RandomBoardDisable : BoardDisableLogic
{
    [SerializeField] protected int _numDisableGems;
    protected int russianRouletteMax = 10;

    public override IEnumerator Execute(BoardDisableContext context)
    {
        int size = _numDisableGems;
        int russianRoulette = russianRouletteMax;
        var boardInfo = context.BoardInfo;
        do
        {
            var failedList = boardInfo.DisableGems(
                RandomIndices(size, boardInfo)
                );

            size = failedList.Count;
            
            russianRoulette--;
            // Do again if any index failed or...
        } while (size > 0 && russianRoulette > 0);

        yield break;
    }

    protected IReadOnlyList<Vector2Int> RandomIndices(int sizeIndices, IBoardInfo boardInfo)
    {
        var list = new List<Vector2Int>(sizeIndices);
        for (int i = 0; i < sizeIndices; i++)
        {
            int row = GlobalRNG.Instance.NextInt(boardInfo.Rows);
            int col = GlobalRNG.Instance.NextInt(boardInfo.Cols);
            list.Add(new Vector2Int(row, col));
        }

        return list;
    }
}
