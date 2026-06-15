// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/15/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DelayAttack.cs
// Summary: A attack logic that delays enemy attack timer.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Ally/Delay Attack")]
public class DelayAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 6;
    [SerializeField] protected float delayAmount = 5;

    public override IEnumerator Execute(AttackContext ctx)
    {
        if (ctx.Target is MonoBehaviour targetMB &&
            targetMB.TryGetComponent<EnemyAttackBehaviour>(out var enemy))
        {
            enemy.DelayAttack(delayAmount);
        }
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(0.75f, -0.3f, 0f);
    }
}
