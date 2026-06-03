// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TimerTutorialStep.cs
// Summary: Tutorial step that locks the board and auto-advances after a fixed duration.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(menuName = "JM/Tutorial/Timer Step")]
public class TimerTutorialStep : TutorialStepData
{
    [SerializeField] private float duration = 3f;
    public float Duration => duration;
}
