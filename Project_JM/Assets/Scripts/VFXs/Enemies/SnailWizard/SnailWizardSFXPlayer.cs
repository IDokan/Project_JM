// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 16/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SnailWizardSFXPlayer.cs
// Summary: Plays SnailWizard-specific combat SFX on animation events.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class SnailWizardSFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioCueSO magicCircleSpawnSFX;
    [SerializeField] private AudioCueSO throwSFX;
    [SerializeField] private AudioCueSO hitSFX;

    public void AnimEvent_MagicCircleSpawn() => AudioManager.Instance.PlayActionSFX(magicCircleSpawnSFX);
    public void AnimEvent_Throw() => AudioManager.Instance.PlayActionSFX(throwSFX);
    public void AnimEvent_Hit() => AudioManager.Instance.PlayActionSFX(hitSFX);
}
