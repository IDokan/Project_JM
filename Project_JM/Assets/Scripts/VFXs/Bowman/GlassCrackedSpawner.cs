// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/28/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GlassCrackedSpawner.cs
// Summary: A script to spawn glass cracked prefab.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class GlassCrackedSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    public void AnimEvent_SpawnGlassCracked()
    {
        GameObject gameObject = Spawn();

        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }
}
