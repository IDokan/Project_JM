// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/18/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GameProgressManager.cs
// Summary: A class to manage whole game progress.
//          Handles and judges game data produced during game progress (e.g. enemy defeat count,
//          enrage state, party HP ratio) to evaluate save-worthy milestones.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TutorialEnums;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel deathChannel;
    [SerializeField] protected DifficultyCurvesSelector curvesSelector;
    [SerializeField] protected CharacterStatus partyStatus;

    protected int _numEnemyDefeated = 0;

    private TutorialProgress _progressAtRunStart;

    // Easy condition: none of the first 4 enemies were enraged when they died
    private bool _anyEnemyEnragedBeforeFourDefeats;

    // Medium condition: kill an enemy while party HP > 33% maxHP after 4 enemies defeated

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
        _progressAtRunStart = SaveDataManager.Instance.Progress;

        _anyEnemyEnragedBeforeFourDefeats = false;
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
        if (_numEnemyDefeated > 1)
        {
            return;
        }

        if (_progressAtRunStart == TutorialProgress.Hard)
        {
            SaveDataManager.Instance.ResetToMedium();
        }
        else if (_progressAtRunStart == TutorialProgress.Medium)
        {
            SaveDataManager.Instance.ResetToEasy();
        }
        // Challenge is permanent — no reset.
    }

    protected void HandleEnemyDied(CharacterStatus stat)
    {
        ++_numEnemyDefeated;
        partyStatus.Initialize(curvesSelector.ActiveCurves.GetAllyDifficultyMultiplier(_numEnemyDefeated));

        if (_progressAtRunStart == TutorialProgress.Easy)
        {
            if (_numEnemyDefeated <= 4
                && stat.TryGetComponent<EnemyAttackBehaviour>(out var behaviour)
                && behaviour.IsEnraged)
            {
                _anyEnemyEnragedBeforeFourDefeats = true;
            }

            if (_numEnemyDefeated == 4 && !_anyEnemyEnragedBeforeFourDefeats)
            {
                SaveDataManager.Instance.SetMedium();
            }
        }

        if (_progressAtRunStart == TutorialProgress.Medium
            && _numEnemyDefeated >= 4
            && partyStatus.CurrentHP > partyStatus.maxHP * 0.4f)
        {
            SaveDataManager.Instance.SetHard();
        }

        if (_progressAtRunStart == TutorialProgress.Hard && _numEnemyDefeated >= 4)
        {
            SaveDataManager.Instance.SetChallenge();
        }
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
        }
    }

}
