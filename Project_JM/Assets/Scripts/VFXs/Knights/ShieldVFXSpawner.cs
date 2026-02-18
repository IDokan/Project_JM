// SPDX-License-Identifier: MIT
// Copyright (c) 01/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ShieldVFXSpawner.cs
// Summary: A script to spawn shield VFX.

using UnityEngine;

public class ShieldVFXSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_Shield() => Spawn();
}
