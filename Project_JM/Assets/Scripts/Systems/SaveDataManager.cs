// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SaveDataManager.cs
// Summary: Manages all cross-session persistent data via PlayerPrefs.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TutorialEnums;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    private const string KeyProgress = "tutorialProgress";

    public TutorialProgress Progress => (TutorialProgress)PlayerPrefs.GetInt(KeyProgress, 0);

    public void SetMedium()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Medium);
        PlayerPrefs.Save();
    }

    public void SetHard()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Hard);
        PlayerPrefs.Save();
    }

    public void SetChallenge()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Challenge);
        PlayerPrefs.Save();
    }

    public void ResetToEasy()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Easy);
        PlayerPrefs.Save();
    }

    public void ResetToMedium()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Medium);
        PlayerPrefs.Save();
    }

    // No ResetToChallenge — Challenge unlocks the ranking system and is permanent.

    private const string KeyTutorialCompleted = "tutorialCompleted_";

    public bool IsTutorialCompleted(TutorialProgress level)
    {
        return PlayerPrefs.GetInt(KeyTutorialCompleted + (int)level, 0) == 1;
    }

    public void SetTutorialCompleted(TutorialProgress level)
    {
        PlayerPrefs.SetInt(KeyTutorialCompleted + (int)level, 1);
        PlayerPrefs.Save();
    }
}
