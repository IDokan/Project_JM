// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/14/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ShieldVFXSpawner.cs
// Summary: A script to spawn shield VFX.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class ShieldVFXSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_Shield() => Spawn();
}
