// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardManager.cs
// Summary: Rolls a random reward offer and applies whichever reward the player
//          chooses. Called into by RewardOfferUI (RollOffer, ChooseReward).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using RewardEnums;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [SerializeField] protected RewardBook rewardBook;
    [SerializeField] protected DamageMultiplierManager damageMultiplierManager;
    [SerializeField] protected CharacterStatus partyStatus;
    [SerializeField] protected PartyRoster partyRoster;
    [SerializeField] protected ComboManager comboManager;
    [SerializeField] protected GameProgressManager gameProgressManager;
    [SerializeField] protected int offerCount = 3;

    [Header("Hint Conditions")]
    [SerializeField, Range(0f, 1f)] protected float blessingsHintHPRatio = 0.4f;
    [SerializeField] protected int berserkedHintMaxEnemyDefeated = 3;

    // Landing VFX for colorless rewards with no character target — see
    // RewardContext.HpBarVfx/ComboBarVfx/ComboIconVfx and RewardDefinition.PlayVfx.
    [SerializeField] protected ParticleSystem hpBarVfx;
    [SerializeField] protected ParticleSystem comboBarVfx;
    [SerializeField] protected ParticleSystem comboIconVfx;

    protected RewardDefinition[] _currentOffer;
    public IReadOnlyList<RewardDefinition> CurrentOffer => _currentOffer;

    // One mini-icon per reward chosen so far this run, in pick order; see
    // RewardDefinition.MiniIcon and RewardHistoryUI.
    protected readonly List<Sprite> _chosenIcons = new List<Sprite>();
    public IReadOnlyList<Sprite> ChosenIcons => _chosenIcons;

    // Parallel to _chosenIcons but by id rather than sprite, so a run's pick
    // order can be persisted (see SaveDataManager.SetBestScoreRewardHistory)
    // without storing Sprite references in PlayerPrefs.
    protected readonly List<RewardId> _chosenRewardIds = new List<RewardId>();
    public IReadOnlyList<RewardId> ChosenRewardIds => _chosenRewardIds;

    public RewardDefinition[] RollOffer()
    {
        _currentOffer = rewardBook.GetRandomRewards(offerCount);
        return _currentOffer;
    }

    // Whether current run state (not the incoming enemy's color — see
    // RewardOfferUI.SetColorMatchParticle for that check) makes this reward
    // worth calling out: Blessings while the party is low on HP, Berserked
    // while still early (few enemies defeated yet, so its temporary HP cost
    // is safer to take).
    public bool IsGameStateRecommended(RewardDefinition reward)
    {
        switch (reward.Id)
        {
            case RewardId.Blessings:
                return partyStatus.CurrentHP <= partyStatus.maxHP * blessingsHintHPRatio;

            case RewardId.Berserked:
                return gameProgressManager.NumEnemyDefeated <= berserkedHintMaxEnemyDefeated;

            default:
                return false;
        }
    }

    public void ChooseReward(RewardDefinition reward)
    {
        _chosenIcons.Add(reward.MiniIcon);
        _chosenRewardIds.Add(reward.Id);
        SaveDataManager.Instance.IncrementRewardPickCount(reward.Id);

        StartCoroutine(reward.Apply(new RewardContext
        {
            DamageMultiplierManager = damageMultiplierManager,
            PartyStatus = partyStatus,
            Roster = partyRoster,
            ComboManager = comboManager,
            HpBarVfx = hpBarVfx,
            ComboBarVfx = comboBarVfx,
            ComboIconVfx = comboIconVfx
        }));
    }
}
