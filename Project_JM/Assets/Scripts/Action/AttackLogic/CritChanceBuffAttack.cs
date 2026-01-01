// SPDX-License-Identifier: MIT
// Copyright (c) 11/23/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CritChanceBuffAttack.cs
// Summary: A skill that gives damage and critical chancebuff to attacker.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CritChanceBuffAttack", menuName = "JM/Combat/AttackLogic/Ally/Crit Chance Buff Attack")]
public class CritChanceBuffAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 6;
    [SerializeField] protected float buffAmount = 0.2f;
    [SerializeField] protected float duration = 5f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Attacker?.AddBuffCritBonus(buffAmount, duration);
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(1f, -0.1f, 0f);
    }
}
