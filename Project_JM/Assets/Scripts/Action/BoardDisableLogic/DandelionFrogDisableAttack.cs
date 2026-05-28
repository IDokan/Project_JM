// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DandelionFrogDisableAttack.cs
// Summary: Disables 5 gems using a randomly chosen pattern from a curated set that fits a 3x3 box
//          with no three disabled cells consecutive horizontally or vertically.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DandelionFrogDisableAttack", menuName = "JM/Combat/BoardDisableLogic/Dandelion Frog Disable Attack")]
public class DandelionFrogDisableAttack : BoardDisableLogic
{
    // 14 curated patterns within a 3x3 box.
    // Each pattern has exactly 5 cells; no row or column is fully disabled.
    private static readonly Vector2Int[][] Patterns =
    {
        // A3  X../·XX/XX·
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 0), new Vector2Int(2, 1) },
        // A6  ·X·/XX·/X·X
        new[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 0), new Vector2Int(2, 2) },
        // A10 ·X·/·XX/X·X
        new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 0), new Vector2Int(2, 2) },
        // A13 ··X/XX·/·XX
        new[] { new Vector2Int(0, 2), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
        // B8  X·X/·X·/X·X
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0), new Vector2Int(2, 2) },
        // B10 ·XX/·X·/X·X
        new[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0), new Vector2Int(2, 2) },
        // B11 XX·/··X/XX·
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 2), new Vector2Int(2, 0), new Vector2Int(2, 1) },
        // B15 ·XX/··X/XX·
        new[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 0), new Vector2Int(2, 1) },
        // C1  XX·/·XX/X··
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 0) },
        // C7  X·X/XX·/·X·
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 2), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
        // C8  X·X/X·X/·X·
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 2), new Vector2Int(1, 0), new Vector2Int(1, 2), new Vector2Int(2, 1) },
        // C9  X·X/·XX/·X·
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) },
        // C10 ·XX/X·X/·X·
        new[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0), new Vector2Int(1, 2), new Vector2Int(2, 1) },
        // C15 ·XX/XX·/··X
        new[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) },
    };

    [SerializeField] private float intervalSeconds = 0.1f;

    private const int MaxTries = 50;

    private readonly List<Vector2Int> _indicesWillDisabled = new List<Vector2Int>();
    private readonly List<Vector2Int> _singleGem = new List<Vector2Int> { default };

    public override IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context)
    {
        _indicesWillDisabled.Clear();

        IBoardInfo boardInfo = context.BoardInfo;

        // All patterns fit within a 3x3 box; offsets go up to (2, 2).
        int rowRange = boardInfo.Rows - 2;
        int colRange = boardInfo.Cols - 2;

        for (int tries = 0; tries < MaxTries; tries++)
        {
            Vector2Int[] pattern = Patterns[GlobalRNG.Instance.NextInt(Patterns.Length)];
            int originRow = GlobalRNG.Instance.NextInt(rowRange);
            int originCol = GlobalRNG.Instance.NextInt(colRange);

            bool allValid = true;
            foreach (Vector2Int offset in pattern)
            {
                if (!boardInfo.CanBeDisable(new Vector2Int(originRow + offset.x, originCol + offset.y)))
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                foreach (Vector2Int offset in pattern)
                {
                    _indicesWillDisabled.Add(new Vector2Int(originRow + offset.x, originCol + offset.y));
                }
                break;
            }
        }

        return _indicesWillDisabled;
    }

    public override IEnumerator Execute(BoardDisableContext context)
    {
        for (int i = _indicesWillDisabled.Count - 1; i > 0; i--)
        {
            int j = GlobalRNG.Instance.NextInt(i + 1);
            Vector2Int temp = _indicesWillDisabled[i];
            _indicesWillDisabled[i] = _indicesWillDisabled[j];
            _indicesWillDisabled[j] = temp;
        }

        for (int i = 0; i < _indicesWillDisabled.Count; i++)
        {
            _singleGem[0] = _indicesWillDisabled[i];
            context.BoardInfo.DisableGems(_singleGem);
            yield return GlobalTimeManager.WaitForGlobalSeconds(intervalSeconds);
        }
    }
}
