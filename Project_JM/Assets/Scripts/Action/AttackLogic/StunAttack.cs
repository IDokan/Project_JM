// SPDX-License-Identifier: MIT
// Copyright (c) 11/24/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: StunAttack.cs
// Summary: A skill that gives damage and stun target enemy.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Stun Attack", menuName = "JM/Combat/AttackLogic/Ally/Stun Attack")]
public class StunAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 9;
    [SerializeField] protected float stunDuration = 5f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        if (ctx.Target is MonoBehaviour targetMB &&
            targetMB.TryGetComponent<EnemyAttackBehaviour>(out var enemy))
        {
            enemy.Stun(stunDuration);
        }
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetAttackerMotionOffset()
    {
        return new Vector3(3f, 2f, 0f);
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(2f, -0.5f, 0f);
    }
}
