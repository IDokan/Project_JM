// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/27/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GraphicsMenu.cs
// Summary: A script to perform graphics menu actions.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GraphicsMenu : Menu
{
    [Header("Graphics Modifiers")]
    [SerializeField] protected Toggle vSyncToggle;
    [SerializeField] protected Toggle fullScreenModeToggle;
    [SerializeField] protected TMP_Dropdown resolutionDropdown;

    protected Resolution[] _availableResolutions;
    protected string _resolutionSignature;

    protected void Awake()
    {
        InitializeResolutionDropdown();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        vSyncToggle.onValueChanged.AddListener(OnVSyncToggled);
        fullScreenModeToggle.onValueChanged.AddListener(OnFullScreenModeToggled);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionValueChanged);

        RefreshResolutionDropdownIfNeeded();
        SyncGraphicsWidgets();
        UpdateResolutionSelectionWithoutNotify();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        vSyncToggle.onValueChanged.RemoveListener(OnVSyncToggled);
        fullScreenModeToggle.onValueChanged.RemoveListener(OnFullScreenModeToggled);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionValueChanged);
    }

    protected void OnVSyncToggled(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
    }

    protected void OnFullScreenModeToggled(bool isOn)
    {
        Screen.fullScreen = isOn;
    }

    protected void OnResolutionValueChanged(int index)
    {
        if (index < 0 || index >= _availableResolutions.Length)
        {
            return;
        }

        Resolution selectedResolution = _availableResolutions[index];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode, selectedResolution.refreshRateRatio);
    }

    protected void SyncGraphicsWidgets()
    {
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
        fullScreenModeToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    protected void InitializeResolutionDropdown()
    {
        _availableResolutions = Screen.resolutions;

        List<string> options = new List<string>(_availableResolutions.Length);

        for (int i = 0; i < _availableResolutions.Length; ++i)
        {
            Resolution resolution = _availableResolutions[i];

            options.Add($"{resolution.width} x {resolution.height} ({resolution.refreshRateRatio.value:0}Hz)");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        UpdateResolutionSelectionWithoutNotify();

        // Memorize signature when ropdown initialized.
        _resolutionSignature = BuildResolutionSignature(_availableResolutions);
    }

    protected void UpdateResolutionSelectionWithoutNotify()
    {
        if (_availableResolutions == null || _availableResolutions.Length == 0)
        {
            return;
        }

        int currentIndex = FindCurrentResolutionIndex();
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();
    }

    protected int FindCurrentResolutionIndex()
    {
        Resolution currentResolution = Screen.currentResolution;

        for (int i = 0; i < _availableResolutions.Length; ++i)
        {
            Resolution resolution = _availableResolutions[i];

            if (resolution.width == currentResolution.width &&
                resolution.height == currentResolution.height &&
                Mathf.Approximately((float)resolution.refreshRateRatio.value, (float)currentResolution.refreshRateRatio.value))
            {

                return i;
            }
        }

        return 0;
    }

    // -------- Below functions are for efficient drop down construction
    protected string BuildResolutionSignature(Resolution[] resolutions)
    {
        if (resolutions == null || resolutions.Length <= 0)
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(resolutions.Length * 16);

        for (int i = 0; i < resolutions.Length; ++i)
        {
            Resolution resolution = resolutions[i];

            builder.Append(resolution.width).Append('x').Append(resolution.height).Append('@').Append(resolution.refreshRateRatio.value).Append(';');
        }

        return builder.ToString();
    }

    protected void RefreshResolutionDropdownIfNeeded()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        Resolution[] currentResolutions = Screen.resolutions;

        string currentSignature = BuildResolutionSignature(currentResolutions);
        if (_availableResolutions == null || _availableResolutions.Length == 0 ||
            currentSignature != _resolutionSignature)
        {
            InitializeResolutionDropdown();
        }
    }


    // -------- End of functions for efficient graphics drop down.
}
