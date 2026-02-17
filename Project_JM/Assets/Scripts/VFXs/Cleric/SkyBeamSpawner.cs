// SPDX-License-Identifier: MIT
// Copyright (c) 02/12/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SkyBeamSpawner.cs
// Summary: A script to spawn SkyBeam effect of Cleric's match 4 attack..

using UnityEngine;

public class SkyBeamSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    [SerializeField] protected CombatManager _combatManager;

    public void AnimEvent_SpawnSkyBeam()
    {
        GameObject skyBeam = Spawn();

        if (_combatManager != null)
        {
            Transform enemy = _combatManager.Enemy.transform.GetChild(0);
            skyBeam.transform.SetParent(enemy, worldPositionStays: false);
            skyBeam.transform.localPosition = Vector3.zero;
            skyBeam.transform.localRotation = Quaternion.identity;
        }
    }
}
