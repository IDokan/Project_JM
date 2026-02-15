// SPDX-License-Identifier: MIT
// Copyright (c) 02/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ChickenGodSpawner.cs
// Summary: A script to spawn Chicken God effect of Cleric's match 5 attack.

using UnityEngine;

public class ChickenGodSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnChickenGod()
        => Spawn();
}
