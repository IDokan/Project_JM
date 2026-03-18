// SPDX-License-Identifier: MIT
// Copyright (c) 11/18/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GameProgressManager.cs
// Summary: A class to manage whole game progress.

using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel _deathChannel;
    [SerializeField] protected DifficultyCurves _curves;
    [SerializeField] protected CharacterStatus _partyStatus;

    protected int numEnemyDefeated = 0;

    protected void OnEnable()
    {
        if (_deathChannel != null)
        {
            _deathChannel.OnRaised += OnCharacterDied;
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
        _deathChannel.OnRaised -= OnCharacterDied;

        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised -= OnTransitionEvent;
        }
    }

    public void Clear()
    {
        numEnemyDefeated = 0;
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
        ++numEnemyDefeated;
        _partyStatus.Initialize(_curves.GetDifficultyMultiplier(numEnemyDefeated));
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
        }
    }
}
