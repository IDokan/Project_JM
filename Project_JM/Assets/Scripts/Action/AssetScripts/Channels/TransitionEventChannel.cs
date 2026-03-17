// SPDX-License-Identifier: MIT
// Copyright (c) 03/11/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TransitionEventChannel.cs
// Summary: A script for transition event channel.


using System;
using UnityEngine;

public enum TransitionPhase
{
    IntroPartyMoveEnd,
    IntroBoardMoveEnd,
    MiddleTransitionStarts,
}

[CreateAssetMenu(menuName = "JM/Events/TransitionEventChannel")]
public class TransitionEventChannel : ScriptableObject
{
    public Action<TransitionPhase> OnRaised;

    public void Raise(TransitionPhase phase)
    {
        OnRaised?.Invoke(phase);
    }
}
