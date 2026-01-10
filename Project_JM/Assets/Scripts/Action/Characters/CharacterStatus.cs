// SPDX-License-Identifier: MIT
// Copyright (c) 11/10/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStatus.cs
// Summary: A class to modify status in runtime.

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
    [SerializeField] protected CharacterStatusData _baseData;
    [SerializeField] protected CharacterDeathEventChannel _deathEvent;

    public string CharacterName { get; }
    public float CurrentHP { get; private set; }
    public float maxHP { get; private set; }
    // 0 means 0%, 1 means 100%
    public float CriticalChance
    {
        get
        {
            float result = _baseData.baseCriticalChance + comboCritBonus;

            float buffBonus = buffCritChanceBonus;
            foreach (var m in _critChanceTimedModifiers)
            {
                buffBonus += m.Multiplier;
            }

            return result + buffBonus;
        }
    }
    // 1 means 100%, 1.5 means 150%
    public float CriticalDamage => _baseData.baseCriticalDamage + buffCritDamageBonus;

    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnShieldChanged;

    protected float _shield;
    public float Shield => _shield;
    public bool IsDead => CurrentHP <= 0f;

    protected float comboCritBonus = 0f;
    protected float buffCritChanceBonus = 0f;
    protected readonly List<TimedModifier> _critChanceTimedModifiers = new();

    protected float buffCritDamageBonus = 0f;

    protected void OnEnable()
    {
        _deathEvent.OnRaised += ClearBuffs;
    }

    protected void OnDisable()
    {
        _deathEvent.OnRaised -= ClearBuffs;
    }

    protected void Awake()
    {
        CurrentHP = _baseData.baseHP;
        maxHP = CurrentHP;
    }

    void Update()
    {
        for (int i = _critChanceTimedModifiers.Count - 1; i >= 0; i--)
        {
            if (_critChanceTimedModifiers[i].UpdateTimer(GlobalTimeManager.DeltaTime))
            {
                _critChanceTimedModifiers.RemoveAt(i);
            }
        }
    }

    public void Initialize(StatusMultiplier multiplier)
    {
        CurrentHP = CurrentHP / maxHP * _baseData.baseHP * multiplier.HPMultiplier;
        maxHP = _baseData.baseHP * multiplier.HPMultiplier;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    // It takes a range of [0, 1]. 1 means 100%
    public void Heal(float healPercentage)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + (maxHP * healPercentage));

        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    public void AddShield(float shieldPercentage)
    {
        _shield += Mathf.Max(0f, maxHP * shieldPercentage);
        OnShieldChanged?.Invoke(_shield, maxHP);
    }

    public void TakeDamage(float damage)
    {
        float calculatedDamage = Mathf.Max(0f, damage - _shield);
        _shield = 0f;
        OnShieldChanged?.Invoke(_shield, maxHP);

        // Spawn Damageui slightly above the origin of attacked target
        DamageUIManager.Instance.SpawnDamage(Mathf.RoundToInt(Mathf.Min(CurrentHP, calculatedDamage)), transform.position + Vector3.up * 1.5f);

        CurrentHP = Mathf.Max(0f, CurrentHP - calculatedDamage);

        OnHPChanged?.Invoke(CurrentHP, maxHP);

        if (IsDead)
        {
            Die();
        }
    }

    protected void Die()
    {
        _deathEvent.Raise(this);
        // Display death motion or anything about death. Instead of immediately deleting it.
        Destroy(gameObject);
    }

    public void SetComboCritBonus(float value)
    {
        comboCritBonus = value;
    }

    public void AddBuffCritBonus(float value)
    {
        buffCritChanceBonus += value;
    }

    public void AddBuffCritBonus(float value, float duration)
    {
        _critChanceTimedModifiers.Add(new TimedModifier(value, duration));
    }

    public void RemoveBuffCritBonus(float value)
    {
        buffCritChanceBonus -= value;
    }

    public void ClearBuffCritBonus()
    {
        buffCritChanceBonus = 0f;
        _critChanceTimedModifiers.Clear();
    }

    public bool IsCriticalHit()
    {
        return CriticalChance > GlobalRNG.Instance.NextFloat();
    }

    public void AddBuffCritDamage(float value)
    {
        buffCritDamageBonus += value;
    }

    public void ClearBuffCitDamageBonus()
    {
        buffCritDamageBonus = 0f;
    }

    public void ClearBuffs(CharacterStatus stat)
    {
        ClearBuffCritBonus();
        ClearBuffCitDamageBonus();
    }
}
