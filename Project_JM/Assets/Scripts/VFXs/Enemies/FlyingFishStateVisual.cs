// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 22/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FlyingFishStateVisual.cs
// Summary: FlyingFish-specific visual responses — eye sprite changes via SpriteResolver and DOTween attack motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class FlyingFishStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string enragedEyeLabel;
    [SerializeField] private string stunnedEyeLabel;
    [SerializeField] private string deadEyeLabel;

    [Header("Enrage Splash")]
    [SerializeField] private float splashDownDistance = 0.5f;
    [SerializeField] private float splashDuration = 0.15f;
    [SerializeField] private float splashPauseDuration = 0.1f;

    [Header("Attack Motion")]
    [SerializeField] private float readyDuration = 0.1f;
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float pauseDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float moveOffsetMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float readyOffsetMultiplier = 0.5f;

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
    }

    public override void OnStunEnd()
    {
        SetEye(_isEnraged ? enragedEyeLabel : normalEyeLabel);
    }

    public override void OnDied()
    {
        SetEye(deadEyeLabel);
    }

    public override Sequence BuildAttackSequence(Vector3 moveOffset)
    {
        Vector3 offset = moveOffset * moveOffsetMultiplier;
        Vector3 readyPosition = _originalPosition - offset * readyOffsetMultiplier;
        Vector3 target = _originalPosition + offset;

        return DOTween.Sequence()
            .Append(transform.DOLocalMove(readyPosition, readyDuration).SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    public override Sequence BuildEnragedSequence()
    {
        Vector3 splashTarget = _originalPosition + Vector3.down * splashDownDistance;

        return DOTween.Sequence()
            .Append(transform.DOLocalMove(splashTarget, splashDuration).SetEase(Ease.OutQuad))
            .AppendInterval(splashPauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, splashDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }
}
