// SPDX-License-Identifier: MIT
// Copyright (c) 03/11/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IntroEventChannel.cs
// Summary: A script for intro event channel.


using System;
using UnityEngine;

public enum IntroSequencePhase
{
    PartyMoveEnd,
    BoardMoveEnd,
}

[CreateAssetMenu(menuName = "JM/Events/IntroEventChannel")]
public class IntroEventChannel : ScriptableObject
{
    public Action<IntroSequencePhase> OnRaised;

    public void Raise(IntroSequencePhase phase)
    {
        OnRaised?.Invoke(phase);
    }
}
