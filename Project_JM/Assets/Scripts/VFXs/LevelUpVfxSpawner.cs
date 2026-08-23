// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LevelUpVfxSpawner.cs
// Summary: Spawns a level-up VFX prefab at a serialized spawn point, parented
//          under it. Triggered by RewardDefinition.SpawnLevelUpVfx() via
//          GetComponentInParent, the same lookup FlashGlow uses for
//          GemPowerGlowEffect.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class LevelUpVfxSpawner : MonoBehaviour
{
    [SerializeField] private GameObject levelUpVfxPrefab;
    [SerializeField] private Transform spawnPoint;

    public void Spawn()
    {
        if (levelUpVfxPrefab == null || spawnPoint == null)
        {
            return;
        }

        Instantiate(levelUpVfxPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
    }
}
