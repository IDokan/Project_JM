// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FoxThiefStateVisual.cs
// Summary: FoxThief-specific visual responses — eye/mouth sprite changes and finger open on death with right-finger draw order adjustment.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class FoxThiefStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private SpriteResolver mouthSpriteResolver;
    [SerializeField] private SpriteResolver leftFingersSpriteResolver;
    [SerializeField] private SpriteResolver rightFingersSpriteResolver;
    [SerializeField] private SpriteRenderer rightFingersSpriteRenderer;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string enragedEyeLabel;
    [SerializeField] private string deadEyeLabel;

    [Header("Mouth Labels")]
    [SerializeField] private string mouthCategory;
    [SerializeField] private string normalMouthLabel;
    [SerializeField] private string enragedMouthLabel;
    [SerializeField] private string deadMouthLabel;

    [Header("Fingers Labels")]
    [SerializeField] private string leftFingersCategory;
    [SerializeField] private string rightFingersCategory;
    [SerializeField] private string openFingersLabel;

    [Header("SFX")]
    [SerializeField] private AudioCueSO normalAttackSFX;

    [Header("Attack Motion")]
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float pauseDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float moveOffsetMultiplier = 1f;

    private bool _isEnraged;
    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
    }

    public override void OnEnraged()
    {
        _isEnraged = true;
        SetEye(enragedEyeLabel);
        SetMouth(enragedMouthLabel);
    }

    public override void OnStunBegin()
    {
        SetEye(deadEyeLabel);
        SetMouth(deadMouthLabel);
    }

    public override void OnStunEnd()
    {
        SetEye(_isEnraged ? enragedEyeLabel : normalEyeLabel);
        SetMouth(_isEnraged ? enragedMouthLabel : normalMouthLabel);
    }

    public override void OnDied()
    {
        SetEye(deadEyeLabel);
        SetMouth(deadMouthLabel);
        leftFingersSpriteResolver.SetCategoryAndLabel(leftFingersCategory, openFingersLabel);
        rightFingersSpriteResolver.SetCategoryAndLabel(rightFingersCategory, openFingersLabel);
        rightFingersSpriteRenderer.sortingOrder -= 10;
    }

    public override Sequence BuildAttackSequence(Vector3 moveOffset)
    {
        Vector3 target = _originalPosition + moveOffset * moveOffsetMultiplier;

        return DOTween.Sequence()
            .Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    public override void OnWin()
    {
        SetEye(normalEyeLabel);
        SetMouth(normalMouthLabel);
    }

    public void AnimEvent_PlayNormalAttackSound() => AudioManager.Instance.PlayActionSFX(normalAttackSFX);

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }

    private void SetMouth(string label)
    {
        mouthSpriteResolver.SetCategoryAndLabel(mouthCategory, label);
    }
}
