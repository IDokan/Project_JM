// SPDX-License-Identifier: MIT
// Copyright (c) 11/11/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackBehaviour.cs
// Summary: A script for enemy combat behaviour.

using System;
using System.Collections;
using UnityEngine;

public class EnemyAttackBehaviour : MonoBehaviour
{
    [SerializeField] protected EnemyAttackEventChannel _attackChannel;
    [SerializeField] protected BoardDisableEventChannel _boardDisableChannel;
    [SerializeField] protected BoardDisableLogic _boardDisableLogic;
    [SerializeField, Min(0.001f)] protected float _cooldown;

    protected Coroutine _loop;
    protected float _attackTimer;
    public float Cooldown => _cooldown;

    public event Action<float, float> OnAttackTimerChanged;

    protected Coroutine _enrangeRoutine;
    protected float _enrageTimer;
    [SerializeField, Min(10f)] protected float _enrageDelay = 30f;

    protected bool _isStunned = false;

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
        _cooldown = GetCooldown();
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
            _attackTimer = _cooldown;

            // Countdown
            while (_attackTimer > 0f)
            {
                if (!_isStunned)
                {
                    if (_attackTimer >= _cooldown)
                    {
                        _attackTimer = _cooldown;
                    }

                    UpdateAttackTimer(-GlobalTimeManager.DeltaTime);
                }

                yield return null;
            }

            Attack();
        }
    }

    protected void Attack()
    {
        _attackChannel.Raise();
        _boardDisableChannel.Raise(_boardDisableLogic);
    }

    protected float GetCooldown()
    {
        return _cooldown;
    }

    protected void UpdateAttackTimer(float value)
    {
        _attackTimer += value;
        OnAttackTimerChanged?.Invoke(_attackTimer, _cooldown);
    }

    protected void Enrage()
    {
        _cooldown *= 0.25f;
        OnAttackTimerChanged?.Invoke(_attackTimer, _cooldown);
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
        StartCoroutine(StunRoutine(duration));
    }

    protected IEnumerator StunRoutine(float duration)
    {
        _isStunned = true;

        yield return new WaitForSeconds(duration);

        _isStunned = false;
    }
}
