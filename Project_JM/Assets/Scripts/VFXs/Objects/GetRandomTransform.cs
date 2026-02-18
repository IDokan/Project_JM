// SPDX-License-Identifier: MIT
// Copyright (c) 02/02/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GetRandomTransforms.cs
// Summary: A script to return random transform by seirialized field and cache it.

using UnityEngine;

public class GetRandomTransform : MonoBehaviour
{
    [SerializeField] protected Transform[] transforms;

    int cachedIndex;

    public Transform RandomTransform()
    {
        if (transforms == null || transforms.Length <= 0 || transforms[0] == null)
        {
            return null;
        }

        cachedIndex = Random.Range(0, transforms.Length);
        return transforms[cachedIndex];
    }

    public Transform GetCachedTransform()
    {
        return transforms[cachedIndex];
    }
}
