// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIManager.cs
// Summary: A manager that controls UI.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] BarStatusBinder enemyHPUIBinder;
    [SerializeField] AttackCooldownBinder enemyAttackUIBinder;
    [SerializeField] EnrageBarBinder enemyEnrageUIBinder;


    protected void OnEnable() => enemySpawnedEventChannel.OnRaised += OnSpawned;
    protected void OnDisable() => enemySpawnedEventChannel.OnRaised -= OnSpawned;

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
        if (enemyHPUIBinder != null)
        {
            enemyHPUIBinder.BindNewStatus(gameObject.GetComponent<CharacterStatus>());
        }
        if (enemyAttackUIBinder != null)
        {
            enemyAttackUIBinder.BindNewAI(gameObject.GetComponent<EnemyAttackBehaviour>());
        }
        if (enemyEnrageUIBinder != null)
        {
            enemyEnrageUIBinder.BindNewAI(gameObject.GetComponent<EnemyAttackBehaviour>());
        }
    }
}
