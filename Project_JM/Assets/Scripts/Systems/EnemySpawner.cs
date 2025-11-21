// SPDX-License-Identifier: MIT
// Copyright (c) 11/14/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemySpawner.cs
// Summary: A class to spawn enemy.

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] protected EnemyBook _enemyBook;
    [SerializeField] protected CharacterDeathEventChannel _characterDeathEventChannel;
    [SerializeField] protected EnemySpawnedEventChannel _enemySpawnedEventChannel;

    [SerializeField] protected DifficultyCurves _difficultyCurves;

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

        var spawnedEnemy = Instantiate(_enemyBook.GetRandomEnemyPrefab(), new Vector3(8f, 0.5f, 0f), Quaternion.identity);
        spawnedEnemy.GetComponent<CharacterStatus>().Initialize(_difficultyCurves.GetDifficultyMultiplier(_numSpanwed));

        _enemySpawnedEventChannel.Raise(spawnedEnemy);
        return spawnedEnemy;
    }

    protected void OnCharacterDied(CharacterStatus stat)
    {
        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            SpawnRandomEnemy();
        }
    }
}
