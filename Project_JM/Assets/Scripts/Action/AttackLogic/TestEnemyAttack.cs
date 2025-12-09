// SPDX-License-Identifier: MIT
// Copyright (c) 11/13/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TestEnemyAttack.cs
// Summary: A test attack logic for enemy.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Enemy/Test Enemy Attack")]
public class TestEnemyAttack : AttackLogic, IHasCooldown
{
    [SerializeField] protected int baseDamage = 36;
    [SerializeField] protected float _cooldown = 5f;
    public float Cooldown => _cooldown;

    public override IEnumerator Execute(AttackContext ctx)
    {
        Debug.Log($"{ctx.Attacker} attacked {ctx.Target} by damage {baseDamage}");
        ctx.Target?.TakeDamage(baseDamage, ctx);
        yield break;
    }

    public override Vector3 GetAttackerMotionOffset()
    {
        return new Vector3(-3f, 0.2f, 0f);
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(-1.5f, -0.2f, 0f);
    }
}
