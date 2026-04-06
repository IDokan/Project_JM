// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/24/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackMotion.cs
// Summary: A script for enemy attack motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using System;
using UnityEngine;

public class EnemyAttackMotion : MonoBehaviour
{
    protected Animator _animator;

    public event Action OnHit;

    [Header("Wiring")]
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;

    // The durations can be changed in near future
    //      to display different pause for different attack logics.
    [SerializeField] protected float moveDuration = 0.1f;
    [SerializeField] protected float pauseDuration = 0.2f;

    protected static readonly int DamagedTrig = Animator.StringToHash("DamagedTrig");
    protected static readonly int AttackTrig = Animator.StringToHash("AttackTrig");
    protected static readonly int TierInt = Animator.StringToHash("TierInt");

    protected Vector3 _originalPosition;
    protected Sequence _moveSequence;

    protected float _timeScaler = 1f;


    protected bool _attackDone = true;

    protected void Awake()
    {
        _originalPosition = transform.localPosition;

        if (!_animator)
        {
            _animator = GetComponent<Animator>();
        }
    }

    protected void OnEnable()
    {
        GlobalTimeManager.OnScaleChanged += ApplyGlobalTweenScale;
        characterDeathEventChannel.OnRaised += OnCharacterDied;
    }

    protected void OnDisable()
    {
        GlobalTimeManager.OnScaleChanged -= ApplyGlobalTweenScale;
        characterDeathEventChannel.OnRaised -= OnCharacterDied;
    }

    public void PlayAttackMotion(Vector3 moveOffset)
    {
        if (!_animator)
        {
            return;
        }

        _attackDone = false;

        Move(moveOffset, moveDuration, pauseDuration, RaiseHit, RaiseAttackEnd);

        _animator.ResetTrigger(AttackTrig);
        _animator.SetTrigger(AttackTrig);
    }

    public void PlayDamagedMotion(Vector3 moveOffset)
    {
        if (_animator == null || _attackDone == false)
        {
            return;
        }

        Move(moveOffset, moveDuration, pauseDuration, null, null);

        _animator.ResetTrigger(DamagedTrig);
        _animator.SetTrigger(DamagedTrig);
    }

    protected void Move(Vector3 offset, float moveDuration, float pauseDuration, Action onReachedTarget, Action onSequenceComplete)
    {
        // If previous sequence has not finished yet, kill it.
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        Vector3 target = _originalPosition + offset;

        // Call OnHit.Invoke() after the first append ended.
        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendCallback(() => onReachedTarget?.Invoke())
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .OnComplete(()=>onSequenceComplete?.Invoke())
            .SetLink(gameObject);

        _moveSequence.timeScale = _timeScaler;
    }

    protected void ApplyGlobalTweenScale(float scale)
    {
        _timeScaler = scale;

        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.timeScale = scale;
        }
    }

    public void RaiseHit()
    {
        OnHit?.Invoke();
    }

    public void RaiseAttackEnd()
    {
        _attackDone = true;
    }


    protected void OnCharacterDied(CharacterStatus characterStat)
    {
        if (characterStat.TryGetComponent<EnemyTag>(out _))
        {
            if (_moveSequence != null && _moveSequence.IsActive())
            {
                _moveSequence.Kill();
            }
        }
    }
}
