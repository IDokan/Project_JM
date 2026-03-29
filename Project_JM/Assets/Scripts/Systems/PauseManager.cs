// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/25/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseManager.cs
// Summary: A script to manage pause menu.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] protected PauseMenu pauseMenu;


    public void Pause()
    {
        pauseMenu.Show();
    }

    public void Resume()
    {
        pauseMenu.Hide();
    }
}
