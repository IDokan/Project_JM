// SPDX-License-Identifier: MIT
// Copyright (c) 11/10/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStatusData.cs
// Summary: A scriptable object for containing status data.

using UnityEngine;

public struct StatusMultiplier
{
    public float HPMultiplier;
}

[CreateAssetMenu(fileName = "CharacterStatusData", menuName = "JM/Data/CharacterStatusData")]
public class CharacterStatusData : ScriptableObject
{
    public string characterName;
    public float baseHP = 100f;
    public float baseCriticalChance = 0f;           // [0, 100] -> 100 means 100% critical chance
    public float baseCriticalDamage = 1.5f;
}
