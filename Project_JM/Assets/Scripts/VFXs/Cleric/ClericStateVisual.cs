// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 10/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ClericStateVisual.cs
// Summary: Cleric-specific visual responses — eye/beak sprite changes for damaged and victory states.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.U2D.Animation;

public class ClericStateVisual : CharacterStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private SpriteResolver beakSpriteResolver;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string victoryEyeLabel;
    [SerializeField] private string damagedEyeLabel;

    [Header("Beak Labels")]
    [SerializeField] private string beakCategory;
    [SerializeField] private string normalBeakLabel;
    [SerializeField] private string victoryBeakLabel;
    [SerializeField] private string damagedBeakLabel;

    public override void OnVictory()
    {
        base.OnVictory();
        SetEye(victoryEyeLabel);
        SetBeak(victoryBeakLabel);
    }

    public override void OnVictoryEnd()
    {
        base.OnVictoryEnd();
        SetEye(normalEyeLabel);
        SetBeak(normalBeakLabel);
    }

    public override void OnDamagedBegin()
    {
        SetEye(damagedEyeLabel);
        SetBeak(damagedBeakLabel);
    }

    public override void OnDamagedEnd()
    {
        SetEye(IsVictory ? victoryEyeLabel : normalEyeLabel);
        SetBeak(IsVictory ? victoryBeakLabel : normalBeakLabel);
    }

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }

    private void SetBeak(string label)
    {
        beakSpriteResolver.SetCategoryAndLabel(beakCategory, label);
    }
}
