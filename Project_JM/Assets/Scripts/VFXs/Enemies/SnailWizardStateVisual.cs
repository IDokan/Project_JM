// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SnailWizardStateVisual.cs
// Summary: SnailWizard-specific visual responses — eye/mouth sprite changes and attack motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.U2D.Animation;

public class SnailWizardStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private SpriteResolver mouthSpriteResolver;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string deadEyeLabel;

    [Header("Mouth Labels")]
    [SerializeField] private string mouthCategory;
    [SerializeField] private string normalMouthLabel;
    [SerializeField] private string enragedMouthLabel;
    [SerializeField] private string damagedMouthLabel;

    private bool _isEnraged;

    public override void OnEnraged()
    {
        _isEnraged = true;
        SetMouth(enragedMouthLabel);
    }

    public override void OnStunBegin()
    {
        SetMouth(damagedMouthLabel);
    }

    public override void OnStunEnd()
    {
        SetMouth(_isEnraged ? enragedMouthLabel : normalMouthLabel);
    }

    public override void OnDied()
    {
        SetEye(deadEyeLabel);
    }

    public override void OnAttack(Vector3 moveOffset) { }

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }

    private void SetMouth(string label)
    {
        mouthSpriteResolver.SetCategoryAndLabel(mouthCategory, label);
    }
}
