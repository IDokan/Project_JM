// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/30/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IceShardSpawner.cs
// Summary: A script for ice shard spawner.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class IceShardSpawner : AbstractAnimEventPrefabRandomTransformSpawner<IceShardProjectile>
{
    [SerializeField] protected float speed = 14f;
    [SerializeField] protected float lifeTime = 1.5f;

    [SerializeField] protected Transform[] targets;


    public void AnimEvent_SpawnIceShard()
    {
        IceShardProjectile  iceShard = Spawn();

        Transform target = targets[Random.Range(0, targets.Length)];


        Vector2 direction = new Vector2(1f, 0f);
        Vector2 distance = (Vector2)(target.position - iceShard.transform.position);
        if (distance.sqrMagnitude > 0.0001f)
        {
            direction = distance.normalized;
        }

        iceShard.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        iceShard.Init(GetComponent<AttackExecutor>(), direction, speed, lifeTime);
    }
}
