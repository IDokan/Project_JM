// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AntGladiatorStateVisual.cs
// Summary: AntGladiator enemy visual — eye/mouth sprite changes via SpriteResolver and DOTween lunge attack motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class AntGladiatorStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver leftEyeSpriteResolver;
    [SerializeField] private SpriteResolver rightEyeSpriteResolver;
    [SerializeField] private SpriteResolver mouthSpriteResolver;

    [Header("Eye Labels")]
    [SerializeField] private string leftEyeCategory;
    [SerializeField] private string rightEyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string deadEyeLabel;
    [SerializeField] private string enragedEyeLabel;
    [SerializeField] private string stunnedEyeLabel;

    [Header("Mouth Labels")]
    [SerializeField] private string mouthCategory;
    [SerializeField] private string normalMouthLabel;
    [SerializeField] private string smallOpenMouthLabel;
    [SerializeField] private string bigOpenMouthLabel;

    [Header("SFX")]
    [SerializeField] private AudioCueSO greatSwordHitSFX;

    [Header("Attack Motion")]
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float pauseDuration = 0.2f;
    [SerializeField] private float returnDuration = 0.1f;
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
    }

    public override void OnStunBegin()
    {
        SetEye(stunnedEyeLabel);
        SetMouth(smallOpenMouthLabel);
    }

    public override void OnStunEnd()
    {
        SetEye(_isEnraged ? enragedEyeLabel : normalEyeLabel);
        SetMouth(normalMouthLabel);
    }

    public override void OnDied()
    {
        SetEye(deadEyeLabel);
        SetMouth(smallOpenMouthLabel);
    }

    public override void OnWin()
    {
        SetEye(normalEyeLabel);
        SetMouth(normalMouthLabel);
    }

    public override Sequence BuildAttackSequence(Vector3 moveOffset)
    {
        SetMouth(bigOpenMouthLabel);
        Vector3 target = _originalPosition + moveOffset * moveOffsetMultiplier;

        return DOTween.Sequence()
            .Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, returnDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    public override void OnAttackEnd()
    {
        SetMouth(normalMouthLabel);
    }

    private void SetEye(string label)
    {
        leftEyeSpriteResolver.SetCategoryAndLabel(leftEyeCategory, label);
        rightEyeSpriteResolver.SetCategoryAndLabel(rightEyeCategory, label);
    }

    public void AnimEvent_GreatSwordHit() => AudioManager.Instance.PlayActionSFX(greatSwordHitSFX);

    private void SetMouth(string label)
    {
        mouthSpriteResolver.SetCategoryAndLabel(mouthCategory, label);
    }
}
