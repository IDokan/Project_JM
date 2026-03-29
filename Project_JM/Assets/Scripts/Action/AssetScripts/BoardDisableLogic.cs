// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/11/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardDisableLogic.cs
// Summary: An abstract scriptable object for board disabler.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoardDisableContext
{
    public IBoardInfo BoardInfo;
}

public abstract class BoardDisableLogic : ScriptableObject
{
    public abstract IReadOnlyList<Vector2Int> PreviewGemWillDisabled(BoardDisableContext context);
    public abstract IEnumerator Execute(BoardDisableContext context);
}
