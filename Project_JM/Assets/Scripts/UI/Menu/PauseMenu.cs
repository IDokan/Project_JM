// SPDX-License-Identifier: MIT
// Copyright (c) 03/25/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseMenu.cs
// Summary: A script to perform pause menu actions.

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
        base.OnEnable();
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

    public void OnRestartButtonPressed()
    {
        // @@ TODO: Need to add destructive confirmation.

        Hide();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnOptionButtonPressed()
    {
        optionPanel.Show();
    }

    public void OnHomeButtonPressed()
    {
        Hide();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitButtonPressed()
    {
        Hide();

#if UNITY_EDITOR
Debug.Log("QuitGame called. Quit does not work in Unity Editor.");
#else
        Application.Quit();
#endif
    }
}
