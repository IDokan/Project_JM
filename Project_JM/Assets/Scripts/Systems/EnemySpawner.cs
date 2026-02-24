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

    [SerializeField] protected DifficultyCurves _difficultyCurves;

    [SerializeField] protected Transform _spawnPosition;

    [SerializeField] protected float spawnDelay = 4f;
    [SerializeField] protected float dispatchEventChannelDelay = 1f;

    protected void OnEnable() => _characterDeathEventChannel.OnRaised += OnCharacterDied;
    protected void OnDisable() => _characterDeathEventChannel.OnRaised -= OnCharacterDied;

    protected int _numSpanwed = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    protected void OnCharacterDied(CharacterStatus stat)
    {
        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            StartCoroutine(SpawnEnemyAfterDelay());
        }
    }

    protected IEnumerator SpawnEnemyAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        SpawnRandomEnemy();
    }
}
