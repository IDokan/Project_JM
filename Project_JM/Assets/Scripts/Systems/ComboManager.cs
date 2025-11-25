// SPDX-License-Identifier: MIT
// Copyright (c) 11/20/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SystemBehaviour.cs
// Summary: A manager for combo.

using MatchEnums;
using GemEnums;
using System;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] protected CharacterStatus _partyStatus;
    [SerializeField] protected float _comboResetTime = 3f;

    public event Action<int, float> OnComboUpdated;

    public float ComboResetTime => _comboResetTime;

    protected int comboCount = 0;
    protected float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (comboCount > 0)
        {
            timer -= GlobalTimeManager.DeltaTime;
            OnComboUpdated.Invoke(comboCount, timer);

            if (timer <= 0f)
            {
                ResetCombo();
            }
        }
    }
    
    public void OnMatch(MatchEvent matchEvent)
    {
        // Pass if no valid color
        if (matchEvent.Color == GemColor.None)
        {
            return;
        }

        // On Match, increase combo for a duration.
        comboCount += (int)matchEvent.Tier;
        timer = _comboResetTime;
        OnComboUpdated.Invoke(comboCount, timer);

        // Increase critical hit chance per combo
        _partyStatus.SetComboCritBonus(comboCount / 100f);
    }

    public void ResetCombo()
    {
        comboCount = 0;
        _partyStatus.SetComboCritBonus(comboCount);
        timer = 0f;
        OnComboUpdated.Invoke(comboCount, timer);
    }
}
