// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemySpawner.cs
// Summary: A class to spawn enemy.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] protected EnemyBook enemyBook;
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected DifficultyCurves difficultyCurves;

    [SerializeField] protected Vector3 spawnPosition;

    [SerializeField] protected float spawnDelay = 4f;
    [SerializeField] protected float dispatchEventChannelDelay = 1f;

    protected Vector3 _spawnOffsetToCamera;
    protected int _numSpanwed = 0;

    protected Coroutine _dispatchRoutine = null;

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
        _spawnOffsetToCamera = spawnPosition - Camera.main.transform.position;
    }

    protected void Clear()
    {
        _numSpanwed = 0;
    }

    protected GameObject SpawnRandomEnemy()
    {
        _numSpanwed++;

        Vector3 spawnPosition = _spawnOffsetToCamera + Camera.main.transform.position;
        var spawnedEnemy = Instantiate(enemyBook.GetRandomEnemyPrefab(), spawnPosition, Quaternion.identity);
        spawnedEnemy.GetComponent<CharacterStatus>().Initialize(difficultyCurves.GetDifficultyMultiplier(_numSpanwed));

        if (_dispatchRoutine != null)
        {
            StopCoroutine(_dispatchRoutine);
            _dispatchRoutine = null;
        }
        _dispatchRoutine = StartCoroutine(DispatchSpawnEventChannelAfterDelay(spawnedEnemy, dispatchEventChannelDelay));

        return spawnedEnemy;
    }

    protected IEnumerator DispatchSpawnEventChannelAfterDelay(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        enemy.GetComponent<EnemyActivation>()?.EnableScripts();
        enemySpawnedEventChannel.Raise(enemy);

        _dispatchRoutine = null;
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
