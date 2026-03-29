// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/12/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SkyBeamSpawner.cs
// Summary: A script to spawn SkyBeam effect of Cleric's match 4 attack..
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class SkyBeamSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    [SerializeField] protected CombatManager combatManager;

    public void AnimEvent_SpawnSkyBeam()
    {
        GameObject skyBeam = Spawn();

        if (combatManager != null && combatManager.Enemy != null)
        {
            Transform enemy = combatManager.EnemyTransform.GetChild(0);
            skyBeam.transform.SetParent(enemy, worldPositionStays: false);
            skyBeam.transform.localPosition = Vector3.zero;
            skyBeam.transform.localRotation = Quaternion.identity;
        }
    }
}
