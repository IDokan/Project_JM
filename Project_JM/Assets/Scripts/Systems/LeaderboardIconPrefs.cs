// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 17/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardIconPrefs.cs
// Summary: Shared PlayerPrefs keys and defaults for the three leaderboard row icon slots.
//          EnsureRandomDefaultsAssigned() seeds any never-chosen slot with a random
//          distinct icon id from the given container, exactly once (see
//          SaveDataManager.Awake()) so first-time players don't all start on the same
//          fixed trio; once a slot's key exists, it is never touched again by this method.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;

public static class LeaderboardIconPrefs
{
    public static readonly string[] Keys = { "LeaderboardIconId_0", "LeaderboardIconId_1", "LeaderboardIconId_2" };
    public static readonly int[] Defaults = { 8, 12, 16 };

    public static void EnsureRandomDefaultsAssigned(LeaderboardIconContainer container)
    {
        bool anyMissing = false;
        for (int i = 0; i < Keys.Length; i++)
        {
            if (!PlayerPrefs.HasKey(Keys[i]))
            {
                anyMissing = true;
                break;
            }
        }
        if (!anyMissing)
        {
            return;
        }

        if (container == null)
        {
            Debug.LogError("[LeaderboardIconPrefs] iconContainer is not assigned; falling back to fixed defaults.");
            for (int i = 0; i < Keys.Length; i++)
            {
                if (!PlayerPrefs.HasKey(Keys[i]))
                {
                    PlayerPrefs.SetInt(Keys[i], Defaults[i]);
                }
            }
            PlayerPrefs.Save();
            return;
        }

        IReadOnlyList<LeaderboardIconContainer.Entry> entries = container.GetEntries();
        List<int> availableIds = new List<int>(entries.Count);
        foreach (LeaderboardIconContainer.Entry entry in entries)
        {
            availableIds.Add(entry.id);
        }

        for (int i = 0; i < Keys.Length; i++)
        {
            if (PlayerPrefs.HasKey(Keys[i]))
            {
                continue;
            }

            if (availableIds.Count == 0)
            {
                PlayerPrefs.SetInt(Keys[i], Defaults[i]);
                continue;
            }

            int pickIndex = Random.Range(0, availableIds.Count);
            PlayerPrefs.SetInt(Keys[i], availableIds[pickIndex]);
            availableIds.RemoveAt(pickIndex);
        }

        PlayerPrefs.Save();
    }
}
