// SPDX-License-Identifier: MIT
// Copyright (c) 11/15/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DelayAttack.cs
// Summary: A attack logic that delays enemy attack timer.


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
        ctx.Target?.TakeDamage(baseDamage, ctx.DamageMultiplierManager);

        yield break;
    }

    public override Vector3 GetAttackerMotionOffset()
    {
        return new Vector3(3f, 6f, 0f);
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(1.5f, -1.5f, 0f);
    }
}
