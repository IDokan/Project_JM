// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MultiRangedAttack.cs
// Summary: Enemy attack that iterates over the attacker's HitTransformOffset array, positioning
//          each hit via ctx.HitTransformOffset so damage UI and hit burst land at the projectile impact point.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Enemy/Multi Ranged Attack")]
public class MultiRangedAttack : AttackLogic
{
    [SerializeField] private int baseDamage = 36;
    [SerializeField] private float intervalSeconds = 0.5f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        var executor = (ctx.Attacker as MonoBehaviour)?.GetComponent<AttackExecutor>();
        if (executor == null)
        {
            yield break;
        }

        Vector3[] offsets = executor.HitTransformOffset;
        for (int i = 0; i < offsets.Length; i++)
        {
            ctx.HitTransformOffset = offsets[i];
            ctx.Target?.TakeDamage(baseDamage, ctx);

            if (i < offsets.Length - 1)
            {
                yield return GlobalTimeManager.WaitForGlobalSeconds(intervalSeconds);
            }
        }
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(-0.5f, -0.2f, 0f);
    }
}
