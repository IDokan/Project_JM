// SPDX-License-Identifier: MIT
// Copyright (c) 01/26/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AimPointSpawner.cs
// Summary: A spawner to spawn AimPoint prefab.

using UnityEngine;

public class AimPointSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnAimPoint()
        => Spawn();
}
