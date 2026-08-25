// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MultiMeleeAttack.cs
// Summary: Enemy attack that deals damage immediately then repeats after a fixed interval for a configurable hit count.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Enemy/Multi Melee Attack")]
public class MultiMeleeAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 36;
    [SerializeField] private float intervalSeconds = 0.1f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        var executor = (ctx.Attacker as MonoBehaviour)?.GetComponent<AttackExecutor>();
        if (executor == null)
        {
            yield break;
        }

        Transform[] perHitTransforms = executor.PerHitTransforms;
        if (perHitTransforms == null || perHitTransforms.Length == 0)
        {
            Debug.LogError($"[MultiMeleeAttack] PerHitTransforms is null or empty on {ctx.Attacker}.");
            yield break;
        }

        for (int i = 0; i < perHitTransforms.Length; i++)
        {
            if (ctx.Attacker.IsDead)
            {
                yield break;
            }

            ctx.HitTransform = perHitTransforms[i];
            ctx.Target?.TakeDamage(baseDamage, ctx);

            if (i < perHitTransforms.Length - 1)
            {
                yield return GlobalTimeManager.WaitForGlobalSeconds(intervalSeconds);
            }
        }
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(-0.5f, -0.2f, 0f);
    }
}
