// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterCombatant.cs
// Summary: A combatant script for action characters.

using GemEnums;
using UnityEngine;

public interface ICombatant
{
    GemColor[] Colors { get; }
    void Heal(float healPercentage);
    void AddShield(float shieldPercentage);
    void TakeDamage(float damage, AttackContext attackContext);
    void AddBuffCritBonus(float value);
    void AddBuffCritBonus(float value, float duration);
    void AddBuffCritDamage(float value);
}

public class CharacterCombatant : MonoBehaviour, ICombatant
{
    [SerializeField] protected CharacterStatus _status;
    [SerializeField] protected GemColor[] _colors;

    public CharacterStatus Status => _status;
    public GemColor[] Colors => _colors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Heal(float healPercentage)
    {
        _status.Heal(healPercentage);
    }

    public void AddShield(float shieldPercentage)
    {
        _status.AddShield(shieldPercentage);
    }

    public void TakeDamage(float rawDamage, AttackContext attackContext)
    {
        float damage = rawDamage * attackContext.DamageMultiplierManager.GetMultiplier;

        // Critical hit calculation
        if (attackContext.Attacker is CharacterCombatant attackerObject)
        {
            if (attackerObject.Status.IsCriticalHit())
            { 
                damage *= attackerObject.Status.CriticalDamage;
            }
        }

        damage *= GemColorUtility.GetGemColorDamageMultiplier(attackContext.Attacker.Colors, attackContext.Target.Colors);

        _status.TakeDamage(damage);
    }

    public void AddBuffCritBonus(float value)
    {
        _status.AddBuffCritBonus(value);
    }

    public void AddBuffCritBonus(float value, float duration)
    {
        _status.AddBuffCritBonus(value, duration);
    }

    public void AddBuffCritDamage(float value)
    {
        _status.AddBuffCritDamage(value);
    }
}
