// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/17/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DefeatedTransitionController.cs
// Summary: A script to control logic when party defeated.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.
//                      Defeated logic conducts below tasks:
//                                          1. Move party down.
//                                          2. Move enemy&UI aside.
//                                          3. Fade out combat BGM.

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefeatedTransitionController : TransitionController
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;

    [Header("Party")]
    [SerializeField] protected Transform partyTransform;
    [SerializeField] protected float partyYOffset = 0;

    [Header("Enemy")]
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnEventChannel;
    [SerializeField] protected Vector3 enemyArrivalPosition;

    [Header("Board")]
    [SerializeField] protected Transform boardTransform;
    [SerializeField] protected Vector3 boardArrivalPosition;

    [Header("Retry Menu")]
    [SerializeField] private RetryMenu retryMenu;
    [SerializeField] private float retryShowDelay = 1f;

    [Header("Timing")]
    [SerializeField] protected float partyMoveDelay = 1f;
    [SerializeField] protected float partyMoveDuration = 1f;
    [SerializeField] protected float enemyMoveDelay = 1f;
    [SerializeField] protected float enemyMoveDuration = 1f;
    [SerializeField] protected float boardMoveDelay = 1f;
    [SerializeField] protected float boardMoveDuration = 1f;

    Transform enemyTransform = null;
    protected Vector3 _enemyOffsetToCamera;

    protected override void Awake()
    {
        base.Awake();
        Transform cameraTransform = Camera.main.transform;
        partyYOffset = partyYOffset - cameraTransform.position.y;
        _enemyOffsetToCamera = enemyArrivalPosition - cameraTransform.position;
    }

    protected void OnEnable()
    {
        characterDeathEventChannel.OnRaised += OnCharacterDied;
        enemySpawnEventChannel.OnRaised += OnEnemySpawned;
    }

    protected void OnDisable()
    {
        characterDeathEventChannel.OnRaised -= OnCharacterDied;
        enemySpawnEventChannel.OnRaised -= OnEnemySpawned;
    }

    protected void StartDefeatedTransition()
    {
        RequestTransitionStart(BeginDefeatedTransition);
    }

    protected void BeginDefeatedTransition()
    {
        transitionEventChannel.Raise(TransitionPhase.EndTransitionBegin);

        if (partyTransform != null)
        {
            StartCoroutine(PartyRoutine());
        }
        if (enemyTransform != null)
        {
            StartCoroutine(EnemyRoutine());
        }
        if (boardTransform != null)
        {
            StartCoroutine(BoardRoutine());
        }

        StartCoroutine(ShowRetryMenuRoutine());
    }

    protected IEnumerator PartyRoutine()
    {
        Vector3 partyStartPosition = partyTransform.position;
        Vector3 partyArrivalPosition = partyStartPosition + new Vector3(0f, partyYOffset, 0);

        yield return MoveParallaxObject(partyTransform, partyArrivalPosition, partyMoveDelay, partyMoveDuration, TransitionPhase.EndPartyMoveEnd);
    }

    protected IEnumerator EnemyRoutine()
    {
        yield return new WaitForSeconds(enemyMoveDelay);
        transitionEventChannel.Raise(TransitionPhase.EndEnemyMoveBegin);
        Vector3 enemyTargetLocation = Camera.main.transform.position + _enemyOffsetToCamera;
        yield return MoveParallaxObject(enemyTransform, enemyTargetLocation, 0f, enemyMoveDuration, TransitionPhase.EndEnemyMoveEnd);
    }

    protected IEnumerator BoardRoutine()
    {
        yield return new WaitForSeconds(boardMoveDelay);

        Vector3 boardStartPosition = boardTransform.localPosition;

        float t = 0f;
        while (t < boardMoveDuration)
        {
            t += Time.deltaTime;
            boardTransform.localPosition = Vector3.Lerp(boardStartPosition, boardArrivalPosition, t / boardMoveDuration);
            yield return null;
        }

        boardTransform.localPosition = boardArrivalPosition;

        transitionEventChannel.Raise(TransitionPhase.EndBoardMoveEnd);
    }

    private IEnumerator ShowRetryMenuRoutine()
    {
        yield return new WaitForSeconds(retryShowDelay);
        retryMenu.Show();
        CompleteTransition();
    }

    protected void OnCharacterDied(CharacterStatus characterStat)
    {
        if (characterStat.TryGetComponent<AllyTag>(out _))
        {
            StartDefeatedTransition();
        }
    }

    protected void OnEnemySpawned(GameObject spawnedEnemy)
    {
        if (spawnedEnemy != null)
        {
            enemyTransform = spawnedEnemy.GetComponent<Transform>();
        }
    }

    protected IEnumerator MoveParallaxObject(Transform targetObject, Vector3 targetPosition, float delay, float duration, TransitionPhase eventPhase)
    {
        yield return new WaitForSeconds(delay);

        if (targetObject != null)
        {
            ParallaxLayer targetParallaxLayer = targetObject.GetComponent<ParallaxLayer>();
            if (targetParallaxLayer != null)
            {
                targetParallaxLayer.SetManualMode();
            }

            Vector3 startPosition = targetObject.position;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                targetObject.position = Vector3.Lerp(startPosition, targetPosition, t / duration);
                yield return null;
            }

            targetObject.position = targetPosition;

            if (targetParallaxLayer != null)
            {
                targetParallaxLayer.SetParallaxMode();
            }
        }

        transitionEventChannel.Raise(eventPhase);
    }
}
