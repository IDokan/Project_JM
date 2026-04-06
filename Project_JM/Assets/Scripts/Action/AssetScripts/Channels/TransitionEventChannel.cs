// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/11/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TransitionEventChannel.cs
// Summary: A script for transition event channel.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using System;
using UnityEngine;

public enum TransitionPhase
{
    IntroTransitionBegin,
    IntroPartyMoveEnd,
    IntroBoardMoveEnd,
    MiddleTransitionStarts,
    MiddleTransitionEnd,
    EndTransitionBegin,
    EndPartyMoveEnd,
    EndEnemyMoveEnd,
    EndBoardMoveEnd,
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
