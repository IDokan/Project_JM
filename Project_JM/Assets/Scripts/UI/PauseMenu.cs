// SPDX-License-Identifier: MIT
// Copyright (c) 03/25/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseMenu.cs
// Summary: A script to perform pause menu actions.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] protected Button restartButton;
    [SerializeField] protected Button optionButton;
    [SerializeField] protected Button homeButton;
    [SerializeField] protected Button quitButton;

    [Header("Logic related refs")]
    [SerializeField] protected CombatIntroController combatIntroController;
    [SerializeField] protected string mainMenuSceneName = "MainMenu";
    [SerializeField] protected GameObject optionPanel;
    [SerializeField] protected PauseManager pauseManager;

    protected void Awake()
    {
        restartButton.onClick.AddListener(OnRestartButtonPressed);
        optionButton.onClick.AddListener(OnOptionButtonPressed);
        homeButton.onClick.AddListener(OnHomeButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    protected void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        optionButton.onClick.RemoveListener(OnOptionButtonPressed);
        homeButton.onClick.RemoveListener(OnHomeButtonPressed);
        quitButton.onClick.RemoveListener(OnQuitButtonPressed);
    }

    public void OnRestartButtonPressed()
    {
        // @@ TODO: Need to add destructive confirmation.

        GlobalTimeManager.Instance.RestoreTimeScaleExitCombatScene();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnOptionButtonPressed()
    {

    }

    public void OnHomeButtonPressed()
    {
        GlobalTimeManager.Instance.RestoreTimeScaleExitCombatScene();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitButtonPressed()
    {
        GlobalTimeManager.Instance.RestoreTimeScaleExitCombatScene();

#if UNITY_EDITOR
Debug.Log("QuitGame called. Quit does not work in Unity Editor.");
#else
        Application.Quit();
#endif
    }
}
