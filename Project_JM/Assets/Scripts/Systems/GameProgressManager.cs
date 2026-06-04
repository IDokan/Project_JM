// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/18/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GameProgressManager.cs
// Summary: A class to manage whole game progress.
//          Handles and judges game data produced during game progress (e.g. enemy defeat count,
//          enrage state, board-disable resolution ratio) to evaluate save-worthy milestones.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using MatchEnums;
using TutorialEnums;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel deathChannel;
    [SerializeField] protected DifficultyCurvesSelector curvesSelector;
    [SerializeField] protected CharacterStatus partyStatus;

    [SerializeField] private BoardDisableEventChannel boardDisableChannel;
    [SerializeField] private MatchEventChannel matchChannel;
    [SerializeField] private SaveDataManager saveDataManager;

    protected int _numEnemyDefeated = 0;

    private TutorialProgress _progressAtRunStart;

    // Easy condition: none of the first 4 enemies were enraged when they died
    private bool _anyEnemyEnragedBeforeFourDefeats;

    // Medium condition: >=90% of board-disable windows resolved before the next disable attack
    private bool _pendingDisableOpportunity;
    private bool _noneMatchedBeforeNextAttack;
    private int _totalDisableOpportunities;
    private int _rapidlyResolvedCount;

    protected void Awake()
    {
        if (saveDataManager == null)
        {
            Debug.LogError("SaveDataManager is not assigned.", this);
            enabled = false;
        }
    }

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

        if (boardDisableChannel != null)
        {
            boardDisableChannel.OnRaised += OnBoardDisableEvent;
        }
        else
        {
            Debug.LogWarning("BoardDisableEventChannel is null", this);
        }

        if (matchChannel != null)
        {
            matchChannel.OnRaised += OnMatchEvent;
        }
        else
        {
            Debug.LogWarning("MatchEventChannel is null", this);
        }
    }

    protected void OnDisable()
    {
        deathChannel.OnRaised -= OnCharacterDied;

        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised -= OnTransitionEvent;
        }

        if (boardDisableChannel != null)
        {
            boardDisableChannel.OnRaised -= OnBoardDisableEvent;
        }

        if (matchChannel != null)
        {
            matchChannel.OnRaised -= OnMatchEvent;
        }
    }

    public void Clear()
    {
        _numEnemyDefeated = 0;
        _progressAtRunStart = saveDataManager.Progress;

        _anyEnemyEnragedBeforeFourDefeats = false;

        _pendingDisableOpportunity = false;
        _noneMatchedBeforeNextAttack = false;
        _totalDisableOpportunities = 0;
        _rapidlyResolvedCount = 0;
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
            saveDataManager.ResetToMedium();
        }
        else if (_progressAtRunStart == TutorialProgress.Medium)
        {
            saveDataManager.ResetToEasy();
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
                saveDataManager.SetMedium();
            }
        }

        // Close any open disable window when the enemy dies
        if (_progressAtRunStart == TutorialProgress.Medium && _pendingDisableOpportunity)
        {
            _totalDisableOpportunities++;
            if (_noneMatchedBeforeNextAttack)
            {
                _rapidlyResolvedCount++;
            }
            _pendingDisableOpportunity = false;
            CheckMediumCondition();
        }

        if (_progressAtRunStart == TutorialProgress.Hard && _numEnemyDefeated >= 4)
        {
            saveDataManager.SetChallenge();
        }
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
        }
    }

    private void OnBoardDisableEvent(BoardDisableEventContext context)
    {
        if (context.boardDisablePhase != BoardDisablePhase.Commit)
        {
            return;
        }

        if (_progressAtRunStart != TutorialProgress.Medium)
        {
            return;
        }

        // Close previous window before opening the next one
        if (_pendingDisableOpportunity)
        {
            _totalDisableOpportunities++;
            if (_noneMatchedBeforeNextAttack)
            {
                _rapidlyResolvedCount++;
            }
            CheckMediumCondition();
        }

        _pendingDisableOpportunity = true;
        _noneMatchedBeforeNextAttack = false;
    }

    private void OnMatchEvent(MatchEvent matchEvent)
    {
        if (_progressAtRunStart != TutorialProgress.Medium)
        {
            return;
        }

        if (_pendingDisableOpportunity && matchEvent.Color == GemColor.None)
        {
            _noneMatchedBeforeNextAttack = true;
        }
    }

    private void CheckMediumCondition()
    {
        if (_progressAtRunStart != TutorialProgress.Medium)
        {
            return;
        }

        if (_numEnemyDefeated < 4 || _totalDisableOpportunities == 0)
        {
            return;
        }

        if ((float)_rapidlyResolvedCount / _totalDisableOpportunities >= 0.75f)
        {
            saveDataManager.SetHard();
        }
    }
}
