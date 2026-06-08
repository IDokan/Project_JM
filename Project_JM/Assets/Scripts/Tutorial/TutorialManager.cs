// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialManager.cs
// Summary: Orchestrates tutorial sequences: checks completion state, drives steps, and releases board control when done.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using MatchEnums;
using TutorialEnums;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialSequenceData> sequences;
    [SerializeField] private SaveDataManager saveDataManager;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private TutorialOverlayUI overlayUI;
    [SerializeField] private TransitionManager transitionManager;
    [SerializeField] private TutorialBoardHighlighter boardHighlighter;
    [SerializeField] private GlobalTimeManager globalTimeManager;
    [SerializeField] private MatchEventChannel matchEventChannel;
    [SerializeField] private EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] private EnemyAttackEventChannel enemyAttackEventChannel;
    [SerializeField] private CharacterStatus partyStatus;

    private bool _matchReceived;
    private TutorialSequenceData _pendingSequence;
    private EnemyAttackBehaviour _enemyAttackBehaviour;

    protected void OnEnable()
    {
        matchEventChannel.OnRaised += OnMatchRaised;
        enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
    }

    protected void OnDisable()
    {
        matchEventChannel.OnRaised -= OnMatchRaised;
        enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
    }

    private void Start()
    {
        TutorialProgress progress = saveDataManager.Progress;

        if (saveDataManager.IsTutorialCompleted(progress))
        {
            return;
        }

        TutorialSequenceData sequence = FindSequenceFor(progress);
        if (sequence == null)
        {
            return;
        }

        _pendingSequence = sequence;
        boardManager.SetTutorialBoardLocked(true);
    }

    private void OnMatchRaised(MatchEvent _) => _matchReceived = true;

    private void OnEnemySpawned(GameObject enemy)
    {
        _enemyAttackBehaviour = enemy.GetComponent<EnemyAttackBehaviour>();

        if (_pendingSequence == null)
        {
            return;
        }

        TutorialSequenceData seq = _pendingSequence;
        _pendingSequence = null;
        StartCoroutine(WaitThenRunSequence(seq));
    }

    private IEnumerator WaitThenRunSequence(TutorialSequenceData sequence)
    {
        yield return new WaitUntil(() => boardManager.IsBoardPopulated);
        yield return StartCoroutine(RunSequence(sequence));
    }

    private IEnumerator RunSequence(TutorialSequenceData sequence)
    {
        overlayUI.SetSequenceActive(true);
        transitionManager.SetSkipHoldBlocked(true);
        boardManager.SetTutorialBoardLocked(true);
        _enemyAttackBehaviour?.SetTutorialActive(true);
        globalTimeManager.TutorialFreezeTimeScale();

        yield return StartCoroutine(overlayUI.ShowBackdrop());

        if (sequence.Steps.Count > 0)
        {
            overlayUI.SetBrightZoneImmediate(sequence.Steps[0].BrightZoneIndex);
            yield return StartCoroutine(overlayUI.ShowStep(sequence.Steps[0]));
            yield return StartCoroutine(RunStep(sequence.Steps[0]));

            for (int i = 1; i < sequence.Steps.Count; i++)
            {
                TutorialStepData step = sequence.Steps[i];
                if (step is EnemyAttackTutorialStep enemyAttackStep)
                {
                    yield return StartCoroutine(RunEnemyAttackStep(enemyAttackStep));
                }
                else if (step is TimerTutorialStep timerStep)
                {
                    yield return StartCoroutine(RunTimerStep(timerStep));
                }
                else
                {
                    overlayUI.SetBrightZoneImmediate(step.BrightZoneIndex);
                    yield return StartCoroutine(overlayUI.TransitionStep(step));
                    yield return StartCoroutine(RunStep(step));
                }
            }

            yield return StartCoroutine(overlayUI.HideStep());
        }

        yield return StartCoroutine(overlayUI.HideBackdrop());

        globalTimeManager.TutorialUnfreezeTimeScale();
        saveDataManager.SetTutorialCompleted(sequence.ForProgress);
        _enemyAttackBehaviour?.SetTutorialActive(false);
        boardManager.SetTutorialBoardLocked(false);
        transitionManager.SetSkipHoldBlocked(false);
        overlayUI.SetSequenceActive(false);
    }

    private IEnumerator RunStep(TutorialStepData step)
    {
        switch (step)
        {
            case InputTutorialStep _:
                yield return StartCoroutine(RunInputStep());
                break;
            case BoardActionTutorialStep boardAction:
                yield return StartCoroutine(RunBoardActionStep(boardAction));
                break;
            case EnemyAttackTutorialStep enemyAttack:
                yield return StartCoroutine(RunEnemyAttackStep(enemyAttack));
                break;
            case TimerTutorialStep timer:
                yield return StartCoroutine(RunTimerStep(timer));
                break;
        }
    }

    private IEnumerator RunInputStep()
    {
        boardManager.SetTutorialBoardLocked(true);
        yield return StartCoroutine(overlayUI.WaitForConfirm());
    }

    private IEnumerator RunBoardActionStep(BoardActionTutorialStep step)
    {
        globalTimeManager.TutorialUnfreezeTimeScale();
        _matchReceived = false;
        boardManager.SetTutorialAllowedCell(step.HighlightedCell);
        boardHighlighter.ShowAt(step.HighlightedCell, boardManager);

        yield return new WaitUntil(() => _matchReceived);

        boardHighlighter.Hide();
        boardManager.ClearTutorialCellFilter();
        globalTimeManager.TutorialFreezeTimeScale();
    }

    private IEnumerator RunEnemyAttackStep(EnemyAttackTutorialStep step)
    {
        yield return StartCoroutine(overlayUI.HideStep());
        yield return StartCoroutine(overlayUI.HideBackdrop());

        boardManager.SetTutorialBoardLocked(true);
        globalTimeManager.TutorialUnfreezeTimeScale();
        yield return new WaitForSeconds(0.5f);
        enemyAttackEventChannel.Raise();
        yield return new WaitForSeconds(step.Delay);
        partyStatus.Heal(1f);
        globalTimeManager.TutorialFreezeTimeScale();

        overlayUI.SetBrightZoneImmediate(step.BrightZoneIndex);
        yield return StartCoroutine(overlayUI.ShowBackdrop());
        yield return StartCoroutine(overlayUI.ShowStep(step));
        yield return StartCoroutine(overlayUI.WaitForConfirm());
    }

    private IEnumerator RunTimerStep(TimerTutorialStep step)
    {
        yield return StartCoroutine(overlayUI.HideStep());
        yield return StartCoroutine(overlayUI.HideBackdrop());

        boardManager.SetTutorialBoardLocked(true);
        globalTimeManager.TutorialUnfreezeTimeScale();
        yield return new WaitForSeconds(step.Duration);
        globalTimeManager.TutorialFreezeTimeScale();

        overlayUI.SetBrightZoneImmediate(step.BrightZoneIndex);
        yield return StartCoroutine(overlayUI.ShowBackdrop());
        yield return StartCoroutine(overlayUI.ShowStep(step));
        yield return StartCoroutine(overlayUI.WaitForConfirm());
    }

    private TutorialSequenceData FindSequenceFor(TutorialProgress progress)
    {
        foreach (TutorialSequenceData seq in sequences)
        {
            if (seq.ForProgress == progress)
            {
                return seq;
            }
        }
        return null;
    }

}
