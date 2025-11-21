// SPDX-License-Identifier: MIT
// Copyright (c) 11/10/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStatus.cs
// Summary: A class to modify status in runtime.

using System;
using System.Collections;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField] protected CharacterStatusData _baseData;
    [SerializeField] protected CharacterDeathEventChannel _deathEvent;

    public string CharacterName { get; }
    public float CurrentHP { get; private set; }
    public float maxHP { get; private set; }
    public float CriticalChance => _baseData.baseCriticalChance + comboCritBonus + buffCritBonus;

    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnShieldChanged;

    protected float _shield;
    public float Shield => _shield;
    public bool IsDead => CurrentHP <= 0f;

    protected float comboCritBonus = 0f;
    protected float buffCritBonus = 0f;

    protected void Awake()
    {
        CurrentHP = _baseData.baseHP;
        maxHP = CurrentHP;
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

        Debug.Log($"{CharacterName} took {calculatedDamage} damage -> HP {CurrentHP}");

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
        buffCritBonus += value;
    }

    public void RemoveBuffCritBonus(float value)
    {
        buffCritBonus -= value;
    }

    public bool IsCriticalHit()
    {
        return CriticalChance > GlobalRNG.Instance.NextFloat() * 100;
    }
}
