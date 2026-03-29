// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/13/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TestEnemyAttack.cs
// Summary: A test attack logic for enemy.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Enemy/Test Enemy Attack")]
public class TestEnemyAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 36;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Target?.TakeDamage(baseDamage, ctx);
        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(-1.5f, -0.2f, 0f);
    }
}
