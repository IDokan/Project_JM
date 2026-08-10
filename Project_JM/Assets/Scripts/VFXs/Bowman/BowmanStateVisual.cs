// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 10/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BowmanStateVisual.cs
// Summary: Bowman-specific visual responses — mouth changes on victory; eye, mouth, and fingers change on damaged.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.U2D.Animation;

public class BowmanStateVisual : CharacterStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private SpriteResolver mouthSpriteResolver;
    [SerializeField] private SpriteResolver fingersSpriteResolver;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string damagedEyeLabel;

    [Header("Mouth Labels")]
    [SerializeField] private string mouthCategory;
    [SerializeField] private string normalMouthLabel;
    [SerializeField] private string victoryMouthLabel;
    [SerializeField] private string damagedMouthLabel;

    [Header("Fingers Labels")]
    [SerializeField] private string fingersCategory;
    [SerializeField] private string normalFingersLabel;
    [SerializeField] private string damagedFingersLabel;

    public override void OnVictory()
    {
        base.OnVictory();
        SetMouth(victoryMouthLabel);
    }

    public override void OnVictoryEnd()
    {
        base.OnVictoryEnd();
        SetMouth(normalMouthLabel);
    }

    public override void OnDamagedBegin()
    {
        SetEye(damagedEyeLabel);
        SetMouth(damagedMouthLabel);
        SetFingers(damagedFingersLabel);
    }

    public override void OnDamagedEnd()
    {
        SetEye(normalEyeLabel);
        SetMouth(IsVictory ? victoryMouthLabel : normalMouthLabel);
        SetFingers(normalFingersLabel);
    }

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }

    private void SetMouth(string label)
    {
        mouthSpriteResolver.SetCategoryAndLabel(mouthCategory, label);
    }

    private void SetFingers(string label)
    {
        fingersSpriteResolver.SetCategoryAndLabel(fingersCategory, label);
    }
}
