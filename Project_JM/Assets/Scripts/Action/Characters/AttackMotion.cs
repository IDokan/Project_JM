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

    public void Move(Vector3 offset, float moveDuration, float pauseDuration)
    {
        // If previous sequence has not finished yet, kill it.
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        Vector3 target = _originalPosition + offset;

        _moveSequence = DOTween.Sequence();
        seq.Append(transform.DOLocalMove(target, moveDuration).SetEase(OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(OutQuad))
            .SetLink(gameObject);
    }
}
