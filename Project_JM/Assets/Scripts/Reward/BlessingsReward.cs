// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BlessingsReward.cs
// Summary: A reward logic that fully heals the party and grants a shield.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Reward/Blessings Reward")]
public class BlessingsReward : RewardDefinition
{
    [SerializeField] protected float shieldPercentage = 0.2f;

    public override GemColor AssociatedColor => GemColor.None;

    public override IEnumerator Apply(RewardContext context)
    {
        context.PartyStatus.Heal(1f);
        context.PartyStatus.AddShield(shieldPercentage);
        PlayVfx(context.HpBarVfx);
        yield break;
    }
}
