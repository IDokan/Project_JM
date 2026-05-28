// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 19/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ChunkDisableAttack.cs
// Summary: Disables an N×N block of gems at a randomly chosen origin, clamped to board bounds.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkDisableAttack", menuName = "JM/Combat/BoardDisableLogic/Chunk Disable Attack")]
public class ChunkDisableAttack : BoardDisableLogic
{
    [SerializeField] private int chunkSize = 5;

    private readonly List<Vector2Int> _indicesWillDisabled = new List<Vector2Int>();

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _indicesWillDisabled.Clear();

        IBoardInfo boardInfo = context.BoardInfo;

        // Guard against a chunk size larger than the board.
        int effectiveSize = Mathf.Clamp(chunkSize, 1, Mathf.Min(boardInfo.Rows, boardInfo.Cols));

        int originRow = GlobalRNG.Instance.NextInt(boardInfo.Rows - effectiveSize + 1);
        int originCol = GlobalRNG.Instance.NextInt(boardInfo.Cols - effectiveSize + 1);

        for (int r = 0; r < effectiveSize; r++)
        {
            for (int c = 0; c < effectiveSize; c++)
            {
                _indicesWillDisabled.Add(new Vector2Int(originRow + r, originCol + c));
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
