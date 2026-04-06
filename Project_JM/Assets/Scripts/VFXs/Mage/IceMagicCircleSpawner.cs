// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IceMagicCircleSpawner.cs
// Summary: A script to spawn Ice magic circle.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class IceMagicCircleSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnMagicCircle()
        => Spawn();
}
