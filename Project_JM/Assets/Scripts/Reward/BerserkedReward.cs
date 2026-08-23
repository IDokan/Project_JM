// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BerserkedReward.cs
// Summary: A reward logic that cuts current HP to a quarter in exchange for
//          a permanent attack power increase across all four colors.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Reward/Berserked Reward")]
public class BerserkedReward : RewardDefinition
{
    [SerializeField] protected float attackPowerIncrease = 0.1f;
    [SerializeField] protected float currentHPDivisor = 4f;
    [SerializeField] protected float stepDelay = 0.25f;

    public override GemColor AssociatedColor => GemColor.None;

    public override IEnumerator Apply(RewardContext context)
    {
        for (int i = 0; i < GemColorUtility.PlayableGemColor.Length; i++)
        {
            context.DamageMultiplierManager.AddRewardAttackPowerBonus(GemColorUtility.PlayableGemColor[i], attackPowerIncrease);
        }

        // Total loss is fixed up front (quarter of current HP), then spread as
        // equal flat chunks across each party member's staggered hit feedback.
        CharacterCombatant[] party = context.Roster.GetAll();
        Transform[] transforms = context.Roster.GetAllCharacterTransforms();
        float totalLoss = context.PartyStatus.CurrentHP - context.PartyStatus.CurrentHP / currentHPDivisor;
        float perStepLoss = totalLoss / party.Length;

        for (int i = 0; i < party.Length; i++)
        {
            float previousHP = context.PartyStatus.CurrentHP;
            context.PartyStatus.ReduceCurrentHPByAmount(perStepLoss);
            int lostHP = Mathf.RoundToInt(previousHP - context.PartyStatus.CurrentHP);

            party[i]?.PlayDamageFeedback(lostHP);
            FlashGlow(transforms[i]);
            SpawnLevelUpVfx(transforms[i]);

            if (i < party.Length - 1)
            {
                yield return GlobalTimeManager.WaitForGlobalSeconds(stepDelay);
            }
        }
    }
}
