// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/20/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SystemBehaviour.cs
// Summary: A manager for combo.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using MatchEnums;
using GemEnums;
using System;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] protected MatchEventChannel matchEvents;
    [SerializeField] protected CharacterStatus partyStatus;
    [SerializeField] protected float comboResetTime = 3f;

    public event Action<int, float> OnComboUpdated;

    public float ComboResetTime => comboResetTime;

    protected int _comboCount = 0;
    protected float _timer = 0f;


    private void OnEnable()
    {
        matchEvents.OnRaised += OnMatch;
    }

    private void OnDisable()
    {
        matchEvents.OnRaised -= OnMatch;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_comboCount > 0)
        {
            _timer -= GlobalTimeManager.DeltaTime;
            OnComboUpdated.Invoke(_comboCount, _timer);

            if (_timer <= 0f)
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
        _comboCount += (int)matchEvent.Tier;
        _timer = comboResetTime;
        OnComboUpdated.Invoke(_comboCount, _timer);

        // Increase critical hit chance per combo
        partyStatus.SetComboCritBonus(_comboCount / 100f);
    }

    public void ResetCombo()
    {
        _comboCount = 0;
        partyStatus.SetComboCritBonus(_comboCount);
        _timer = 0f;
        OnComboUpdated.Invoke(_comboCount, _timer);
    }
}
