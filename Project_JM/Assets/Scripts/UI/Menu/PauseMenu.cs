// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/25/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseMenu.cs
// Summary: A script to perform pause menu actions.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : Menu
{
    [Header("Buttons")]
    [SerializeField] protected Button restartButton;
    [SerializeField] protected Button optionButton;
    [SerializeField] protected Button homeButton;
    [SerializeField] protected Button quitButton;

    [Header("Logic related refs")]
    [SerializeField] protected CombatIntroController combatIntroController;
    [SerializeField] protected string mainMenuSceneName = "MainMenu";
    [SerializeField] protected OptionMenu optionPanel;
    [SerializeField] private ConfirmationDialog confirmationDialog;

    [Header("Confirmation icons")]
    [SerializeField] private Sprite restartConfirmIcon;
    [SerializeField] private Sprite homeConfirmIcon;
    [SerializeField] private Sprite quitConfirmIcon;

    protected override void OnEnable()
    {
        base.OnEnable();
        restartButton.onClick.AddListener(OnRestartButtonPressed);
        optionButton.onClick.AddListener(OnOptionButtonPressed);
        homeButton.onClick.AddListener(OnHomeButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        optionButton.onClick.RemoveListener(OnOptionButtonPressed);
        homeButton.onClick.RemoveListener(OnHomeButtonPressed);
        quitButton.onClick.RemoveListener(OnQuitButtonPressed);
    }

    public override void Show()
    {
        base.Show();

        GlobalTimeManager.Instance.PauseTimeScale();
    }

    public override void Hide()
    {
        base.Hide();
        GlobalTimeManager.Instance.RestoreTimeScaleExitCombatScene();
    }

    protected void OnRestartButtonPressed()
    {
        confirmationDialog.Show(restartConfirmIcon, () =>
        {
            Hide();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        });
    }

    protected void OnOptionButtonPressed()
    {
        optionPanel.Show();
    }

    protected void OnHomeButtonPressed()
    {
        confirmationDialog.Show(homeConfirmIcon, () =>
        {
            Hide();
            SceneManager.LoadScene(mainMenuSceneName);
        });
    }

    protected void OnQuitButtonPressed()
    {
        confirmationDialog.Show(quitConfirmIcon, () =>
        {
            Hide();
#if UNITY_EDITOR
            Debug.Log("QuitGame called. Quit does not work in Unity Editor.");
#else
            Application.Quit();
#endif
        });
    }
}
