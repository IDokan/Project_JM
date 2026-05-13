// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SnailWizardStateVisual.cs
// Summary: SnailWizard-specific visual responses — eye/mouth sprite changes and attack motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
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
    [SerializeField] private string normalAttackMouthLabel;
    [SerializeField] private string enragedMouthLabel;
    [SerializeField] private string enragedAttackMouthLabel;
    [SerializeField] private string damagedMouthLabel;

    [Header("Attack Motion")]
    [SerializeField] private float readyDuration = 0.1f;
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float pauseDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float moveOffsetMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float readyOffsetMultiplier = 0.5f;

    private bool _isEnraged;
    private Vector3 _originalPosition;
    private Sequence _moveSequence;
    private float _timeScaler = 1f;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        GlobalTimeManager.OnScaleChanged += ApplyGlobalTweenScale;
    }

    private void OnDisable()
    {
        GlobalTimeManager.OnScaleChanged -= ApplyGlobalTweenScale;
    }

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
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        SetEye(deadEyeLabel);
    }

    public override void OnAttack(Vector3 moveOffset)
    {
        Move(moveOffset * moveOffsetMultiplier);
    }

    private void Move(Vector3 offset)
    {
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        Vector3 readyPosition = _originalPosition - offset * readyOffsetMultiplier;
        Vector3 target = _originalPosition + offset;
        string attackLabel = _isEnraged ? enragedAttackMouthLabel : normalAttackMouthLabel;
        string idleLabel = _isEnraged ? enragedMouthLabel : normalMouthLabel;

        _moveSequence = DOTween.Sequence()
            .AppendCallback(() => SetMouth(attackLabel))
            .Append(transform.DOLocalMove(readyPosition, readyDuration).SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .AppendCallback(() => SetMouth(idleLabel))
            .SetLink(gameObject);

        _moveSequence.timeScale = _timeScaler;
    }

    private void ApplyGlobalTweenScale(float scale)
    {
        _timeScaler = scale;

        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.timeScale = scale;
        }
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
