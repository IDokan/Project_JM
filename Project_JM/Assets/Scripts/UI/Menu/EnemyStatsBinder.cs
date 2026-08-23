// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 27/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyStatsBinder.cs
// Summary: Reads lifetime enemy defeat counts from SaveDataManager, writes them
//          into the Enemy tab's text fields, and blacks out undiscovered
//          portraits (defeat count <= 0).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using CharacterEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatsBinder : MonoBehaviour
{
    [System.Serializable]
    public struct EnemyStatRow
    {
        public Image portraitImage;
        public TMP_Text countText;
    }

    [SerializeField] protected EnemyStatRow slimeKingRow;
    [SerializeField] protected EnemyStatRow antGladiatorRow;
    [SerializeField] protected EnemyStatRow mushroomBullyRow;
    [SerializeField] protected EnemyStatRow foxThiefRow;
    [SerializeField] protected EnemyStatRow snailWizardRow;
    [SerializeField] protected EnemyStatRow dandelionToadRow;

    protected void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        RefreshRow(CharacterId.SlimeKing, slimeKingRow);
        RefreshRow(CharacterId.AntGladiator, antGladiatorRow);
        RefreshRow(CharacterId.MushroomBully, mushroomBullyRow);
        RefreshRow(CharacterId.FoxThief, foxThiefRow);
        RefreshRow(CharacterId.SnailWizard, snailWizardRow);
        RefreshRow(CharacterId.DandelionToad, dandelionToadRow);
    }

    protected void RefreshRow(CharacterId id, EnemyStatRow row)
    {
        int defeatCount = SaveDataManager.Instance.GetEnemyDefeatCount(id);
        row.countText.text = defeatCount.ToString();
        row.portraitImage.color = defeatCount <= 0 ? Color.black : Color.white;
    }
}
