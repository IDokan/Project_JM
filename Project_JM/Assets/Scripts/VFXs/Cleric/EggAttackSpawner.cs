// SPDX-License-Identifier: MIT
// Copyright (c) 02/09/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EggAttackSpawner.cs
// Summary: A script to spawn an egg attack projectile.

using UnityEngine;

public class EggAttackSpawner : AbstractAnimEventPrefabSpawner<FlyToTarget>
{
    [SerializeField] CombatManager combatManager;
    [SerializeField] AttackExecutor clericAttackExecutor;

    public void AnimEvent_SpawnEgg()
    {
        FlyToTarget egg = Spawn();
        egg.Init(clericAttackExecutor, combatManager.EnemyTransform);
    }
}
