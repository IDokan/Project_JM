// SPDX-License-Identifier: MIT
// Copyright (c) 11/11/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardDisableLogic.cs
// Summary: An abstract scriptable object for board disabler.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoardDisableLogic : ScriptableObject
{
    public abstract IEnumerator Execute(BoardDisableEventChannel channel);
}
