// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/27/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AudioMenu.cs
// Summary: Audio settings menu. Binds Master/BGM/SFX sliders to AudioManager volume API.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class AudioMenu : Menu
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    protected override void OnEnable()
    {
        base.OnEnable();

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    public override void Show(UnityEngine.UI.Selectable returnTo)
    {
        base.Show(returnTo);
        SyncSliders();
    }

    public override void Hide()
    {
        AudioManager.Instance.SaveVolumes();
        base.Hide();
    }

    private void SyncSliders()
    {
        masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetMasterVolume());
        bgmVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetBGMVolume());
        sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());
    }

    private void OnMasterVolumeChanged(float value) => AudioManager.Instance.SetMasterVolume(value);
    private void OnBGMVolumeChanged(float value)    => AudioManager.Instance.SetBGMVolume(value);
    private void OnSFXVolumeChanged(float value)    => AudioManager.Instance.SetSFXVolume(value);
}
