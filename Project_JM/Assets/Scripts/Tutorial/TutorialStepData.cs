// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialStepData.cs
// Summary: Abstract base ScriptableObject for a single step in a tutorial sequence.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialStepData : ScriptableObject
{
    [SerializeField] private List<TutorialSpriteEntry> sprites = new();
    public IReadOnlyList<TutorialSpriteEntry> Sprites => sprites;
}
