// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DandelionBubbleSpawner.cs
// Summary: Spawns a DandelionBubbleProjectile per HitTransformOffset entry toward the current
//          attack target on AnimEvent_SpawnBubble, with a configurable interval between each.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AttackExecutor))]
public class DandelionBubbleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float flyDuration = 1f;
    [SerializeField] private float glideDuration = 0.5f;
    [SerializeField] private float spawnInterval = 0.2f;

    private AttackExecutor _executor;

    private void Awake()
    {
        _executor = GetComponent<AttackExecutor>();
    }

    public void AnimEvent_SpawnBubble()
    {
        var targetMB = _executor.Context.Target as MonoBehaviour;
        if (targetMB == null)
        {
            return;
        }

        StartCoroutine(SpawnRoutine(targetMB.transform));
    }

    private IEnumerator SpawnRoutine(Transform target)
    {
        foreach (Vector3 offset in _executor.HitTransformOffset)
        {
            var go = Instantiate(bubblePrefab, spawnPoint.position, Quaternion.identity);
            go.GetComponent<DandelionBubbleProjectile>().Initialize(target, offset, flyDuration, glideDuration);
            yield return GlobalTimeManager.WaitForGlobalSeconds(spawnInterval);
        }
    }
}
