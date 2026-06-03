// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardActionTutorialStep.cs
// Summary: Tutorial step that unlocks a single highlighted cell and waits for the player to match gems.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(menuName = "JM/Tutorial/Board Action Step")]
public class BoardActionTutorialStep : TutorialStepData
{
    [SerializeField] private Vector2Int highlightedCell;
    public Vector2Int HighlightedCell => highlightedCell;
}
