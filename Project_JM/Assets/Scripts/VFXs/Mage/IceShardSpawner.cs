// SPDX-License-Identifier: MIT
// Copyright (c) 01/30/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IceShardSpawner.cs
// Summary: A script for ice shard spawner.

using UnityEngine;

public class IceShardSpawner : MonoBehaviour
{
    [SerializeField] protected IceShardProjectile prefab;

    [SerializeField] protected float speed = 14f;
    [SerializeField] protected float lifeTime = 1.5f;

    [SerializeField] protected GetRandomTransform randomTransform;
    [SerializeField] protected Vector3 muzzleOffset;
    [SerializeField] protected Transform[] targets;


    public void AnimEvent_SpawnIceShard()
    {
        if (prefab == null)
        {
            return;
        }
        if (randomTransform == null)
        {
            randomTransform = GetComponent<GetRandomTransform>();
        }
        if (targets == null || targets.Length <= 0 || targets[0] == null)
        {
            return;
        }

        SpawnIceShard(randomTransform.GetCachedTransform(), targets[Random.Range(0, targets.Length)]);
    }

    protected void SpawnIceShard(Transform muzzle, Transform target)
    {
        Transform t = muzzle != null ? muzzle : transform;
        Vector3 spawnPos = t.position + muzzleOffset;

        Vector2 direction = new Vector2(1f, 0f);
        Vector2 distance = (Vector2)(target.position - muzzle.position);
        if (distance.sqrMagnitude > 0.0001f)
        {
            direction = distance.normalized;
        }

        IceShardProjectile p = Instantiate(prefab, spawnPos, Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        p.Init(GetComponent<AttackExecutor>(), direction, speed, lifeTime);
    }
}
