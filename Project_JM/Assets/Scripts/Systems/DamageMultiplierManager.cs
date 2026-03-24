// SPDX-License-Identifier: MIT
// Copyright (c) 11/17/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageMultiplierManager.cs
// Summary: A class to manage damage multiplier.

using System.Collections.Generic;
using UnityEngine;

public class DamageMultiplierManager : MonoBehaviour
{
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected DifficultyCurves difficultyCurves;

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

    public float GetRawMultiplier
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
    }

    public void OnCharacterDiedHandle(CharacterStatus status)
    {

        if (status.TryGetComponent<EnemyTag>(out _))
        {
            OnEnemyKilled(_numEnemyDefeated + 1);
        }
    }

    protected void OnLevelTransition(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            OnEnemyKilled(0);
        }
    }

    protected void OnEnemyKilled(int newNumEnemyDefeated)
    {
        // Clear timed bonus added during combat
        _damageBonus = 1f;

        _numEnemyDefeated = newNumEnemyDefeated;

        _damageMultiplier = (difficultyCurves.DamageMultiplierCurve.Evaluate(_numEnemyDefeated));
    }
}
