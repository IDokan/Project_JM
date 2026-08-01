// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardTransitionController.cs
// Summary: A script to manage reward transition logic.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.
//                      Reward transition conducts below tasks when an enemy is defeated:
//                                          1. Cover the gem board without refilling it.
//                                          2. Preview the next enemy and show its alert.
//                                          3. Show the damage record breakdown for the enemy that just died.
//                                          4. Play the defeated enemy's exit animation.
//                      It then waits for the player to confirm a reward before
//                      handing off to the middle transition.

using UnityEngine;

public class RewardTransitionController : TransitionController
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;
    [SerializeField] protected RewardChosenEventChannel rewardChosenEventChannel;

    protected void OnEnable()
    {
        characterDeathEventChannel.OnRaised += OnAnyoneDied;
        rewardChosenEventChannel.OnRaised += OnRewardChosen;
    }

    protected void OnDisable()
    {
        characterDeathEventChannel.OnRaised -= OnAnyoneDied;
        rewardChosenEventChannel.OnRaised -= OnRewardChosen;
    }

    protected void OnAnyoneDied(CharacterStatus stat)
    {
        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            RequestTransitionStart(BeginRewardTransition);
        }
    }

    protected void BeginRewardTransition()
    {
        transitionEventChannel.Raise(TransitionPhase.RewardTransitionStarts);
    }

    protected void OnRewardChosen()
    {
        transitionEventChannel.Raise(TransitionPhase.RewardTransitionEnd);
        CompleteTransition();
    }
}
