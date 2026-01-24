// SPDX-License-Identifier: MIT
// Copyright (c) 11/11/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackBehaviour.cs
// Summary: A script for enemy combat behaviour.

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(StunRepresenter))]
public class EnemyAttackBehaviour : MonoBehaviour
{
    [SerializeField] protected EnemyAttackEventChannel attackChannel;
    [SerializeField] protected BoardDisableEventChannel boardDisableChannel;
    [SerializeField] protected BoardDisableLogic boardDisableLogic;
    [SerializeField] protected StunRepresenter stunRepresenter;
    [SerializeField, Min(0.001f)] protected float baseCooldown = 5f;

    protected float _currentCooldown;

    protected Coroutine _loop;
    protected float _attackTimer;
    public float Cooldown => _currentCooldown;

    public event Action<float, float> OnAttackTimerChanged;

    protected Coroutine _enrangeRoutine;
    protected float _enrageTimer;
    [SerializeField, Min(10f)] protected float _enrageDelay = 30f;

    protected bool _isStunned = false;
    protected Coroutine _stunRoutine = null;

    protected void OnEnable()
    {
        _loop = StartCoroutine(Loop());
        _enrangeRoutine = StartCoroutine(EnrageAfterDelay());
    }
    protected void OnDisable()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
        }
        if (_enrangeRoutine != null)
        {
            StopCoroutine(_enrangeRoutine);
        }
    }

    protected void Awake()
    {
        _currentCooldown = baseCooldown;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void DelayAttack(float delay)
    {
        UpdateAttackTimer(delay);
    }

    // Update is called once per frame
    protected IEnumerator Loop()
    {
        while (true)
        {
            _attackTimer = _currentCooldown;

            // Countdown
            while (_attackTimer > 0f)
            {
                if (!_isStunned)
                {
                    UpdateAttackTimer(-GlobalTimeManager.DeltaTime);
                }

                yield return null;
            }

            Attack();
        }
    }

    protected void Attack()
    {
        attackChannel.Raise();
        boardDisableChannel.Raise(boardDisableLogic);
    }

    protected float GetCooldown()
    {
        return _currentCooldown;
    }

    protected void UpdateAttackTimer(float value)
    {
        _attackTimer = Mathf.Clamp(_attackTimer + value, 0f, _currentCooldown);
        OnAttackTimerChanged?.Invoke(_attackTimer, _currentCooldown);
    }

    protected void Enrage()
    {
        _currentCooldown *= 0.25f;
        OnAttackTimerChanged?.Invoke(_attackTimer, _currentCooldown);
    }

    protected IEnumerator EnrageAfterDelay()
    {
        _enrageTimer = _enrageDelay;

        // Countdown
        while (_enrageTimer > 0f)
        {
            if (!_isStunned)
            {
                _enrageTimer -= GlobalTimeManager.DeltaTime;
            }

            yield return null;
        }

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

        stunRepresenter.Stun(duration);

        yield return GlobalTimeManager.WaitForGlobalSeconds(duration);

        _isStunned = false;
    }
}
