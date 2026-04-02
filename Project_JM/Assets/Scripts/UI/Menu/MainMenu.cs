// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MainMenu.cs
// Summary: Main menu controller; handles game start, options, and quit.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : Menu
{
    [Header("Buttons")]
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;

    [Header("Refs")]
    [SerializeField] private OptionMenu optionMenu;
    [SerializeField] private ConfirmationDialog confirmationDialog;

    [Header("Confirmation icons")]
    [SerializeField] private Image quitConfirmIcon;

    protected override void OnEnable()
    {
        base.OnEnable();

        gameStartButton.onClick.AddListener(OnGameStartClicked);
        optionButton.onClick.AddListener(OnOptionClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        gameStartButton.onClick.RemoveListener(OnGameStartClicked);
        optionButton.onClick.RemoveListener(OnOptionClicked);
        quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnGameStartClicked()
    {
        SceneManager.LoadScene("CombatScene");
    }

    private void OnOptionClicked()
    {
        optionMenu.Show(optionButton);
    }

    private void OnQuitClicked()
    {
        QuitWithConfirmation(confirmationDialog, quitConfirmIcon, quitButton);
    }
}
