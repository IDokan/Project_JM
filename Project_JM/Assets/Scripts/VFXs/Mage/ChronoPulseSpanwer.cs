// SPDX-License-Identifier: MIT
// Copyright (c) 02/08/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ChronoPulseSpawner.cs
// Summary: A script to spawn Chrono Pulse VFX.

using UnityEngine;

public class ChronoPulseSpanwer : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnChronoPulse()
        => Spawn();
}
