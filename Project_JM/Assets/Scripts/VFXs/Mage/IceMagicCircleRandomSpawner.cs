// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/01/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IceMagicCircleRandomSpawner.cs
// Summary: A script to spawn Ice magic circle using GetRandomTransform script.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class IceMagicCircleRandomSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    [SerializeField] protected GetRandomTransform randomTransform;

    protected void Start()
    {
        if(randomTransform == null)
        {
            randomTransform = GetComponent<GetRandomTransform>();
        }
    }

    public void AnimEvent_SpawnRandomMagicCircle()
    {
        SpawnAtTransform(randomTransform.RandomTransform());
    }
}
