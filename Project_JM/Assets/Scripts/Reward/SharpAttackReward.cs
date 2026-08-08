// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SharpAttackReward.cs
// Summary: A reward logic that permanently increases one color's critical damage.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Reward/Sharp Attack Reward")]
public class SharpAttackReward : RewardDefinition
{
    [SerializeField] protected GemColor targetColor = GemColor.Red;
    [SerializeField] protected float criticalDamageIncrease = 0.15f;

    public override GemColor AssociatedColor => targetColor;

    public override IEnumerator Apply(RewardContext context)
    {
        context.DamageMultiplierManager.AddRewardCriticalDamageBonus(targetColor, criticalDamageIncrease);
        FlashGlow(context.Roster.GetCharacterTransform(targetColor));
        yield break;
    }
}
