// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FortifyReward.cs
// Summary: A reward logic that permanently increases the party's HP.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Reward/Fortify Reward")]
public class FortifyReward : RewardDefinition
{
    [SerializeField] protected float hpIncrease = 60f;

    public override GemColor AssociatedColor => GemColor.None;

    public override IEnumerator Apply(RewardContext context)
    {
        context.PartyStatus.AddRewardHPBonus(hpIncrease);
        yield break;
    }
}
