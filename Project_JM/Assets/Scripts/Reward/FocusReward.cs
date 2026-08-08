// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FocusReward.cs
// Summary: A reward logic that permanently increases the combo reset time.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Reward/Focus Reward")]
public class FocusReward : RewardDefinition
{
    [SerializeField] protected float comboResetTimeIncrease = 1f;

    public override GemColor AssociatedColor => GemColor.None;

    public override IEnumerator Apply(RewardContext context)
    {
        context.ComboManager.AddComboResetTimeBonus(comboResetTimeIncrease);
        PlayVfx(context.ComboBarVfx);
        yield break;
    }
}
