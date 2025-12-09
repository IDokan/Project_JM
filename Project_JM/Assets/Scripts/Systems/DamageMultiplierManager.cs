// SPDX-License-Identifier: MIT
// Copyright (c) 11/17/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageMultiplierManager.cs
// Summary: A class to manage damage multiplier.

using System.Collections.Generic;
using UnityEngine;

public class DamageMultiplierManager : MonoBehaviour
{
    [SerializeField] CharacterDeathEventChannel _characterDeathEventChannel;
    [SerializeField] DifficultyCurves _difficultyCurves;

    protected float DamageMultiplier = 1f;
    protected int numEnemyDefeated = 0;
    protected float damageBonus = 1f;

    public float GetMultiplier => DamageMultiplier * damageBonus;

    protected void OnEnable() => _characterDeathEventChannel.OnRaised += OnCharacterDiedHandle;
    protected void OnDisable() => _characterDeathEventChannel.OnRaised -= OnCharacterDiedHandle;

    public float GetRawMultiplier
    {
        get { return DamageMultiplier; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void AddTimedBonus(float multiplier)
    {
        damageBonus *= multiplier;
    }

    public void OnCharacterDiedHandle(CharacterStatus status)
    {
        // Clear damage bonus affeted by buffs whenever characters defeated.
        damageBonus = 1f;

        if (status.TryGetComponent<EnemyTag>(out _))
        {
            ++numEnemyDefeated;

            DamageMultiplier = (_difficultyCurves.DamageMultiplierCurve.Evaluate(numEnemyDefeated));
        }
    }
}
