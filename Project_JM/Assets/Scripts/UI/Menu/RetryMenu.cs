// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RetryMenu.cs
// Summary: Controls the retry menu shown on game over: restart immediately, or navigate
//          to main menu / quit with a confirmation dialog.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RetryMenu : Menu
{
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Logic related refs")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private ConfirmationDialog confirmationDialog;
    [SerializeField] private SceneTransition sceneTransition;

    [Header("Confirmation icons")]
    [SerializeField] private Image mainMenuConfirmIcon;
    [SerializeField] private Image quitConfirmIcon;

    public override void Show(Selectable returnTo = null)
    {
        playerInput.SwitchToUIMap();
        base.Show(returnTo);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        restartButton.onClick.AddListener(OnRestartButtonPressed);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonPressed);
        quitButton.onClick.RemoveListener(OnQuitButtonPressed);
    }

    private void OnRestartButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenuButtonPressed()
    {
        confirmationDialog.Show(mainMenuConfirmIcon, () =>
        {
            sceneTransition.FadeAndLoad(mainMenuSceneName);
        }, mainMenuButton);
    }

    private void OnQuitButtonPressed()
    {
        confirmationDialog.Show(quitConfirmIcon, () =>
        {
            sceneTransition.FadeAndQuit();
        }, quitButton);
    }
}
