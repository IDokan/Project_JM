// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BlessingsReward.cs
// Summary: A reward logic that heals the party to full over a short tick and
//          then grants a shield.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Reward/Blessings Reward")]
public class BlessingsReward : RewardDefinition
{
    [SerializeField] protected float shieldPercentage = 0.2f;
    [SerializeField] protected float healDuration = 0.5f;

    public override GemColor AssociatedColor => GemColor.None;

    public override IEnumerator Apply(RewardContext context)
    {
        PlayVfx(context.HpBarVfx);
        yield return HealToFullOverTime(context.PartyStatus, healDuration);
        context.PartyStatus.AddShield(shieldPercentage);
    }

    // Heals CurrentHP up to maxHP over healDuration seconds rather than in one
    // jump. deficit (the missing HP fraction) is measured once up front and
    // spread evenly across the tick, so partial HP heals a proportionally
    // smaller amount - e.g. 70% HP heals 30% of maxHP over healDuration, 50%
    // HP heals 50%. Heal() clamps to maxHP, so the last frame always lands
    // exactly at full HP even though the final step can overshoot slightly.
    private static IEnumerator HealToFullOverTime(CharacterStatus partyStatus, float duration)
    {
        float deficit = 1f - partyStatus.CurrentHP / partyStatus.maxHP;
        if (deficit <= 0f || duration <= 0f)
        {
            partyStatus.Heal(deficit);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            partyStatus.Heal(deficit * Time.deltaTime / duration);
            yield return null;
        }
    }
}
