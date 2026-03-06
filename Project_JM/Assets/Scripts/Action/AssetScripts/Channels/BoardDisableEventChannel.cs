// SPDX-License-Identifier: MIT
// Copyright (c) 11/11/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardDisableEventChannel.cs
// Summary: A scriptable object for board disable event channel.

using System;
using UnityEngine;

public enum BoardDisablePhase
{
    Preview,
    Commit,
}

public struct BoardDisableEventContext
{
    public BoardDisablePhase boardDisablePhase;
    public BoardDisableLogic boardDisableLogic;
}

[CreateAssetMenu(menuName = "JM/Events/Board Disable Event Channel")]
public class BoardDisableEventChannel : ScriptableObject
{
    public event Action<BoardDisableEventContext> OnRaised;

    public void Raise(BoardDisableEventContext context) => OnRaised?.Invoke(context);
}
