// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/03/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AbstractAnimEventPrefabRandomTransformSpawner.cs
// Summary: A script for ice shard spawner.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(GetRandomTransform))]
public abstract class AbstractAnimEventPrefabRandomTransformSpawner<TPrefab> : MonoBehaviour
    where TPrefab : UnityEngine.Object
{
    [SerializeField] protected TPrefab prefab;
    [SerializeField] protected GetRandomTransform randomTransformScript;
    [SerializeField] protected Vector3 localOffset;

    public TPrefab Spawn(TPrefab givenPrefab = null, bool isCachedTransform = true)
    {
        if (prefab == null && givenPrefab == null)
        {
            return null;
        }

        if (randomTransformScript == null)
        {
            randomTransformScript = GetComponent<GetRandomTransform>();
        }

        Transform t = isCachedTransform ? randomTransformScript.GetCachedTransform() : randomTransformScript.RandomTransform();

        Vector3 pos = t.position + localOffset;

        return Instantiate(givenPrefab == null ? prefab : givenPrefab, pos, Quaternion.identity);
    }
}
