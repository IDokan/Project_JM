// SPDX-License-Identifier: MIT
// Copyright (c) 03/27/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: OptionMenu.cs
// Summary: A script to perform option menu actions.

using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : Menu
{
    [Header("Buttons")]
    [SerializeField] protected Button graphicsSettingButton;
    [SerializeField] protected Button audioSettingButton;

    [Header("Logic related refs")]
    [SerializeField] protected GraphicsMenu graphicsMenu;
    [SerializeField] protected AudioMenu audioMenu;

    protected override void OnEnable()
    {
        base.OnEnable();

        graphicsSettingButton.onClick.AddListener(OnGraphicsButtonClicked);
        audioSettingButton.onClick.AddListener(OnAudioButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        graphicsSettingButton.onClick.RemoveListener(OnGraphicsButtonClicked);
        audioSettingButton.onClick.RemoveListener(OnAudioButtonClicked);
    }

    protected void OnGraphicsButtonClicked()
    {
        graphicsMenu.Show();
        audioMenu.Hide();
    }

    protected void OnAudioButtonClicked()
    {
        graphicsMenu.Hide();
        audioMenu.Show();
    }
}
