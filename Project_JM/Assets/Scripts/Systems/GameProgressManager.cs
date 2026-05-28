// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/18/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GameProgressManager.cs
// Summary: A class to manage whole game progress.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel deathChannel;
    [SerializeField] protected DifficultyCurves curves;
    [SerializeField] protected CharacterStatus partyStatus;

    protected int _numEnemyDefeated = 0;

    protected void OnEnable()
    {
        if (deathChannel != null)
        {
            deathChannel.OnRaised += OnCharacterDied;
        }
        else
        {
            Debug.LogWarning("CharacterDeathEventChannel is null", this);
        }

        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised += OnTransitionEvent;
        }
        else
        {
            Debug.LogWarning("TransitionEventChannel is null", this);
        }
    }

    protected void OnDisable()
    {
        deathChannel.OnRaised -= OnCharacterDied;

        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised -= OnTransitionEvent;
        }
    }

    public void Clear()
    {
        _numEnemyDefeated = 0;
    }

    protected void OnCharacterDied(CharacterStatus stat)
    {
        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            HandleEnemyDied(stat);
        }
        else if (stat.TryGetComponent<AllyTag>(out _))
        {
            HandleAllyDied(stat);
        }
        else
        {
            // CharacterStat must have at least one Tag class.
        }
    }

    protected void HandleAllyDied(CharacterStatus stat)
    {
        // Need to handle 
    }

    protected void HandleEnemyDied(CharacterStatus stat)
    {
        ++_numEnemyDefeated;
        partyStatus.Initialize(curves.GetAllyDifficultyMultiplier(_numEnemyDefeated));
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
        }
    }
}
