// SPDX-License-Identifier: MIT
// Copyright (c) 11/14/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIManager.cs
// Summary: A manager that controls UI.

using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] EnemySpawnedEventChannel _enemySpawnedEventChannel;
    [SerializeField] BarStatusBinder _allyHPUIBinder;
    [SerializeField] BarStatusBinder _enemyHPUIBinder;
    [SerializeField] AttackCooldownBinder _enemyAttackUIBinder;


    protected void OnEnable() => _enemySpawnedEventChannel.OnRaised += OnSpawned;
    protected void OnDisable() => _enemySpawnedEventChannel.OnRaised -= OnSpawned;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void OnSpawned(GameObject gameObject)
    {
        _enemyHPUIBinder.BindNewStatus(gameObject.GetComponent<CharacterStatus>());
        _enemyAttackUIBinder.BindNewAI(gameObject.GetComponent<EnemyAttackBehaviour>());
    }
}
