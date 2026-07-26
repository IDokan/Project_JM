// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 27/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ScoreStatsBinder.cs
// Summary: Reads the lifetime best score from SaveDataManager, displays it,
//          and swaps the milestone image once the score crosses a tier threshold.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

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
                return;
            }
        }
        // Below every threshold: leave whatever sprite is already authored in the prefab.
    }
}
