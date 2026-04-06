// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/14/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SlashSpawner.cs
// Summary: A script to spawn slash VFX.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class SlashSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_Slash()
        => Spawn();
}
