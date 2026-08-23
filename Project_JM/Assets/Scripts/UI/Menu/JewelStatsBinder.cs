// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 26/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JewelStatsBinder.cs
// Summary: Reads lifetime jewel match counts and the max combo record from
//          SaveDataManager and writes them into the Jewel tab's text fields.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using MatchEnums;
using TMPro;
using UnityEngine;

public class JewelStatsBinder : MonoBehaviour
{
    [System.Serializable]
    public struct ColorStatRow
    {
        public TMP_Text tierThreeCountText;
        public TMP_Text tierFourCountText;
        public TMP_Text tierFiveCountText;
    }

    [SerializeField] protected ColorStatRow redRow;
    [SerializeField] protected ColorStatRow greenRow;
    [SerializeField] protected ColorStatRow blueRow;
    [SerializeField] protected ColorStatRow yellowRow;
    [SerializeField] protected TMP_Text noneCountText;
    [SerializeField] protected TMP_Text maxComboText;

    protected void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        RefreshRow(GemColor.Red, redRow);
        RefreshRow(GemColor.Green, greenRow);
        RefreshRow(GemColor.Blue, blueRow);
        RefreshRow(GemColor.Yellow, yellowRow);

        noneCountText.text = SaveDataManager.Instance.GetJewelMatchCount(GemColor.None, default).ToString();
        maxComboText.text = SaveDataManager.Instance.GetMaxCombo().ToString();
    }

    protected void RefreshRow(GemColor color, ColorStatRow row)
    {
        row.tierThreeCountText.text = SaveDataManager.Instance.GetJewelMatchCount(color, MatchTier.Three).ToString();
        row.tierFourCountText.text = SaveDataManager.Instance.GetJewelMatchCount(color, MatchTier.Four).ToString();
        row.tierFiveCountText.text = SaveDataManager.Instance.GetJewelMatchCount(color, MatchTier.Five).ToString();
    }
}
