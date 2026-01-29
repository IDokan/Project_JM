// SPDX-License-Identifier: MIT
// Copyright (c) 01/28/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GlassCrackedSpawner.cs
// Summary: A script to spawn glass cracked prefab.

using UnityEngine;

public class GlassCrackedSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnGlassCracked()
        => Spawn();
}
