// SPDX-License-Identifier: MIT
// Copyright (c) 11/12/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackMotion.cs
// Summary: A script for enemy combat behaviour.

using DG.Tweening;
using System;
using UnityEngine;

public class AttackMotion : MonoBehaviour
{
    protected Animator _animator;

    // The durations can be changed in near future
    //      to display different pause for different attack logics.
    protected const float _moveDuration = 0.1f;
    protected const float _pauseDuration = 0.2f;

    protected static readonly int DamagedTrig = Animator.StringToHash("DamagedTrig");
    protected static readonly int AttackTrig = Animator.StringToHash("AttackTrig");
    protected static readonly int TierInt = Animator.StringToHash("TierInt");

    protected Vector3 _originalPosition;
    protected Sequence _moveSequence;

    protected void Awake()
    {
        _originalPosition = transform.localPosition;

        if (!_animator)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void PlayAttackMotion(Vector3 moveOffset)
    {
        if (!_animator)
        {
            return;
        }

        Move(moveOffset, 0.1f, 0.2f);

        _animator.ResetTrigger(AttackTrig);
        _animator.SetTrigger(AttackTrig);
    }

    public void PlayAttackMotion(int matchTier)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetInteger(TierInt, matchTier);

        _animator.ResetTrigger(AttackTrig);
        _animator.SetTrigger(AttackTrig);
    }

    public void PlayDamagedMotion(Vector3 moveOffset)
    {
        if (_animator == null)
        {
            return;
        }

        Move(moveOffset, 0.1f, 0.2f);

        _animator.ResetTrigger(DamagedTrig);
        _animator.SetTrigger(DamagedTrig);
    }

    protected void Move(Vector3 offset, float moveDuration, float pauseDuration)
    {
        // If previous sequence has not finished yet, kill it.
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        Vector3 target = _originalPosition + offset;

        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }
}
