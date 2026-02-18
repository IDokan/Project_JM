// SPDX-License-Identifier: MIT
// Copyright (c) 01/26/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AimPointSpawnerT5.cs
// Summary: A spawner to spawn AimPointT5 prefab.

using UnityEngine;

public class AimPointT5Spawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnAimPointT5()
        => Spawn();
}
