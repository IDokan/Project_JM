// SPDX-License-Identifier: MIT
// Copyright (c) 01/21/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: StunVFXSpawner.cs
// Summary: A script to spawn stun attack VFX.

using UnityEngine;

public class StunAttackVFXSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_StunAttackVFX()
        => Spawn();
}
