// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 10/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: KnightStateVisual.cs
// Summary: Knight-specific visual responses — eye/mouth sprite changes for damaged and victory states.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.U2D.Animation;

public class KnightStateVisual : CharacterStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private SpriteResolver mouthSpriteResolver;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string victoryEyeLabel;
    [SerializeField] private string damagedEyeLabel;

    [Header("Mouth Labels")]
    [SerializeField] private string mouthCategory;
    [SerializeField] private string normalMouthLabel;
    [SerializeField] private string victoryMouthLabel;
    [SerializeField] private string damagedMouthLabel;

    public override void OnVictory()
    {
        base.OnVictory();
        SetEye(victoryEyeLabel);
        SetMouth(victoryMouthLabel);
    }

    public override void OnVictoryEnd()
    {
        base.OnVictoryEnd();
        SetEye(normalEyeLabel);
        SetMouth(normalMouthLabel);
    }

    public override void OnDamagedBegin()
    {
        SetEye(damagedEyeLabel);
        SetMouth(damagedMouthLabel);
    }

    public override void OnDamagedEnd()
    {
        SetEye(IsVictory ? victoryEyeLabel : normalEyeLabel);
        SetMouth(IsVictory ? victoryMouthLabel : normalMouthLabel);
    }

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }

    private void SetMouth(string label)
    {
        mouthSpriteResolver.SetCategoryAndLabel(mouthCategory, label);
    }
}
