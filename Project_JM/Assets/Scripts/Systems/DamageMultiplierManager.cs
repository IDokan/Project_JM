// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/17/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageMultiplierManager.cs
// Summary: A class to manage damage multiplier.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
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
        }
    }

    protected void OnEnemyKilled(int newNumEnemyDefeated)
    {
        _numEnemyDefeated = newNumEnemyDefeated;

        _damageMultiplier = curvesSelector.ActiveCurves.GetLevelMultiplier(_numEnemyDefeated);
    }
}
