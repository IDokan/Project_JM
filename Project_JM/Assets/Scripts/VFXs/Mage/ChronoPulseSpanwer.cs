// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ChronoPulseSpawner.cs
// Summary: A script to spawn Chrono Pulse VFX.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class ChronoPulseSpanwer : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnChronoPulse()
        => Spawn();
}
