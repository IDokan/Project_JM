// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/17/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ShieldAttack.cs
// Summary: An attack logic that adds shield.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Ally/Shield Attack")]
public class ShieldAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 6;
    [SerializeField] protected float shieldPercentage = 0.15f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        float shieldRatio = 0f;
        if (ctx.Attacker is CharacterCombatant attacker && attacker.Status.maxHP > 0f)
        {
            shieldRatio = attacker.Status.Shield / attacker.Status.maxHP;
        }

        ctx.Attacker?.AddShield(shieldPercentage);
        ctx.Target?.TakeDamage(baseDamage * (1f + shieldRatio), ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(0f, -0.3f, 0f);
    }
}
