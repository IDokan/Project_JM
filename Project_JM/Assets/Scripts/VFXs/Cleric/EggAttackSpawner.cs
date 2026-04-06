// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EggAttackSpawner.cs
// Summary: A script to spawn an egg attack projectile.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

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
