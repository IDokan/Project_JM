// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 23/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: KnightSFXPlayer.cs
// Summary: Plays Knight-specific SFX on animation events.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class KnightSFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioCueSO shieldAttackSFX;
    [SerializeField] private AudioCueSO bashSFX;
    [SerializeField] private AudioCueSO armorHitSFX;

    public void AnimEvent_ShieldAttack() => AudioManager.Instance.PlayActionSFX(shieldAttackSFX);
    public void AnimEvent_Bash() => AudioManager.Instance.PlayActionSFX(bashSFX);
    public void AnimEvent_ArmorHit() => AudioManager.Instance.PlayActionSFX(armorHitSFX);
}
