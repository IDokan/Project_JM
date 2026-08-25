// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/23/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CritChanceBuffAttack.cs
// Summary: A skill that gives damage and critical chancebuff to attacker.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CritChanceBuffAttack", menuName = "JM/Combat/AttackLogic/Ally/Crit Chance Buff Attack")]
public class CritChanceBuffAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 6;
    [SerializeField] protected float buffAmount = 0.2f;
    [SerializeField] protected float duration = 5f;
    [SerializeField] protected GameObject impactPrefab;

    public override IEnumerator Execute(AttackContext ctx)
    {
        if (ctx.Attacker.IsDead)
        {
            yield break;
        }

        ctx.ImpactAttachPrefab = impactPrefab;

        float duplicateRatio = 0f;
        if (ctx.Attacker is CharacterCombatant attacker && buffAmount > 0f)
        {
            float room = Mathf.Max(0f, 1f - attacker.Status.CriticalChance);
            float wasted = Mathf.Max(0f, buffAmount - room);
            duplicateRatio = wasted / buffAmount;
        }
        ctx.Attacker?.AddBuffCritBonus(buffAmount, duration);
        ctx.Target?.TakeDamage(baseDamage * (1f + duplicateRatio), ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(0.5f, -0.02f, 0f);
    }
}
