// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/21/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: StunVFXSpawner.cs
// Summary: A script to spawn stun attack VFX.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class StunAttackVFXSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_StunAttackVFX()
        => Spawn();
}
