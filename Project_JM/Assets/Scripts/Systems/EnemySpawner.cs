// SPDX-License-Identifier: MIT
// Copyright (c) 11/14/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemySpawner.cs
// Summary: A class to spawn enemy.

using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] protected EnemyBook _enemyBook;
    [SerializeField] protected CharacterDeathEventChannel _characterDeathEventChannel;
    [SerializeField] protected EnemySpawnedEventChannel _enemySpawnedEventChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected DifficultyCurves _difficultyCurves;

    [SerializeField] protected Vector3 _spawnPosition;

    [SerializeField] protected float spawnDelay = 4f;
    [SerializeField] protected float dispatchEventChannelDelay = 1f;

    protected Vector3 spawnOffsetToCamera;
    protected int _numSpanwed = 0;

    protected Coroutine dispatchRoutine = null;

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }
    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void Awake()
    {
        spawnOffsetToCamera = _spawnPosition - Camera.main.transform.position;
    }

    protected void Clear()
    {
        _numSpanwed = 0;
    }

    protected GameObject SpawnRandomEnemy()
    {
        _numSpanwed++;

        Vector3 spawnPosition = spawnOffsetToCamera + Camera.main.transform.position;
        var spawnedEnemy = Instantiate(_enemyBook.GetRandomEnemyPrefab(), spawnPosition, Quaternion.identity);
        spawnedEnemy.GetComponent<CharacterStatus>().Initialize(_difficultyCurves.GetDifficultyMultiplier(_numSpanwed));

        if (dispatchRoutine != null)
        {
            StopCoroutine(dispatchRoutine);
            dispatchRoutine = null;
        }
        dispatchRoutine = StartCoroutine(DispatchSpawnEventChannelAfterDelay(spawnedEnemy, dispatchEventChannelDelay));

        return spawnedEnemy;
    }

    protected IEnumerator DispatchSpawnEventChannelAfterDelay(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        enemy.GetComponent<EnemyActivation>()?.EnableScripts();
        _enemySpawnedEventChannel.Raise(enemy);

        dispatchRoutine = null;
    }

    public void SpawnEnemyAfterDelay()
    {
        StartCoroutine(SpawnEnemyAfterDelayRoutine(spawnDelay));
    }

    protected IEnumerator SpawnEnemyAfterDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnRandomEnemy();
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroPartyMoveEnd)
        {
            SpawnRandomEnemy();
        }
        else if (phase == TransitionPhase.MiddleTransitionStarts)
        {
            SpawnEnemyAfterDelay();
        }
        else if(phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
        }
    }
}
