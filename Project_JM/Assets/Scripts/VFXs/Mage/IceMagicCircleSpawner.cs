// SPDX-License-Identifier: MIT
// Copyright (c) 02/01/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IceMagicCircleSpawner.cs
// Summary: A script to spawn Ice magic circle.

using UnityEngine;

public class IceMagicCircleSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    [SerializeField] protected GetRandomTransform randomTransform;

    protected void Start()
    {
        if(randomTransform == null)
        {
            randomTransform = GetComponent<GetRandomTransform>();
        }
    }

    public void AnimEvent_SpawnMagicCircle()
    {
        SpawnAtTransform(randomTransform.RandomTransform());
    }
}
