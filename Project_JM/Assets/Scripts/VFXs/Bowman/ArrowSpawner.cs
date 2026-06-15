// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 15/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ArrowSpawner.cs
// Summary: Spawns an ArrowProjectile on AnimEvent_FireT3/T4/T5Arrow and fires it in the
//          Bowman's local forward direction at a constant speed. Each animation clip calls
//          the matching tier method so the tier is encoded at the call site, not tracked as state.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using MatchEnums;
using UnityEngine;

public class ArrowSpawner : AbstractAnimEventPrefabSpawner<ArrowProjectile>
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 2f;

    private AttackExecutor _attackExecutor;

    private void Awake()
    {
        _attackExecutor = GetComponent<AttackExecutor>();
    }

    public void AnimEvent_FireT3Arrow() => FireArrow(MatchTier.Three);
    public void AnimEvent_FireT4Arrow() => FireArrow(MatchTier.Four);
    public void AnimEvent_FireT5Arrow() => FireArrow(MatchTier.Five);

    private void FireArrow(MatchTier tier)
    {
        ArrowProjectile arrow = Spawn();
        if (arrow == null)
        {
            return;
        }

        Vector2 forward = transform.right;
        arrow.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
        arrow.Init(_attackExecutor, tier, forward, speed, lifeTime);
    }
}
