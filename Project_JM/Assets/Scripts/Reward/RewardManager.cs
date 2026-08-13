// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardManager.cs
// Summary: Rolls a random reward offer and applies whichever reward the player
//          chooses. Called into by RewardOfferUI (RollOffer, ChooseReward).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [SerializeField] protected RewardBook rewardBook;
    [SerializeField] protected DamageMultiplierManager damageMultiplierManager;
    [SerializeField] protected CharacterStatus partyStatus;
    [SerializeField] protected PartyRoster partyRoster;
    [SerializeField] protected ComboManager comboManager;
    [SerializeField] protected int offerCount = 3;

    // Landing VFX for colorless rewards with no character target — see
    // RewardContext.HpBarVfx/ComboBarVfx and RewardDefinition.PlayVfx.
    [SerializeField] protected ParticleSystem hpBarVfx;
    [SerializeField] protected ParticleSystem comboBarVfx;

    protected RewardDefinition[] _currentOffer;
    public IReadOnlyList<RewardDefinition> CurrentOffer => _currentOffer;

    // One mini-icon per reward chosen so far this run, in pick order; see
    // RewardDefinition.MiniIcon and RewardHistoryUI.
    protected readonly List<Sprite> _chosenIcons = new List<Sprite>();
    public IReadOnlyList<Sprite> ChosenIcons => _chosenIcons;

    public RewardDefinition[] RollOffer()
    {
        _currentOffer = rewardBook.GetRandomRewards(offerCount);
        return _currentOffer;
    }

    public void ChooseReward(RewardDefinition reward)
    {
        _chosenIcons.Add(reward.MiniIcon);

        StartCoroutine(reward.Apply(new RewardContext
        {
            DamageMultiplierManager = damageMultiplierManager,
            PartyStatus = partyStatus,
            Roster = partyRoster,
            ComboManager = comboManager,
            HpBarVfx = hpBarVfx,
            ComboBarVfx = comboBarVfx
        }));
    }
}
