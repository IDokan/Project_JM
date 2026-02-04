// SPDX-License-Identifier: MIT
// Copyright (c) 02/03/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AbstractAnimEventPrefabRandomTransformSpawner.cs
// Summary: A script for ice shard spawner.

using UnityEngine;

[RequireComponent(typeof(GetRandomTransform))]
public abstract class AbstractAnimEventPrefabRandomTransformSpawner<TPrefab> : MonoBehaviour
    where TPrefab : UnityEngine.Object
{
    [SerializeField] protected TPrefab prefab;
    [SerializeField] protected GetRandomTransform randomTransformScript;
    [SerializeField] protected Vector3 localOffset;

    public TPrefab Spawn(bool isCachedTransform = true)
    {
        if (prefab == null)
        {
            return null;
        }

        if (randomTransformScript == null)
        {
            randomTransformScript = GetComponent<GetRandomTransform>();
        }

        Transform t = isCachedTransform ? randomTransformScript.GetCachedTransform() : randomTransformScript.RandomTransform();

        Vector3 pos = t.position + localOffset;

        return Instantiate(prefab, pos, Quaternion.identity);
    }
}
