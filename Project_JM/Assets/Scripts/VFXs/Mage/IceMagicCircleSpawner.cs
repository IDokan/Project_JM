// SPDX-License-Identifier: MIT
// Copyright (c) 02/01/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IceMagicCircleSpawner.cs
// Summary: A script to spawn Ice magic circle.

using UnityEngine;

public class IceMagicCircleSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnMagicCircle()
        => Spawn();
}
