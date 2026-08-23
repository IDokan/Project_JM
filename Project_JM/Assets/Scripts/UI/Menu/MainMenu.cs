// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MainMenu.cs
// Summary: Main menu controller; handles game start, options, and quit.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : Menu
{
    [Header("Buttons")]
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button statsButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;

    [Header("Refs")]
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private OptionMenu optionMenu;
    [SerializeField] private LeaderboardMenu leaderboardMenu;
    [SerializeField] private StatsMenu statsMenu;
    [SerializeField] private ConfirmationDialog confirmationDialog;

    [Header("Transition")]
    [SerializeField] private float exitFadeDuration = 0.4f;

    [Header("Confirmation icons")]
    [SerializeField] private Image quitConfirmIcon;

    protected override void OnEnable()
    {
        base.OnEnable();

        gameStartButton.onClick.AddListener(OnGameStartClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        statsButton.onClick.AddListener(OnStatsClicked);
        optionButton.onClick.AddListener(OnOptionClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        gameStartButton.onClick.RemoveListener(OnGameStartClicked);
        leaderboardButton.onClick.RemoveListener(OnLeaderboardClicked);
        statsButton.onClick.RemoveListener(OnStatsClicked);
        optionButton.onClick.RemoveListener(OnOptionClicked);
        quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    public override void OnCancel(BaseEventData eventData)
    {
        QuitWithFadeTransition();
    }

    private void OnGameStartClicked()
    {
        mainMenuController.DisableInput();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        SaveDataManager.Instance.PushToSteamCloud();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync("CombatScene");
        loadOp.allowSceneActivation = false;

        canvasGroup.DOFade(0f, exitFadeDuration)
            .SetUpdate(true)
            .OnComplete(() => loadOp.allowSceneActivation = true);
    }

    private void OnLeaderboardClicked()
    {
        leaderboardMenu.Show(leaderboardButton);
    }

    private void OnStatsClicked()
    {
        statsMenu.Show(statsButton);
    }

    private void OnOptionClicked()
    {
        optionMenu.Show(optionButton);
    }

    private void OnQuitClicked()
    {
        QuitWithFadeTransition();
    }

    private void QuitWithFadeTransition()
    {
        confirmationDialog.Show(quitConfirmIcon, () =>
        {
            sceneTransition.FadeAndQuit();
        }, quitButton);
    }
}
