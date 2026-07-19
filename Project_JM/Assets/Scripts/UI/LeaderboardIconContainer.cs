// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 28/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardIconContainer.cs
// Summary: ScriptableObject that maps integer icon IDs to Sprites for leaderboard display.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/UI/Leaderboard/Icon Container", fileName = "LeaderboardIconContainer")]
public class LeaderboardIconContainer : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int id;
        public Sprite sprite;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();
    [SerializeField] private Sprite fallbackSprite;

    public Sprite GetSprite(int id)
    {
        foreach (Entry entry in entries)
        {
            if (entry.id == id)
            {
                return entry.sprite;
            }
        }

        return fallbackSprite;
    }

    public IReadOnlyList<Entry> GetEntries() => entries;
}
