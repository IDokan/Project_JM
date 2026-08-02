// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PowerUpReward.cs
// Summary: A reward logic that permanently increases one color's attack power.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Reward/PowerUp Reward")]
public class PowerUpReward : RewardDefinition
{
    [SerializeField] protected GemColor targetColor = GemColor.Red;
    [SerializeField] protected float attackPowerIncrease = 0.1f;

    public override GemColor AssociatedColor => targetColor;

    public override IEnumerator Apply(RewardContext context)
    {
        context.DamageMultiplierManager.AddRewardAttackPowerBonus(targetColor, attackPowerIncrease);
        yield break;
    }
}
