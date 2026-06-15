// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/23/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CritDamageBuffAttack.cs
// Summary: A skill that gives damage and critical damage buff to attacker.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CritDamageBuffAttack", menuName = "JM/Combat/AttackLogic/Ally/Crit Damage Buff Attack")]
public class CritDamageBuffAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 9;
    [SerializeField] protected float buffAmount = 0.3f;
    [SerializeField] protected GameObject impactPrefab;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.ImpactAttachPrefab = impactPrefab;

        ctx.Attacker?.AddBuffCritDamage(buffAmount);
        ctx.Target?.TakeDamage(baseDamage, ctx);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(0.75f, -0.04f, 0f);
    }
}
