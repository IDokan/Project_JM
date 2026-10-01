// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/27/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: OptionMenu.cs
// Summary: A script to perform option menu actions.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : Menu
{
    [Header("Buttons")]
    [SerializeField] protected Button graphicsSettingButton;
    [SerializeField] protected Button audioSettingButton;
    [SerializeField] protected Button languageSettingButton;

    [Header("Logic related refs")]
    [SerializeField] protected GraphicsMenu graphicsMenu;
    [SerializeField] protected AudioMenu audioMenu;
    [SerializeField] protected LanguageMenu languageMenu;

    protected override void OnEnable()
    {
        base.OnEnable();

        graphicsSettingButton.onClick.AddListener(OnGraphicsButtonClicked);
        audioSettingButton.onClick.AddListener(OnAudioButtonClicked);
        languageSettingButton.onClick.AddListener(OnLanguageButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        graphicsSettingButton.onClick.RemoveListener(OnGraphicsButtonClicked);
        audioSettingButton.onClick.RemoveListener(OnAudioButtonClicked);
        languageSettingButton.onClick.RemoveListener(OnLanguageButtonClicked);
    }

    protected void OnGraphicsButtonClicked()
    {
        audioMenu.Hide();
        languageMenu.Hide();
        graphicsMenu.Show(graphicsSettingButton);
    }

    protected void OnAudioButtonClicked()
    {
        graphicsMenu.Hide();
        languageMenu.Hide();
        audioMenu.Show(audioSettingButton);
    }

    protected void OnLanguageButtonClicked()
    {
        graphicsMenu.Hide();
        audioMenu.Hide();
        languageMenu.Show(languageSettingButton);
    }

}
