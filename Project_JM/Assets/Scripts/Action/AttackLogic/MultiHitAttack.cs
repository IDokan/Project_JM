// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MultiHitAttack.cs
// Summary: Enemy attack that deals damage immediately then repeats after a fixed interval for a configurable hit count.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Enemy/Multi Hit Attack")]
public class MultiHitAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 36;
    [SerializeField] private int hitCount = 5;
    [SerializeField] private float intervalSeconds = 0.1f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Target?.TakeDamage(baseDamage, ctx);

        for (int i = 1; i < hitCount; i++)
        {
            yield return GlobalTimeManager.WaitForGlobalSeconds(intervalSeconds);
            ctx.Target?.TakeDamage(baseDamage, ctx);
        }
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(-0.5f, -0.2f, 0f);
    }
}
