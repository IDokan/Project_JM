// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/10/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStatus.cs
// Summary: A class to modify status in runtime.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedModifier
{
    public float Multiplier { get; private set; }
    protected float _timeRemaining;

    public TimedModifier(float multiplier, float duration)
    {
        Multiplier = multiplier;
        _timeRemaining = duration;
    }

    public bool UpdateTimer(float deltaTime)
    {
        _timeRemaining -= deltaTime;
        return _timeRemaining <= 0f;
    }
}

public class CharacterStatus : MonoBehaviour
{
    [SerializeField] protected CharacterStatusData baseData;
    [SerializeField] protected CharacterDeathEventChannel deathEvent;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    public string CharacterName { get; }
    public float CurrentHP { get; private set; }
    public float maxHP { get; private set; }
    // 0 means 0%, 1 means 100%
    public float CriticalChance
    {
        get
        {
            float result = baseData.baseCriticalChance + _comboCritBonus;

            float buffBonus = _buffCritChanceBonus;
            foreach (var m in _critChanceTimedModifiers)
            {
                buffBonus += m.Multiplier;
            }

            // Clamped to [0, 1] to prevent crit chance UI from displaying values over 100%
            return Mathf.Clamp01(result + buffBonus);
        }
    }
    // 1 means 100%, 1.5 means 150%
    public float CriticalDamage => baseData.baseCriticalDamage + _buffCritDamageBonus;

    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnShieldChanged;
    public event Action<float> OnCritChanceChanged;
    public event Action OnCriticalHit;
    public event Action OnCritTimedBuffStarted;
    public event Action OnCritTimedBuffStopped;

    protected float _shield;
    public float Shield => _shield;
    public bool IsDead => CurrentHP <= 0f;

    protected float _comboCritBonus = 0f;
    protected float _buffCritChanceBonus = 0f;
    protected readonly List<TimedModifier> _critChanceTimedModifiers = new();

    protected float _buffCritDamageBonus = 0f;

    protected void OnEnable()
    {
        if (deathEvent != null)
        {
            deathEvent.OnRaised += ClearBuffs;
        }
        else
        {
            Debug.LogWarning("CharacterDeathEventChannel is null", this);
        }

        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised += OnTransitionEvent;
        }
        else
        {
            Debug.LogWarning("TransitionEventChannel is null", this);
        }
    }

    protected void OnDisable()
    {
        deathEvent.OnRaised -= ClearBuffs;
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void Awake()
    {
        Clear();
    }

    void Update()
    {
        bool anyExpired = false;
        for (int i = _critChanceTimedModifiers.Count - 1; i >= 0; i--)
        {
            if (_critChanceTimedModifiers[i].UpdateTimer(GlobalTimeManager.DeltaTime))
            {
                _critChanceTimedModifiers.RemoveAt(i);
                anyExpired = true;
            }
        }

        if (anyExpired)
        {
            RaiseCritChanceChanged();
            if (_critChanceTimedModifiers.Count == 0)
            {
                OnCritTimedBuffStopped?.Invoke();
            }
        }
    }

    public void Initialize(StatusMultiplier multiplier)
    {
        CurrentHP = CurrentHP / maxHP * baseData.baseHP * multiplier.HPMultiplier;
        maxHP = baseData.baseHP * multiplier.HPMultiplier;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    // It takes a range of [0, 1]. 1 means 100%
    public void Heal(float healPercentage)
    {
        if (IsDead)
        {
            return;
        }

        CurrentHP = Mathf.Min(maxHP, CurrentHP + (maxHP * healPercentage));

        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    public void AddShield(float shieldPercentage)
    {
        if (IsDead)
        {
            return;
        }

        _shield += Mathf.Max(0f, maxHP * shieldPercentage);
        OnShieldChanged?.Invoke(_shield, maxHP);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        float calculatedDamage = Mathf.Max(0f, damage - _shield);
        _shield = 0f;
        OnShieldChanged?.Invoke(_shield, maxHP);

        CurrentHP = Mathf.Max(0f, CurrentHP - calculatedDamage);

        OnHPChanged?.Invoke(CurrentHP, maxHP);

        if (IsDead)
        {
            Die();
        }
    }

    protected void Die()
    {
        deathEvent.Raise(this);
    }

    public void SetComboCritBonus(float value)
    {
        _comboCritBonus = value;
        RaiseCritChanceChanged();
    }

    public void AddBuffCritBonus(float value)
    {
        _buffCritChanceBonus += value;
        RaiseCritChanceChanged();
    }

    public void AddBuffCritBonus(float value, float duration)
    {
        if (_critChanceTimedModifiers.Count == 0)
        {
            OnCritTimedBuffStarted?.Invoke();
        }
        _critChanceTimedModifiers.Add(new TimedModifier(value, duration));
        RaiseCritChanceChanged();
    }

    public void RemoveBuffCritBonus(float value)
    {
        _buffCritChanceBonus -= value;
        RaiseCritChanceChanged();
    }

    public void ClearBuffCritBonus()
    {
        _buffCritChanceBonus = 0f;
        bool hadTimedModifiers = _critChanceTimedModifiers.Count > 0;
        _critChanceTimedModifiers.Clear();
        if (hadTimedModifiers)
        {
            OnCritTimedBuffStopped?.Invoke();
        }
        RaiseCritChanceChanged();
    }

    private void RaiseCritChanceChanged() => OnCritChanceChanged?.Invoke(CriticalChance);

    public bool IsCriticalHit()
    {
        bool isCrit = CriticalChance > GlobalRNG.Instance.NextFloat();
        if (isCrit)
        {
            OnCriticalHit?.Invoke();
        }
        return isCrit;
    }

    public void AddBuffCritDamage(float value)
    {
        _buffCritDamageBonus += value;
    }

    public void ClearBuffCritDamageBonus()
    {
        _buffCritDamageBonus = 0f;
    }

    public void ClearBuffs(CharacterStatus stat)
    {
        ClearBuffCritBonus();
        ClearBuffCritDamageBonus();
    }

    protected void Clear()
    {
        CurrentHP = baseData.baseHP;
        maxHP = CurrentHP;

        OnHPChanged?.Invoke(CurrentHP, maxHP);

        _shield = 0f;
        OnShieldChanged?.Invoke(_shield, maxHP);
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (IsEnemyDestroyConditionOnTransitionEvent(phase))
        {
            Destroy(gameObject);
        }
        else if (phase == TransitionPhase.IntroTransitionBegin)
        {   // Clear level-up effects to the party object
            Clear();
        }
    }

    protected bool IsEnemyDestroyConditionOnTransitionEvent(TransitionPhase phase)
    {
        bool isEnemy = TryGetComponent<EnemyTag>(out _);

        if (!isEnemy)
        {   // Do not destroy party roster object
            return false;
        }

        return 
            (phase == TransitionPhase.EndEnemyMoveEnd)
            ||
            (phase == TransitionPhase.MiddleTransitionEnd && IsDead)
            || 
            (phase == TransitionPhase.IntroTransitionBegin)
            ;
    }
}
