// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/18/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageBuffAttack.cs
// Summary: A skill that gives damage and damage buff to attacker.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageBuffAttack", menuName = "JM/Combat/AttackLogic/Ally/DamageBuffAttack")]
public class DamageBuffAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 9;
    [SerializeField] protected float buffAmount = 1.2f;


    public override IEnumerator Execute(AttackContext ctx)
    {
        if (ctx.Attacker.IsDead)
        {
            yield break;
        }

        ctx.DamageMultiplierManager.AddTimedBonus(buffAmount);
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(0.75f, 0f, 0f);
    }
}
