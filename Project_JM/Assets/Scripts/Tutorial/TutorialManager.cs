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
    [SerializeField] private TutorialBoardHighlighter boardHighlighter;
    [SerializeField] private MatchEventChannel matchEventChannel;
    [SerializeField] private EnemySpawnedEventChannel enemySpawnedEventChannel;

    private bool _matchReceived;
    private TutorialSequenceData _pendingSequence;

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

    private void OnEnemySpawned(GameObject _)
    {
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
        boardManager.SetTutorialBoardLocked(true);

        foreach (TutorialStepData step in sequence.Steps)
        {
            yield return StartCoroutine(overlayUI.ShowStep(step));
            yield return StartCoroutine(RunStep(step));
            yield return StartCoroutine(overlayUI.HideStep());
        }

        saveDataManager.SetTutorialCompleted(sequence.ForProgress);
        boardManager.SetTutorialBoardLocked(false);
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
        _matchReceived = false;
        boardManager.SetTutorialAllowedCell(step.HighlightedCell);
        boardHighlighter.ShowAt(step.HighlightedCell, boardManager);

        yield return new WaitUntil(() => _matchReceived);

        boardHighlighter.Hide();
        boardManager.ClearTutorialCellFilter();
    }

    private IEnumerator RunTimerStep(TimerTutorialStep step)
    {
        boardManager.SetTutorialBoardLocked(true);
        yield return new WaitForSeconds(step.Duration);
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
