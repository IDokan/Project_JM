// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/24/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TimeStopAttack.cs
// Summary: An attack logic that gives damage and stop time. Chrono Pulse.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Ally/Time Stop Attack")]
public class TimeStopAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 9;

    [SerializeField] protected float timeScaler = 0.5f;
    [SerializeField] protected float duration = 5;

    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Target?.TakeDamage(baseDamage, ctx);
        GlobalTimeManager.Instance.SetTimer(timeScaler, duration);

        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(2f, 0f, 0f);
    }
}
