// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemySpawner.cs
// Summary: Spawns enemies by delegating group progression to EnemyBook.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] protected EnemyBook enemyBook;
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected DifficultyCurves difficultyCurves;

    [SerializeField] protected Vector3 spawnPosition;
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
        enemyBook.ResetProgression();
    }

    protected GameObject SpawnNextEnemy()
    {
        _numSpanwed++;

        GameObject prefab = enemyBook.GetNextEnemy();

        Vector3 pos = _spawnOffsetToCamera + Camera.main.transform.position;
        var spawnedEnemy = Instantiate(prefab, pos, Quaternion.identity);
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

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroPartyMoveEnd)
        {
            SpawnNextEnemy();
        }
        else if (phase == TransitionPhase.MiddleEnemySpawnBegin)
        {
            SpawnNextEnemy();
        }
        else if (phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
        }
    }
}
