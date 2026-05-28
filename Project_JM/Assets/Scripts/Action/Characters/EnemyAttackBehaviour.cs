// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/11/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackBehaviour.cs
// Summary: A script for enemy combat behaviour.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(StunRepresenter))]
public class EnemyAttackBehaviour : MonoBehaviour
{
    [SerializeField] protected EnemyAttackEventChannel attackChannel;
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;

    [Header("SFX")]
    [SerializeField] protected AudioCueSO enrageSfx;

    [SerializeField] protected StunRepresenter stunRepresenter;
    [SerializeField, Min(0.001f)] protected float baseCooldown = 5f;

    protected float _currentCooldown;

    protected Coroutine _loop;
    protected float _attackTimer;
    public float Cooldown => _currentCooldown;

    public event Action<float, float> OnAttackTimerChanged;

    protected Coroutine _enrangeRoutine;
    protected float _enrageTimer;
    [SerializeField, Min(10f)] protected float enrageDelay = 30f;
    public float EnrageDelay => enrageDelay;

    public event Action<float, float> OnEnrageTimeChanged;

    public event Action OnEnraged;
    public event Action OnStunBegin;
    public event Action OnStunEnd;
    public event Action OnDied;

    protected bool _isEnraged = false;
    protected bool _isStunned = false;
    protected bool _isAttacking = false;
    protected Coroutine _stunRoutine = null;

    public bool IsStunned => _isStunned;

    protected void OnEnable()
    {
        _loop = StartCoroutine(Loop());
        _enrangeRoutine = StartCoroutine(EnrageAfterDelay());

        characterDeathEventChannel.OnRaised += HandleDeath;
    }

    protected void OnDisable()
    {
        StopRoutines();

        characterDeathEventChannel.OnRaised -= HandleDeath;
    }

    protected void Awake()
    {
        _currentCooldown = baseCooldown;
    }

    public void DelayAttack(float delay)
    {
        UpdateAttackTimer(delay);
    }

    protected IEnumerator Loop()
    {
        while (true)
        {
            _attackTimer = _currentCooldown;

            while (_attackTimer > 0f)
            {
                if (!_isStunned && !_isAttacking)
                {
                    UpdateAttackTimer(-GlobalTimeManager.DeltaTime);
                }

                yield return null;
            }

            Attack();

            while (_isAttacking)
            {
                yield return null;
            }
        }
    }

    protected void Attack()
    {
        _isAttacking = true;
        attackChannel.Raise();
    }

    public void NotifyAttackFinished()
    {
        _isAttacking = false;
    }

    protected void UpdateAttackTimer(float value)
    {
        _attackTimer = Mathf.Clamp(_attackTimer + value, 0f, _currentCooldown);
        OnAttackTimerChanged?.Invoke(_attackTimer, _currentCooldown);
    }

    protected void Enrage()
    {
        _isEnraged = true;
        _currentCooldown *= 0.25f;
        OnAttackTimerChanged?.Invoke(_attackTimer, _currentCooldown);
        AudioManager.Instance.PlayEnemyActionSFX(enrageSfx);
        OnEnraged?.Invoke();
    }

    protected IEnumerator EnrageAfterDelay()
    {
        _enrageTimer = enrageDelay;
        OnEnrageTimeChanged?.Invoke(_enrageTimer, enrageDelay);

        while (_enrageTimer > 0f)
        {
            if (!_isStunned)
            {
                _enrageTimer -= GlobalTimeManager.DeltaTime;
                OnEnrageTimeChanged?.Invoke(_enrageTimer, enrageDelay);
            }

            yield return null;
        }

        OnEnrageTimeChanged?.Invoke(0, enrageDelay);
        Enrage();
    }

    public void Stun(float duration)
    {
        if (_stunRoutine != null)
        {
            StopCoroutine(_stunRoutine);
            _stunRoutine = null;
        }
        _stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    protected IEnumerator StunRoutine(float duration)
    {
        _isStunned = true;
        _isAttacking = false;
        OnStunBegin?.Invoke();

        stunRepresenter.Stun(duration);

        yield return GlobalTimeManager.WaitForGlobalSeconds(duration);

        _isStunned = false;
        OnStunEnd?.Invoke();
    }

    void StopRoutines()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
        if (_enrangeRoutine != null)
        {
            StopCoroutine(_enrangeRoutine);
            _enrangeRoutine = null;
        }
        if (_stunRoutine != null)
        {
            StopCoroutine(_stunRoutine);
            _stunRoutine = null;
        }
    }

    protected void HandleDeath(CharacterStatus stat)
    {
        StopRoutines();

        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            OnDied?.Invoke();
        }
    }
}
