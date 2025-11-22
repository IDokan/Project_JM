// SPDX-License-Identifier: MIT
// Copyright (c) 11/11/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ImmediateBoardDisable.cs
// Summary: A board disable logic only for test purpose.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImmediateBoardDisable", menuName = "JM/Combat/BoardDisableLogic/Immediate Board Disable")]
public class ImmediateBoardDisable : BoardDisableLogic
{
    [SerializeField] protected List<Vector2Int> disableIndices;

    public override IEnumerator Execute(BoardDisableContext context)
    {
        context.BoardInfo.DisableGems(disableIndices);

        yield break;
    }
}
