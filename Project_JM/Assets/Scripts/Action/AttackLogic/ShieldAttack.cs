// SPDX-License-Identifier: MIT
// Copyright (c) 11/17/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ShieldAttack.cs
// Summary: An attack logic that adds shield.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Ally/Shield Attack")]
public class ShieldAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 6;
    [SerializeField] protected float shieldPercentage = 0.15f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Attacker?.AddShield(shieldPercentage);
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(0f, -1.5f, 0f);
    }
}
