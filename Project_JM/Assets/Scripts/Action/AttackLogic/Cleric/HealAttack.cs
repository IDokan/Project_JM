// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/17/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: HealAttack.cs
// Summary: A attack logic that heals.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


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

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(0.75f, 0f, 0f);
    }
}
