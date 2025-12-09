// SPDX-License-Identifier: MIT
// Copyright (c) 11/17/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: HealAttack.cs
// Summary: A attack logic that heals.


using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Ally/Heal Attack")]
public class HealAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 6;
    [SerializeField] protected float healPercentage = 0.1f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Attacker.Heal(healPercentage);
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetAttackerMotionOffset()
    {
        return new Vector3(3f, 0f, 0f);
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(1.5f, 0f, 0f);
    }
}
