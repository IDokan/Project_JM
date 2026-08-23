// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/17/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageMultiplierManager.cs
// Summary: A class to manage damage multiplier.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using GemEnums;
using UnityEngine;

public class DamageMultiplierManager : MonoBehaviour
{
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected DifficultyCurvesSelector curvesSelector;
    [SerializeField] protected DamageBonusChangedEventChannel damageBonusChangedEventChannel;

    protected float _damageMultiplier = 1f;
    protected int _numEnemyDefeated = 0;
    protected float _damageBonus = 1f;

    // Permanent per-color attack power bonus granted by rewards (e.g. PowerUp).
    // Only ever consulted for ally attackers; see CharacterCombatant.TakeDamage.
    protected readonly Dictionary<GemColor, float> _rewardAttackPowerBonus = new Dictionary<GemColor, float>();

    // Permanent per-color critical damage bonus granted by rewards (e.g. SharpAttack).
    // Only ever consulted for ally attackers; see CharacterCombatant.TakeDamage.
    protected readonly Dictionary<GemColor, float> _rewardCriticalDamageBonus = new Dictionary<GemColor, float>();

    public float GetMultiplier => _damageMultiplier * _damageBonus;

    protected void OnEnable()
    {
        characterDeathEventChannel.OnRaised += OnCharacterDiedHandle;
        transitionEventChannel.OnRaised += OnLevelTransition;
    }
    protected void OnDisable()
    {
        characterDeathEventChannel.OnRaised -= OnCharacterDiedHandle;
        transitionEventChannel.OnRaised -= OnLevelTransition;
    }

    public float GetEnemyMultiplier
    {
        get { return _damageMultiplier; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void AddTimedBonus(float multiplier)
    {
        _damageBonus *= multiplier;
        damageBonusChangedEventChannel.Raise(_damageBonus);
    }

    protected void ClearTimedBonus()
    {
        _damageBonus = 1f;
        damageBonusChangedEventChannel.Raise(_damageBonus);
    }

    public float GetRewardAttackPowerMultiplier(GemColor color)
    {
        return _rewardAttackPowerBonus.TryGetValue(color, out float bonus) ? 1f + bonus : 1f;
    }

    public void AddRewardAttackPowerBonus(GemColor color, float bonus)
    {
        _rewardAttackPowerBonus.TryGetValue(color, out float current);
        _rewardAttackPowerBonus[color] = current + bonus;
    }

    public float GetRewardCriticalDamageMultiplier(GemColor color)
    {
        return _rewardCriticalDamageBonus.TryGetValue(color, out float bonus) ? 1f + bonus : 1f;
    }

    public void AddRewardCriticalDamageBonus(GemColor color, float bonus)
    {
        _rewardCriticalDamageBonus.TryGetValue(color, out float current);
        _rewardCriticalDamageBonus[color] = current + bonus;
    }

    public void OnCharacterDiedHandle(CharacterStatus status)
    {
        ClearTimedBonus();

        if (status.TryGetComponent<EnemyTag>(out _))
        {
            OnEnemyKilled(_numEnemyDefeated + 1);
        }
    }

    protected void OnLevelTransition(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            ClearTimedBonus();
            OnEnemyKilled(0);
            _rewardAttackPowerBonus.Clear();
            _rewardCriticalDamageBonus.Clear();
        }
    }

    protected void OnEnemyKilled(int newNumEnemyDefeated)
    {
        _numEnemyDefeated = newNumEnemyDefeated;

        _damageMultiplier = curvesSelector.ActiveCurves.GetLevelMultiplier(_numEnemyDefeated);
    }
}
