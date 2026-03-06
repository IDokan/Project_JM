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

    protected List<Vector2Int> _indicesWillDisabled = new List<Vector2Int>();

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        var boardInfo = context.BoardInfo;

        _indicesWillDisabled.Clear();

        int maxTries = russianRouletteMax * _numDisableGems;
        int tries = 0;

        while (_indicesWillDisabled.Count < _numDisableGems && tries++ < maxTries)
        {
            Vector2Int index = RandomIndex(boardInfo);
            if (boardInfo.CanBeDisable(index) &&
                _indicesWillDisabled.Contains(index) == false)
            {
                _indicesWillDisabled.Add(index);
            }
        }
        return _indicesWillDisabled.ToArray();
    }

    public override IEnumerator Execute(BoardDisableContext context)
    {
        context.BoardInfo.DisableGems(_indicesWillDisabled);

        yield break;
    }

    protected Vector2Int RandomIndex(IBoardInfo boardInfo)
    {
        int row = GlobalRNG.Instance.NextInt(boardInfo.Rows);
        int col = GlobalRNG.Instance.NextInt(boardInfo.Cols);

        return new Vector2Int(row, col);
    }
}
