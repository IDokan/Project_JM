// SPDX-License-Identifier: MIT
// Copyright (c) 03/17/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DefeatedTransitionController.cs
// Summary: A script to control logic when party defeated.
//                      Defeated logic conducts below tasks:
//                                          1. Move party down.
//                                          2. Move enemy&UI aside.

using System.Collections;
using UnityEngine;

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

    [Header("UI Positions")]
    [SerializeField] protected RectTransform[] uiTransforms;
    [SerializeField] protected Vector3[] uiArrivalPositions;

    [Header("Timing")]
    [SerializeField] protected float partyMoveDelay = 1f;
    [SerializeField] protected float partyMoveDuration = 1f;
    [SerializeField] protected float enemyMoveDelay = 1f;
    [SerializeField] protected float enemyMoveDuration = 1f;
    [SerializeField] protected float boardMoveDelay = 1f;
    [SerializeField] protected float boardMoveDuration = 1f;
    [SerializeField] protected float uiMoveDelay = 1f;
    [SerializeField] protected float uiMoveDuration = 1f;

    Transform enemyTransform = null;
    protected Vector3 enemyOffsetToCamera;

    void Awake()
    {
        Transform cameraTransform = Camera.main.transform;
        partyYOffset = partyYOffset - cameraTransform.position.y;
        enemyOffsetToCamera = enemyArrivalPosition - cameraTransform.position;
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
        RaiseStarted();
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
        if (uiTransforms.Length > 0 && uiArrivalPositions.Length == uiTransforms.Length)
        {
            StartCoroutine(UIRoutine());
        }
    }

    protected IEnumerator PartyRoutine()
    {

        Vector3 partyStartPosition = partyTransform.position;
        Vector3 partyArrivalPosition = partyStartPosition + new Vector3(0f, partyYOffset, 0);

        yield return MoveParallaxObject(partyTransform, partyArrivalPosition, partyMoveDelay, partyMoveDuration, TransitionPhase.EndPartyMoveEnd);
    }

    protected IEnumerator EnemyRoutine()
    {
        Vector3 enemyTargetLocation = Camera.main.transform.position + enemyOffsetToCamera;
        yield return MoveParallaxObject(enemyTransform, enemyTargetLocation, enemyMoveDelay, enemyMoveDuration, TransitionPhase.EndEnemyMoveEnd);
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

    protected IEnumerator UIRoutine()
    {
        yield return new WaitForSeconds(uiMoveDelay);

        Vector2[] uiStartPositions = new Vector2[uiTransforms.Length];

        for (int i = 0; i < uiTransforms.Length; i++)
        {
            uiStartPositions[i] = uiTransforms[i].anchoredPosition;
            uiTransforms[i].gameObject.SetActive(true);
        }

        float t = 0f;
        while (t < uiMoveDuration)
        {
            t += Time.deltaTime;

            for (int i = 0; i < uiTransforms.Length; i++)
            {
                RectTransform uiTransform = uiTransforms[i];
                uiTransform.anchoredPosition = Vector2.Lerp(uiStartPositions[i], uiArrivalPositions[i], t / uiMoveDuration);
            }
            yield return null;
        }

        for (int i = 0; i < uiTransforms.Length; i++)
        {
            RectTransform uiTransform = uiTransforms[i];
            uiTransform.anchoredPosition = uiArrivalPositions[i];
        }

        RaiseCompleted();
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

        transitionEventChannel.Raise(eventPhase);
    }
}
