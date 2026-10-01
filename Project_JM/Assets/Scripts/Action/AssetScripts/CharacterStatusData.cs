// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/10/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStatusData.cs
// Summary: A scriptable object for containing status data.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using CharacterEnums;
using UnityEngine;
using UnityEngine.Localization;

public struct StatusMultiplier
{
    public float HPMultiplier;
}

[CreateAssetMenu(fileName = "CharacterStatusData", menuName = "JM/Data/CharacterStatusData")]
public class CharacterStatusData : ScriptableObject
{
    public CharacterId characterId;
    public Sprite icon;
    public float baseHP = 100f;
    public float baseCriticalChance = 0f;           // [0, 100] -> 100 means 100% critical chance
    public float baseCriticalDamage = 1.5f;

    [Header("Tooltip")]
    [SerializeField] private LocalizedString tooltipTitle = new LocalizedString();
    [SerializeField] private LocalizedString tooltipDescription = new LocalizedString();

    public LocalizedString TooltipTitle => tooltipTitle;
    public LocalizedString TooltipDescription => tooltipDescription;
}
