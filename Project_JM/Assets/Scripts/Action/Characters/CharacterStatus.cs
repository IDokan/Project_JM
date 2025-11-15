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
    public float DamageMultiplier => _baseData.baseDamageMultiplier;
    public float CriticalChance => _baseData.baseCriticalChance;

    public event Action<float, float> OnHPChanged;


    public bool IsDead => CurrentHP <= 0f;

    protected void Awake()
    {
        CurrentHP = _baseData.baseHP;
        maxHP = CurrentHP;
    }

    public void TakeDamage(float damage)
    {
        CurrentHP = Mathf.Max(0f, CurrentHP - damage);

        OnHPChanged?.Invoke(CurrentHP, _baseData.baseHP);

        Debug.Log($"{CharacterName} took {damage} damage -> HP {CurrentHP}");

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
}
