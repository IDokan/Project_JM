// SPDX-License-Identifier: MIT
// Copyright (c) 02/03/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RayFrostSpawner.cs
// Summary: A script to spawn ray of frost prefab.

using UnityEngine;

public class RayFrostSpawner : AbstractAnimEventPrefabRandomTransformSpawner<GameObject>
{

    [SerializeField] protected Transform[] targets;


    public void AnimEvent_SpawnRayofFrost()
    {
        GameObject rayFrost = Spawn();

        Transform target = targets[Random.Range(0, targets.Length)];


        Vector2 direction = new Vector2(1f, 0f);
        Vector2 distance = (Vector2)(target.position - rayFrost.transform.position);
        if (distance.sqrMagnitude > 0.0001f)
        {
            direction = distance.normalized;
        }

        rayFrost.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }
}
