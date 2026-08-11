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
        audioMenu.Hide();
        graphicsMenu.Show(graphicsSettingButton);
    }

    protected void OnAudioButtonClicked()
    {
        graphicsMenu.Hide();
        audioMenu.Show(audioSettingButton);
    }

    public override Selectable GetFirstSelectable()
    {
        bool isPortrait = Screen.height > Screen.width;
        return isPortrait ? audioSettingButton : graphicsSettingButton;
    }
}
