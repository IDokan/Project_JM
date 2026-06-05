// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialSequenceData.cs
// Summary: Ordered list of tutorial steps associated with a specific TutorialProgress level.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using TutorialEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Tutorial/Tutorial Sequence")]
public class TutorialSequenceData : ScriptableObject
{
    [SerializeField] private TutorialProgress forProgress;
    public TutorialProgress ForProgress => forProgress;

    [SerializeField] private List<TutorialStepData> steps = new();
    public IReadOnlyList<TutorialStepData> Steps => steps;
}
