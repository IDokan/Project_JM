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
    protected List<Resolution> _uniqueResolutions;
    protected string _resolutionSignature;
    protected bool _initialized;

    public void PreInitialize()
    {
        if (_initialized)
        {
            return;
        }

        InitializeDropdowns();
        _initialized = true;
    }

    protected void Awake()
    {
        PreInitialize();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        vSyncToggle.onValueChanged.AddListener(OnVSyncToggled);
        fullScreenModeToggle.onValueChanged.AddListener(OnFullScreenModeToggled);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionValueChanged);

        RefreshDropdownsIfNeeded();
        SyncGraphicsWidgets();
        UpdateDropdownSelectionWithoutNotify();
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
        if (index < 0 || index >= _uniqueResolutions.Count)
        {
            return;
        }

        ApplyCurrentSettings();
    }

    protected void SyncGraphicsWidgets()
    {
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
        fullScreenModeToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    protected void InitializeDropdowns()
    {
        _availableResolutions = Screen.resolutions;
        _resolutionSignature = BuildResolutionSignature(_availableResolutions);

        BuildUniqueResolutionList();
        PopulateResolutionDropdown();
        UpdateDropdownSelectionWithoutNotify();
    }

    protected void BuildUniqueResolutionList()
    {
        _uniqueResolutions = new List<Resolution>(_availableResolutions.Length);

        for (int i = 0; i < _availableResolutions.Length; ++i)
        {
            Resolution r = _availableResolutions[i];
            if (r.width * 9 != r.height * 16)
            {
                continue;
            }

            _uniqueResolutions.Add(r);
        }
    }

    protected void PopulateResolutionDropdown()
    {
        List<string> options = new List<string>(_uniqueResolutions.Count);

        for (int i = 0; i < _uniqueResolutions.Count; ++i)
        {
            options.Add($"{_uniqueResolutions[i].width} x {_uniqueResolutions[i].height} @ {_uniqueResolutions[i].refreshRateRatio.value:0.##}Hz");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    protected void ApplyCurrentSettings()
    {
        int resIndex = resolutionDropdown.value;
        if (resIndex < 0 || resIndex >= _uniqueResolutions.Count)
        {
            return;
        }

        Resolution selected = _uniqueResolutions[resIndex];
        Screen.SetResolution(selected.width, selected.height, Screen.fullScreenMode, selected.refreshRateRatio);
    }

    protected void UpdateDropdownSelectionWithoutNotify()
    {
        if (_availableResolutions == null || _availableResolutions.Length == 0 ||
            _uniqueResolutions == null || _uniqueResolutions.Count == 0)
        {
            return;
        }

        int resIndex = FindCurrentResolutionIndex();
        resolutionDropdown.SetValueWithoutNotify(resIndex);
        resolutionDropdown.RefreshShownValue();
    }

    protected int FindCurrentResolutionIndex()
    {
        Resolution current = Screen.currentResolution;

        for (int i = 0; i < _uniqueResolutions.Count; ++i)
        {
            Resolution r = _uniqueResolutions[i];
            if (r.width == current.width && r.height == current.height &&
                Mathf.Approximately((float)r.refreshRateRatio.value, (float)current.refreshRateRatio.value))
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

    protected void RefreshDropdownsIfNeeded()
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
            InitializeDropdowns();
        }
    }


    // -------- End of functions for efficient graphics drop down.
}
