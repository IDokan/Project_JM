// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 27/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ScoreStatsBinder.cs
// Summary: Reads the lifetime best score from SaveDataManager, displays it,
//          swaps the milestone image once the score crosses a tier threshold,
//          and displays how many times each reward has been chosen.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using RewardEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStatsBinder : MonoBehaviour
{
    [System.Serializable]
    public struct ScoreMilestone
    {
        public int scoreThreshold;
        public Sprite milestoneSprite;
    }

    [SerializeField] protected TMP_Text bestScoreText;
    [SerializeField] protected Image milestoneImage;

    // Redeclared here rather than reusing DefeatedTransitionController's
    // Bronze/Silver/Gold constants — those are Steam-achievement-domain names,
    // this is a separate UI-display concern. Ordered ascending; add entries
    // (e.g. 35000/40000) here to introduce new tiers without touching logic.
    [SerializeField] protected ScoreMilestone[] milestonesAscending;

    [System.Serializable]
    public struct RewardRecordEntry
    {
        public RewardId rewardId;
        public TMP_Text countText;
    }

    // How many times each reward has been chosen, lifetime - see
    // SaveDataManager.GetRewardPickCount and RewardManager.ChooseReward.
    [SerializeField] protected RewardRecordEntry[] rewardRecords;

    // Icon-grid history of the specific run that set the current best score, in
    // pick order - see SaveDataManager.GetBestScoreRewardHistory. Distinct from
    // rewardRecords above, which is a lifetime per-type tally.
    [SerializeField] protected RewardBook rewardBook;
    [SerializeField] protected GameObject rewardIconHolderPrefab;
    [SerializeField] protected GridLayoutGroup rewardHistoryGridLayoutGroup;

    // How many full rows/columns of the grid the reward history is allowed to
    // fill before extra picks are dropped - see PopulateRewardHistory.
    [SerializeField] protected int maxRow = 2;

    protected void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int bestScore = SaveDataManager.Instance.GetBestScore();
        bestScoreText.text = bestScore.ToString();

        for (int i = milestonesAscending.Length - 1; i >= 0; --i)
        {
            if (bestScore >= milestonesAscending[i].scoreThreshold)
            {
                milestoneImage.sprite = milestonesAscending[i].milestoneSprite;
                break;
            }
        }
        // Below every threshold: leave whatever sprite is already authored in the prefab.

        for (int i = 0; i < rewardRecords.Length; i++)
        {
            rewardRecords[i].countText.text = SaveDataManager.Instance.GetRewardPickCount(rewardRecords[i].rewardId).ToString();
        }

        PopulateRewardHistory();
    }

    protected void PopulateRewardHistory()
    {
        Transform container = rewardHistoryGridLayoutGroup.transform;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }

        RewardId[] rewardHistory = SaveDataManager.Instance.GetBestScoreRewardHistory();

        // Caps the grid to maxRow full rows/columns instead of growing
        // unbounded for a very reward-heavy run - reads the live Constraint
        // Count rather than a separately-authored value, so it can't drift
        // out of sync with however the grid is actually configured.
        int maxRewardHistoryIcons = rewardHistoryGridLayoutGroup.constraintCount * maxRow;
        int count = Mathf.Min(rewardHistory.Length, maxRewardHistoryIcons);

        for (int i = 0; i < count; i++)
        {
            RewardDefinition reward = rewardBook.GetRewardDefinition(rewardHistory[i]);
            if (reward == null)
            {
                continue;
            }

            GameObject instance = Instantiate(rewardIconHolderPrefab, container);
            Image iconImage = instance.GetComponentInChildren<Image>();
            iconImage.sprite = reward.MiniIcon;
            iconImage.SetNativeSize();
        }
    }
}
