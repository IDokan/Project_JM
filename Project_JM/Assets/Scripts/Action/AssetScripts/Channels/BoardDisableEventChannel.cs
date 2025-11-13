// SPDX-License-Identifier: MIT
// Copyright (c) 11/11/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardDisableEventChannel.cs
// Summary: A scriptable object for board disable event channel.

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Events/Board Disable Event Channel")]
public class BoardDisableEventChannel : ScriptableObject
{
    public event Action<BoardDisableLogic> OnRaised;

    public void Raise(BoardDisableLogic logic) => OnRaised?.Invoke(logic);
}
