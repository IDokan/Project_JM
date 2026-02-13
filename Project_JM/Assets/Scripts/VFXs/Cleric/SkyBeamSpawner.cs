// SPDX-License-Identifier: MIT
// Copyright (c) 02/12/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SkyBeamSpawner.cs
// Summary: A script to spawn SkyBeam effect of Cleric's match 4 attack..

using UnityEngine;

public class SkyBeamSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnSkyBeam()
    => Spawn();
}
