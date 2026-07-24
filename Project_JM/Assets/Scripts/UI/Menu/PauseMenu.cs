// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/25/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseMenu.cs
// Summary: A script to perform pause menu actions.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : Menu
{
    [Header("Buttons")]
    [SerializeField] protected Button restartButton;
    [SerializeField] protected Button optionButton;
    [SerializeField] protected Button homeButton;
    [SerializeField] protected Button quitButton;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Logic related refs")]
    [SerializeField] protected string mainMenuSceneName = "MainMenu";
    [SerializeField] protected OptionMenu optionPanel;
    [SerializeField] private ConfirmationDialog confirmationDialog;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private BoardManager boardManager;

    [Header("Confirmation icons")]
    [SerializeField] private Image restartConfirmIcon;
    [SerializeField] private Image homeConfirmIcon;
    [SerializeField] private Image quitConfirmIcon;

    protected override void OnEnable()
    {
        base.OnEnable();
        restartButton.onClick.AddListener(OnRestartButtonPressed);
        optionButton.onClick.AddListener(OnOptionButtonPressed);
        homeButton.onClick.AddListener(OnHomeButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
        SteamManager.OnOverlayActivated += OnSteamOverlayActivated;
        playerInput.onDeviceLost += OnDeviceLost;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        optionButton.onClick.RemoveListener(OnOptionButtonPressed);
        homeButton.onClick.RemoveListener(OnHomeButtonPressed);
        quitButton.onClick.RemoveListener(OnQuitButtonPressed);
        SteamManager.OnOverlayActivated -= OnSteamOverlayActivated;
        playerInput.onDeviceLost -= OnDeviceLost;
    }

    public bool IsPaused { get; private set; }

    protected void OnSteamOverlayActivated(bool isActive)
    {
        if (isActive && !IsPaused)
        {
            Show();
        }
    }

    protected void OnDeviceLost(PlayerInput player)
    {
        if (!IsPaused)
        {
            Show();
        }
    }

    public override void Show(Selectable returnTo = null)
    {
        IsPaused = true;
        playerInput.SwitchToUIMap();
        base.Show(returnTo);
        GlobalTimeManager.Instance.PauseTimeScale();
        AudioManager.Instance.PauseScaledSFX();
        boardManager.SetGemsVisible(false);
    }

    public override void Hide()
    {
        IsPaused = false;
        playerInput.SwitchToGameplayMap();
        base.Hide();
        GlobalTimeManager.Instance.RestoreTimeScaleFromPause();
        AudioManager.Instance.ResumeScaledSFX();
        boardManager.SetGemsVisible(true);
    }

    protected void OnRestartButtonPressed()
    {
        confirmationDialog.Show(restartConfirmIcon, () =>
        {
            Hide();
            GlobalTimeManager.Instance.RestoreTimeScaleExitCombatScene();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }, restartButton);
    }

    protected void OnOptionButtonPressed()
    {
        optionPanel.Show(optionButton);
    }

    protected void OnHomeButtonPressed()
    {
        confirmationDialog.Show(homeConfirmIcon, () =>
        {
            Hide();
            GlobalTimeManager.Instance.RestoreTimeScaleExitCombatScene();
            sceneTransition.FadeAndLoad(mainMenuSceneName);
        }, homeButton);
    }

    protected void OnQuitButtonPressed()
    {
        confirmationDialog.Show(quitConfirmIcon, () =>
        {
            sceneTransition.FadeAndQuit();
        }, quitButton);
    }
}
