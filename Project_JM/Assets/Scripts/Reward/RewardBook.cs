// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardBook.cs
// Summary: A scriptable object that holds the full reward pool and picks random offers from it.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using RewardEnums;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardBook", menuName = "JM/Data/RewardBook")]
public class RewardBook : ScriptableObject
{
    [SerializeField] protected RewardDefinition[] rewards;

    // Used by ScoreStatsBinder to resolve a persisted RewardId back to its
    // MiniIcon for the best-run reward history grid.
    public RewardDefinition GetRewardDefinition(RewardId id)
    {
        foreach (RewardDefinition reward in rewards)
        {
            if (reward.Id == id)
            {
                return reward;
            }
        }
        return null;
    }

    // Picks up to `count` unique rewards from the pool. Not board-related, so
    // this uses UnityEngine.Random rather than GlobalRNG.
    public RewardDefinition[] GetRandomRewards(int count)
    {
        int pickCount = Mathf.Min(count, rewards.Length);
        RewardDefinition[] pool = (RewardDefinition[])rewards.Clone();
        RewardDefinition[] result = new RewardDefinition[pickCount];

        for (int i = 0; i < pickCount; i++)
        {
            int pick = Random.Range(i, pool.Length);
            (pool[i], pool[pick]) = (pool[pick], pool[i]);
            result[i] = pool[i];
        }

        return result;
    }
}
