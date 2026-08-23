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
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private Button scoreButton;
    [SerializeField] private LeaderboardMenu leaderboardMenu;

    [Header("Confirmation icons")]
    [SerializeField] private Image mainMenuConfirmIcon;
    [SerializeField] private Image quitConfirmIcon;

    [Header("Icon selector")]
    [SerializeField] private LeaderboardIconOpener[] iconOpeners;

    private int _cachedScore;

    public override void Show(Selectable returnTo = null)
    {
        playerInput.SwitchToUIMap();

        if (ScoreManager.Instance != null)
        {
            _cachedScore = ScoreManager.Instance.TotalScore;
            if (scoreText != null)
            {
                scoreText.text = _cachedScore.ToString();
            }
            if (rankText != null)
            {
                int rank = ComputeLocalRank(_cachedScore);
                rankText.text = rank <= LeaderboardManager.MaxScoresPerPlayer ? $"#{rank}" : "#--";
            }
        }

        for (int i = 0; i < iconOpeners.Length; i++)
        {
            iconOpeners[i].Init(i, PlayerPrefs.GetInt(LeaderboardIconPrefs.Keys[i], LeaderboardIconPrefs.Defaults[i]));
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
    }

    protected override void OnDisable()
    {
        PlayerPrefs.Save();
        base.OnDisable();
        restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonPressed);
        quitButton.onClick.RemoveListener(OnQuitButtonPressed);
        scoreButton.onClick.RemoveListener(OnScoreButtonPressed);
    }

    public override void OnCancel(BaseEventData eventData) 
    {
        // Do nothing becuase retry menu will not be hidden.
    }

    private void OnScoreButtonPressed()
    {
        leaderboardMenu.ShowWithLiveEntry(scoreButton, _cachedScore, iconOpeners[0].SelectedId, iconOpeners[1].SelectedId, iconOpeners[2].SelectedId);
    }

    private int ComputeLocalRank(int score)
    {
        if (LeaderboardManager.Instance == null) { return 1; }

        int rank = 1;
        foreach (LeaderboardEntry entry in LeaderboardManager.Instance.GetMyScores())
        {
            if (entry.score > score) { rank++; }
        }
        return rank;
    }

    private void SubmitScore()
    {
        LeaderboardManager.Instance.SubmitScore(_cachedScore, iconOpeners[0].SelectedId, iconOpeners[1].SelectedId, iconOpeners[2].SelectedId);
    }

    private void OnRestartButtonPressed()
    {
        SubmitScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenuButtonPressed()
    {
        confirmationDialog.Show(mainMenuConfirmIcon, () =>
        {
            SubmitScore();
            SaveDataManager.Instance.PushToSteamCloud();
            sceneTransition.FadeAndLoad(mainMenuSceneName);
        }, mainMenuButton);
    }

    private void OnQuitButtonPressed()
    {
        confirmationDialog.Show(quitConfirmIcon, () =>
        {
            SubmitScore();
            sceneTransition.FadeAndQuit();
        }, quitButton);
    }
}
