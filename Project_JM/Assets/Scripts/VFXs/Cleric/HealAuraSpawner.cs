// SPDX-License-Identifier: MIT
// Copyright (c) 02/12/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: HealAuraSpawner.cs
// Summary: A script to spawn heal aura of Cleric's match 4 attack.

using UnityEngine;

public class HealAuraSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnHealAura()
    => Spawn();
}
