// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 28/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SlimeKingStateVisual.cs
// Summary: SlimeKing-specific visual responses — eye sprite changes, enrage particle, and attack motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class SlimeKingStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private ParticleSystem enrageParticle;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string enragedEyeLabel;
    [SerializeField] private string stunnedEyeLabel;
    [SerializeField] private string deadEyeLabel;

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
        SetEye(enragedEyeLabel);
        enrageParticle?.Play();
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
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        SetEye(deadEyeLabel);
        enrageParticle?.Stop();
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

        _moveSequence = DOTween.Sequence()
            .Append(transform.DOLocalMove(readyPosition, readyDuration).SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
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
}
