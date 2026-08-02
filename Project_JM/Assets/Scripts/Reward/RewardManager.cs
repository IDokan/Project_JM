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

    protected RewardDefinition[] _currentOffer;
    public IReadOnlyList<RewardDefinition> CurrentOffer => _currentOffer;

    public RewardDefinition[] RollOffer()
    {
        _currentOffer = rewardBook.GetRandomRewards(offerCount);
        return _currentOffer;
    }

    public void ChooseReward(RewardDefinition reward)
    {
        StartCoroutine(reward.Apply(new RewardContext
        {
            DamageMultiplierManager = damageMultiplierManager,
            PartyStatus = partyStatus,
            Roster = partyRoster,
            ComboManager = comboManager
        }));
    }
}
