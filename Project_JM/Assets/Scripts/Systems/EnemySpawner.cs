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

    [SerializeField] protected Transform _spawnPosition;

    [SerializeField] protected float spawnDelay = 4f;
    [SerializeField] protected float dispatchEventChannelDelay = 1f;

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }
    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected int _numSpanwed = 0;

    protected GameObject SpawnRandomEnemy()
    {
        _numSpanwed++;

        var spawnedEnemy = Instantiate(_enemyBook.GetRandomEnemyPrefab(), _spawnPosition.position, _spawnPosition.rotation);
        spawnedEnemy.GetComponent<CharacterStatus>().Initialize(_difficultyCurves.GetDifficultyMultiplier(_numSpanwed));

        StartCoroutine(DispatchSpawnEventChannelAfterDelay(spawnedEnemy, dispatchEventChannelDelay));

        return spawnedEnemy;
    }

    protected IEnumerator DispatchSpawnEventChannelAfterDelay(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        enemy.GetComponent<EnemyActivation>()?.EnableScripts();
        _enemySpawnedEventChannel.Raise(enemy);
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
    }
}
