// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/24/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: StunAttack.cs
// Summary: A skill that gives damage and stun target enemy.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Stun Attack", menuName = "JM/Combat/AttackLogic/Ally/Stun Attack")]
public class StunAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 9;
    [SerializeField] protected float stunDuration = 5f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        if (ctx.Attacker.IsDead)
        {
            yield break;
        }

        if (ctx.Target is MonoBehaviour targetMB &&
            targetMB.TryGetComponent<EnemyAttackBehaviour>(out var enemy))
        {
            enemy.Stun(stunDuration);
        }
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(1f, -0.1f, 0f);
    }
}
