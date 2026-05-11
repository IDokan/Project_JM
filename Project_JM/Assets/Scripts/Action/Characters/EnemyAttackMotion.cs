// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/24/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackMotion.cs
// Summary: Universal enemy animation controller — drives Animator and delegates state visuals to EnemyStateVisual.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using System;
using UnityEngine;

public class EnemyAttackMotion : MonoBehaviour
{
    protected Animator _animator;

    public event Action OnHit;

    [Header("Wiring")]
    [SerializeField] protected EnemyAttackBehaviour enemyAttackBehaviour;
    [SerializeField] protected EnemyStateVisual stateVisual;
    [SerializeField] private TransitionEventChannel transitionEventChannel;

    [Header("SFX")]
    [SerializeField] protected AudioCueSO swingSfx;
    [SerializeField] protected AudioCueSO impactSfx;

    [Header("Damaged Motion")]
    [SerializeField] protected float moveDuration = 0.1f;
    [SerializeField] protected float pauseDuration = 0.2f;

    protected static readonly int DamagedTrig = Animator.StringToHash("DamagedTrig");
    protected static readonly int AttackTrig = Animator.StringToHash("AttackTrig");
    protected static readonly int DeadTrig = Animator.StringToHash("DeadTrig");
    protected static readonly int WalkTrig = Animator.StringToHash("WalkTrig");

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
        enemyAttackBehaviour.OnEnraged += OnEnraged;
        enemyAttackBehaviour.OnStunBegin += OnStunBegin;
        enemyAttackBehaviour.OnStunEnd += OnStunEnd;
        enemyAttackBehaviour.OnDied += OnDied;
        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised += OnTransitionPhase;
        }
    }

    protected void OnDisable()
    {
        GlobalTimeManager.OnScaleChanged -= ApplyGlobalTweenScale;
        enemyAttackBehaviour.OnEnraged -= OnEnraged;
        enemyAttackBehaviour.OnStunBegin -= OnStunBegin;
        enemyAttackBehaviour.OnStunEnd -= OnStunEnd;
        enemyAttackBehaviour.OnDied -= OnDied;
        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised -= OnTransitionPhase;
        }
    }

    public void PlayAttackMotion(Vector3 moveOffset)
    {
        if (!_animator)
        {
            return;
        }

        _attackDone = false;

        AudioManager.Instance.PlayEnemyActionSFX(swingSfx);
        _animator.ResetTrigger(AttackTrig);
        _animator.SetTrigger(AttackTrig);
        stateVisual?.OnAttack(moveOffset);
    }

    public void PlayDamagedMotion(Vector3 moveOffset)
    {
        if (_animator == null || _attackDone == false)
        {
            return;
        }

        Move(moveOffset, null, null);

        _animator.ResetTrigger(DamagedTrig);
        _animator.SetTrigger(DamagedTrig);
    }

    protected void Move(Vector3 offset, Action onReachedTarget, Action onSequenceComplete)
    {
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        Vector3 target = _originalPosition + offset;

        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendCallback(() => onReachedTarget?.Invoke())
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .OnComplete(() => onSequenceComplete?.Invoke())
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

    // Called via Animation Event at the frame the attack connects.
    public void AnimEvent_RaiseHit()
    {
        AudioManager.Instance.PlayEnemyActionSFX(impactSfx);
        OnHit?.Invoke();
    }

    // Called via Animation Event at the last frame of the attack clip.
    public void AnimEvent_RaiseAttackEnd()
    {
        _attackDone = true;
    }

    private void OnEnraged()
    {
        stateVisual?.OnEnraged();
    }

    private void OnStunBegin()
    {
        stateVisual?.OnStunBegin();
    }

    private void OnStunEnd()
    {
        stateVisual?.OnStunEnd();
    }

    private void OnDied()
    {
        if (_moveSequence != null && _moveSequence.IsActive())
        {
            _moveSequence.Kill();
        }

        _animator.SetTrigger(DeadTrig);
        stateVisual?.OnDied();
    }

    private void OnTransitionPhase(TransitionPhase phase)
    {
        if (phase == TransitionPhase.EndEnemyMoveBegin)
        {
            _animator.SetTrigger(WalkTrig);
            stateVisual?.OnWin();
        }
    }
}
