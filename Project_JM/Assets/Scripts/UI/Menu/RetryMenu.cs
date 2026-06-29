// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RetryMenu.cs
// Summary: Controls the retry menu shown on game over: restart immediately, or navigate
//          to main menu / quit with a confirmation dialog.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button scoreButton;
    [SerializeField] private LeaderboardMenu leaderboardMenu;

    [Header("Confirmation icons")]
    [SerializeField] private Image mainMenuConfirmIcon;
    [SerializeField] private Image quitConfirmIcon;

    [Header("Icon selector")]
    [SerializeField] private IconSelectorMenu iconSelectorMenu;
    [SerializeField] private Button[] iconButtons;
    [SerializeField] private Image[] iconButtonImages;
    [SerializeField] private LeaderboardIconContainer iconContainer;

    private int[] _selectedIconIds = new int[] { 0, 0, 0 };

    public override void Show(Selectable returnTo = null)
    {
        playerInput.SwitchToUIMap();

        if (scoreText != null && ScoreManager.Instance != null)
        {
            scoreText.text = ScoreManager.Instance.TotalScore.ToString();
        }

        base.Show(returnTo);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        restartButton.onClick.AddListener(OnRestartButtonPressed);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
        scoreButton.onClick.AddListener(OnScoreButtonPressed);
        iconButtons[0].onClick.AddListener(OnIconButton0Pressed);
        iconButtons[1].onClick.AddListener(OnIconButton1Pressed);
        iconButtons[2].onClick.AddListener(OnIconButton2Pressed);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonPressed);
        quitButton.onClick.RemoveListener(OnQuitButtonPressed);
        scoreButton.onClick.RemoveListener(OnScoreButtonPressed);
        iconButtons[0].onClick.RemoveListener(OnIconButton0Pressed);
        iconButtons[1].onClick.RemoveListener(OnIconButton1Pressed);
        iconButtons[2].onClick.RemoveListener(OnIconButton2Pressed);
    }

    public override void OnCancel(BaseEventData eventData) { }

    private void OnScoreButtonPressed()
    {
        leaderboardMenu.Show(scoreButton);
    }

    private void OnIconButton0Pressed() => OpenIconSelector(0);
    private void OnIconButton1Pressed() => OpenIconSelector(1);
    private void OnIconButton2Pressed() => OpenIconSelector(2);

    private void OpenIconSelector(int slot)
    {
        iconSelectorMenu.Open(iconButtons[slot], _selectedIconIds[slot], id => OnIconSelected(slot, id));
    }

    private void OnIconSelected(int slot, int id)
    {
        _selectedIconIds[slot] = id;
        iconButtonImages[slot].sprite = iconContainer.GetSprite(id);
        iconButtonImages[slot].SetNativeSize();
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
