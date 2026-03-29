// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackBook.cs
// Summary: A scriptable object to contain attack logic data.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using GemEnums;
using MatchEnums;

[System.Serializable]
public struct AttackKey
{
    public GemColor color;
    public MatchTier tier;
}

[System.Serializable]
public struct AttackEntry
{
    public AttackKey key;
    public AttackLogic logic;
}

[CreateAssetMenu(fileName = "AttackBook", menuName = "JM/Data/Attack Book")]
public class AttackBook : ScriptableObject
{
    [SerializeField] protected AttackEntry[] entries;

    public AttackLogic Get(GemColor color, MatchTier tier)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].key.color == color && entries[i].key.tier == tier)
            {
                return entries[i].logic;
            }
        }

        return null;
    }
}
