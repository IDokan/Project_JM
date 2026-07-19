// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SnailWizardProjectileSpawner.cs
// Summary: Spawns a CurvedProjectile VFX toward the current attack target on AnimEvent_SpawnProjectileEffect.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(AttackExecutor))]
public class SnailWizardProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject magicCirclePrefab;
    [SerializeField] private Transform spawnPoint;
    // #37 - travelDuration must match the gap between AnimEvent_SpawnProjectileEffect
    // and AnimEvent_RaiseHit in the SnailWizard attack animation clip.
    [SerializeField] private float travelDuration = 1f;

    private AttackExecutor _executor;

    private void Awake()
    {
        _executor = GetComponent<AttackExecutor>();
    }

    public void AnimEvent_SpawnMagicCircle()
    {
        Instantiate(magicCirclePrefab, spawnPoint.position, Quaternion.identity);
    }

    public void AnimEvent_SpawnProjectileEffect()
    {
        var targetMB = _executor.Context.Target as MonoBehaviour;
        if (targetMB == null)
        {
            return;
        }

        var go = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        go.GetComponent<CurvedProjectile>().Initialize(targetMB.transform, _executor.HitTransformOffset[0], travelDuration);
    }
}
