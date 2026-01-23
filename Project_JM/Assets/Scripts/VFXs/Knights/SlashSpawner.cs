// SPDX-License-Identifier: MIT
// Copyright (c) 01/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SlashSpawner.cs
// Summary: A script to spawn slash VFX.

using UnityEngine;

public class SlashSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_Slash()
        => Spawn();
}
