// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 27/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStatsBinder.cs
// Summary: Reads lifetime damage dealt by each party class from SaveDataManager
//          and writes them into the Character tab's text fields.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using TMPro;
using UnityEngine;

public class CharacterStatsBinder : MonoBehaviour
{
    [SerializeField] protected TMP_Text knightDamageText;
    [SerializeField] protected TMP_Text bowmanDamageText;
    [SerializeField] protected TMP_Text mageDamageText;
    [SerializeField] protected TMP_Text clericDamageText;

    protected void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        knightDamageText.text = SaveDataManager.Instance.GetDamageDealt(GemColor.Red).ToString();
        bowmanDamageText.text = SaveDataManager.Instance.GetDamageDealt(GemColor.Yellow).ToString();
        mageDamageText.text = SaveDataManager.Instance.GetDamageDealt(GemColor.Blue).ToString();
        clericDamageText.text = SaveDataManager.Instance.GetDamageDealt(GemColor.Green).ToString();
    }
}
