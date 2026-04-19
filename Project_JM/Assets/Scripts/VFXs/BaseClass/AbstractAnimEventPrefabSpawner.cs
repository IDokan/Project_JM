// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/21/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AbstractAnimEventPrefabSpawner.cs
// Summary: An abstract base script to spawn a prefab and optionally play a SFX on animation events.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public abstract class AbstractAnimEventPrefabSpawner<TPrefab> : MonoBehaviour
    where TPrefab : UnityEngine.Object
{

    [SerializeField] protected TPrefab prefab;
    [SerializeField] protected Transform parentTransform;
    [SerializeField] protected Vector3 localOffset;
    [SerializeField] protected float baseRotatingDeg = 0f;
    [SerializeField] private AudioCueSO sfx;

    protected TPrefab Spawn()
    {
        if (sfx != null)
            AudioManager.Instance.PlayActionSFX(sfx);
        return SpawnAtTransform(parentTransform);
    }

    protected TPrefab SpawnAtTransform(Transform givenTransform)
    {
        if (prefab == null)
        {
            return null;
        }

        Transform t = givenTransform ? givenTransform : transform;

        Vector3 pos = t.position + localOffset;

        // Rotate slash 
        Quaternion rot = Quaternion.Euler(0f, 0f, baseRotatingDeg);

        return Instantiate(prefab, pos, rot);
    }
}
