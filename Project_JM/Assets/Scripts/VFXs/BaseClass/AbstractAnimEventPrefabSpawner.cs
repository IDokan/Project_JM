// SPDX-License-Identifier: MIT
// Copyright (c) 01/21/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AbstractAnimEventPrefabSpawner.cs
// Summary: An abstract base script to spawn shield VFX.

using UnityEngine;

public abstract class AbstractAnimEventPrefabSpawner<TPrefab> : MonoBehaviour
    where TPrefab : UnityEngine.Object
{

    [SerializeField] protected TPrefab prefab;
    [SerializeField] protected Transform parentTransform;
    [SerializeField] protected Vector3 localOffset;
    [SerializeField] protected float baseRotatingDeg = 0f;

    protected TPrefab Spawn()
    {
        if (prefab == null)
        {
            return null;
        }

        Transform t = parentTransform ? parentTransform : transform;

        Vector3 pos = t.position + t.TransformVector(localOffset);

        // Rotate slash 
        Quaternion rot = Quaternion.Euler(0f, 0f, baseRotatingDeg);

        return Instantiate(prefab, pos, rot);
    }
}
