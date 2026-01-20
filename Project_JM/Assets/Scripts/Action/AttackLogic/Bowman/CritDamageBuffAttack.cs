// SPDX-License-Identifier: MIT
// Copyright (c) 11/23/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CritDamageBuffAttack.cs
// Summary: A skill that gives damage and critical damage buff to attacker.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CritDamageBuffAttack", menuName = "JM/Combat/AttackLogic/Ally/Crit Damage Buff Attack")]
public class CritDamageBuffAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 9;
    [SerializeField] protected float buffAmount = 0.3f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Attacker?.AddBuffCritDamage(buffAmount);
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(1.5f, -0.2f, 0f);
    }
}
