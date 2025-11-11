// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterCombatant.cs
// Summary: A combatant script for action characters.

using UnityEngine;

public interface ICombatant
{
    void TakeDamage(float damage);
}

public class CharacterCombatant : MonoBehaviour, ICombatant
{
    [SerializeField] protected CharacterStatus _status;

    public CharacterStatus Status => _status;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float rawDamage)
    {
        float damage = rawDamage * _status.DamageMultiplier;

        // Critical hit calculation
        if(_status.CriticalChance > GlobalRNG.Instance.NextFloat() * 100)
        {
            damage *= 2;
        }

        _status.TakeDamage(damage);
    }

}
