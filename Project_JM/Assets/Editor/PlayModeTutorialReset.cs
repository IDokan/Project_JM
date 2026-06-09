// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 08/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PlayModeTutorialReset.cs
// Summary: Editor-only hook that sets tutorial progress to Medium and clears the Medium
//          completed flag each time Play mode is entered, for testing Medium tutorials.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEditor;
using UnityEngine;

public static class PlayModeTutorialReset
{
    static PlayModeTutorialReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
        {
            return;
        }

        PlayerPrefs.SetInt("tutorialProgress", 2);       // TutorialProgress.Hard
        PlayerPrefs.SetInt("tutorialCompleted_2", 0);    // IsTutorialCompleted(Hard) = false
        PlayerPrefs.Save();

        Debug.Log("[PlayModeTutorialReset] Tutorial progress → Hard, Hard completed flag → false.");
    }
}
