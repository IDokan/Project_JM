// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/26/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AimPointSpawner.cs
// Summary: A spawner to spawn AimPoint prefab.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class AimPointSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnAimPoint()
        => Spawn();
}
