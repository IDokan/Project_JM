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
    [SerializeField] protected AttackLogic _attackLogic;
    [SerializeField] protected BoardDisableEventChannel _boardDisableChannel;
    [SerializeField] protected BoardDisableLogic _boardDisableLogic;
    [SerializeField, Min(0.001f)] protected float _fallbackCooldown = 5f;

    protected Coroutine _loop;
    protected float _timer;
    protected float _cooldown { get; private set; }
    public float Cooldown => _cooldown;

    public event Action<float, float> OnAttackTimerChanged;

    protected Coroutine _enrangeRoutine;
    [SerializeField, Min(10f)] protected float _enrageDelay = 30f;

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
        UpdateTimer(delay);
    }

    // Update is called once per frame
    protected IEnumerator Loop()
    {
        while (true)
        {
            _timer = _cooldown;

            // Countdown
            while (_timer > 0f)
            {
                if (_timer >= _cooldown)
                {
                    _timer = _cooldown;
                }

                UpdateTimer(-Time.deltaTime);

                yield return null;
            }

            Attack();
        }
    }

    protected void Attack()
    {
        _attackChannel.Raise(_attackLogic);
        _boardDisableChannel.Raise(_boardDisableLogic);
    }

    protected float GetCooldown()
    {
        if (_attackLogic is IHasCooldown c)
        {
            return c.Cooldown;
        }

        return _fallbackCooldown;
    }

    protected void UpdateTimer(float value)
    {
        _timer += value;
        OnAttackTimerChanged?.Invoke(_timer, _cooldown);
    }

    protected void Enrange()
    {
        _cooldown *= 0.25f;
        OnAttackTimerChanged?.Invoke(_timer, _cooldown);
    }

    protected IEnumerator EnrageAfterDelay()
    {
        yield return new WaitForSeconds(_enrageDelay);

        Enrange();
    }
}
