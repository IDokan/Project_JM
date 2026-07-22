// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemySpawner.cs
// Summary: Spawns enemies by delegating group progression to EnemyBook.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using CharacterEnums;
using TutorialEnums;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyBook easyEnemyBook;
    [SerializeField] private EnemyBook mediumEnemyBook;
    [SerializeField] private EnemyBook hardEnemyBook;

    protected EnemyBook _enemyBook;

    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] protected EnemyAlertEventChannel enemyAlertEventChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected DifficultyCurvesSelector curvesSelector;

    [SerializeField] protected Vector3 landscapeSpawnPosition;
    [SerializeField] protected Vector3 portraitSpawnPosition;
    [SerializeField] protected float dispatchEventChannelDelay = 1f;

    protected Vector3 _spawnOffsetToCamera;
    protected int _numSpanwed = 0;

    protected Coroutine _dispatchRoutine = null;

    private float _spawnTime;
    private CharacterId _spawnedEnemyId;

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
        if (SaveDataManager.Instance.Progress >= TutorialProgress.Hard)
        {
            _enemyBook = hardEnemyBook;
        }
        else if (SaveDataManager.Instance.Progress >= TutorialProgress.Medium)
        {
            _enemyBook = mediumEnemyBook;
        }
        else
        {
            _enemyBook = easyEnemyBook;
        }

        bool isPortrait = Screen.height > Screen.width;
        Vector3 spawnPosition = isPortrait ? portraitSpawnPosition : landscapeSpawnPosition;

        CameraOrientationSetter cameraOrientationSetter = Camera.main.GetComponent<CameraOrientationSetter>();
        Vector3 cameraPosition = cameraOrientationSetter != null ? cameraOrientationSetter.OriginalPosition : Camera.main.transform.position;
        _spawnOffsetToCamera = spawnPosition - cameraPosition;
    }

    protected void Clear()
    {
        _numSpanwed = 0;
        _enemyBook.ResetProgression();
    }

    protected GameObject SpawnNextEnemy()
    {
        _numSpanwed++;

        GameObject prefab = _enemyBook.GetNextEnemy();

        Vector3 pos = _spawnOffsetToCamera + Camera.main.transform.position;
        var spawnedEnemy = Instantiate(prefab, pos, Quaternion.identity);
        var characterStatus = spawnedEnemy.GetComponent<CharacterStatus>();
        characterStatus.Initialize(curvesSelector.ActiveCurves.GetDifficultyMultiplier(_numSpanwed));
        _spawnTime = Time.time;
        _spawnedEnemyId = characterStatus.CharacterId;

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
        else if (phase == TransitionPhase.MiddleTransitionStarts)
        {
            enemyAlertEventChannel.Raise(_enemyBook.PeekNextEnemy());
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
