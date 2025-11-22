// SPDX-License-Identifier: MIT
// Copyright (c) 11/18/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GameProgressManager.cs
// Summary: A class to manage whole game progress.

using UnityEngine;

public class GameProgressManager : MonoBehaviour
{

    [SerializeField] protected CharacterDeathEventChannel _deathChannel;
    [SerializeField] protected DifficultyCurves _curves;
    [SerializeField] protected CharacterStatus _partyStatus;

    protected int numEnemyDefeated = 0;

    protected void OnEnable()
    {
        _deathChannel.OnRaised += OnCharacterDied;
    }

    protected void OnDisable()
    {
        _deathChannel.OnRaised -= OnCharacterDied;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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
}
