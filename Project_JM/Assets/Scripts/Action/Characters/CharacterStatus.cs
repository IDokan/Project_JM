// SPDX-License-Identifier: MIT
// Copyright (c) 11/10/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStatus.cs
// Summary: A class to modify status in runtime.

using System.Collections;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField] protected CharacterStatusData _baseData;

    public string CharacterName { get; }
    public float CurrentHP { get; private set; }
    public float DamageMultiplier => _baseData.baseDamageMultiplier;
    public float CriticalChance => _baseData.baseCriticalChance;


    public bool IsDead => CurrentHP <= 0f;

    protected void Awake()
    {
        CurrentHP = _baseData.baseHP;
    }

    public void TakeDamage(float damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        Debug.Log($"{CharacterName} took {damage} damage -> HP {CurrentHP}");
    }
}
