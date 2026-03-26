// SPDX-License-Identifier: MIT
// Copyright (c) 03/25/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseManager.cs
// Summary: A script to manage pause menu.

using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] protected PauseMenu pauseMenu;


    public void Pause()
    {
        GlobalTimeManager.Instance.PauseTimeScale();
        pauseMenu.gameObject.SetActive(true);
    }

    public void Resume()
    {
        GlobalTimeManager.Instance.RestoreTimeScaleFromPause();
        pauseMenu.gameObject.SetActive(false);
    }
}
