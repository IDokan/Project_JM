// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SaveDataManager.cs
// Summary: Manages all cross-session persistent data via PlayerPrefs.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    private const string KeyEasyPassed = "isEasyPassed";
    private const string KeyMediumPassed = "isMediumPassed";

    public bool IsEasyPassed => PlayerPrefs.GetInt(KeyEasyPassed, 0) == 1;
    public bool IsMediumPassed => PlayerPrefs.GetInt(KeyMediumPassed, 0) == 1;
    // IsAllPassed must be checked before IsEasyPassed in if-else chains: both-on is a subset of easy-on,
    // so reversing the order would always take the easy branch even when medium should be reset.
    public bool IsAllPassed => IsEasyPassed && IsMediumPassed;

    public void SetEasyPassed()
    {
        PlayerPrefs.SetInt(KeyEasyPassed, 1);
        // Save() flushes to disk (Windows registry) — acceptable here because this flag
        // is set at most once per lifetime of the game. If any future data needs to be
        // written frequently, remove Save() from the setter and call it only in OnApplicationQuit.
        PlayerPrefs.Save();
    }

    public void SetMediumPassed()
    {
        PlayerPrefs.SetInt(KeyEasyPassed, 1);
        PlayerPrefs.SetInt(KeyMediumPassed, 1);
        // Same note as SetEasyPassed: disk flush is fine for a once-ever write.
        // Move Save() to OnApplicationQuit if write frequency increases.
        PlayerPrefs.Save();
    }

    public void ResetEasyPassed()
    {
        PlayerPrefs.SetInt(KeyEasyPassed, 0);
        PlayerPrefs.Save();
    }

    public void ResetMediumPassed()
    {
        PlayerPrefs.SetInt(KeyMediumPassed, 0);
        PlayerPrefs.Save();
    }
}
