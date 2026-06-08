// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 08/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackTutorialStep.cs
// Summary: Tutorial step that hides the overlay, triggers one enemy attack via the event channel,
//          waits a fixed delay for the animation to finish, then shows the next tutorial panel.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(menuName = "JM/Tutorial/Enemy Attack Step")]
public class EnemyAttackTutorialStep : TutorialStepData
{
    [SerializeField] private float delay = 2f;
    public float Delay => delay;
}
